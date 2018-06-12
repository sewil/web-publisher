using FileTools.NET.Extensions;
using Network.NET.Clients;
using Network.NET.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace WebPublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.configFilename))
            {
                throw new Exception("Config file is not configured.");
            }
            string configFile = Settings.Default.configFilename + ".json";
            if (args.Length == 1 && args[0].ToLower() == "init")
            {
                string json = JsonConvert.SerializeObject(Config.Default, Formatting.Indented);
                File.WriteAllText(configFile, json);
                Environment.Exit(0);
            }
            if (!File.Exists(configFile))
            {
                throw new Exception("Config file not found. Type \"webpublisher init\" to create one.");
            }
            string configContents = File.ReadAllText(configFile);
            Config config = JsonConvert.DeserializeObject<Config>(configContents);
            if (string.IsNullOrWhiteSpace(Settings.Default.ftpHost))
            {
                throw new Exception("FTP host is not configured.");
            }
            if (string.IsNullOrWhiteSpace(Settings.Default.ftpUsername))
            {
                throw new Exception("FTP username is not configured.");
            }
            if (string.IsNullOrWhiteSpace(Settings.Default.ftpPassword))
            {
                throw new Exception("FTP password is not configured.");
            }
            string originalEntryText = File.ReadAllText(config.Entry);
            string entryText = originalEntryText;
            var filesToUpload = new List<string>();
            filesToUpload.Add(config.Entry);
            for (int i = 0; i < config.Include.Count; i++)
            {
                ConfigAttachment attachment = config.Include[i];
                if (attachment.File.HasText() && File.Exists(attachment.File))
                {
                    filesToUpload.Add(attachment.File);
                    if (attachment.EntryLinkPattern.HasText())
                    {
                        FileInfo fileInfo = new FileInfo(attachment.File);
                        Match fileMatch = Regex.Match(entryText, attachment.EntryLinkPattern);
                        string fileHash = fileInfo.GetCRC32();
                        entryText = ReplaceVHash(fileMatch, fileInfo, fileHash, entryText);
                    }
                }
            }
            if (entryText != originalEntryText)
            {
                File.WriteAllText(config.Entry, entryText);
            }
            var ftpClient = new FTPClient(Settings.Default.ftpHost, Settings.Default.ftpUsername, Settings.Default.ftpPassword);
            for(int i = 0; i < filesToUpload.Count; i++)
            {
                string fileToUpload = filesToUpload[i];
                string fileName = Path.GetFileName(fileToUpload);
                string directory = "";
                if (fileToUpload.Contains("/"))
                {
                    directory = fileToUpload.Substring(0, fileToUpload.LastIndexOf('/'));
                }
                ftpClient.Upload(fileToUpload, Path.Combine(config.UploadDirectory, directory), fileName, FileExistsAction.Overwrite);
            }
        }
        static string ReplaceVHash(Match match, FileInfo file, string hash, string indexText)
        {
            int groupIndex = match.Groups[1].Index;
            int groupLength = match.Groups[1].Length;
            string href = file.Name + "?v=" + hash;
            indexText = indexText.Remove(groupIndex, groupLength);
            indexText = indexText.Insert(groupIndex, href);
            return indexText;
        }
    }
}
