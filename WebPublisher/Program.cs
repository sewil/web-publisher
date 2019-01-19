using DamienG.Security.Cryptography;
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

            FTPClient ftpClient = new FTPClient(Settings.Default.ftpHost, Settings.Default.ftpUsername, Settings.Default.ftpPassword);

            // Upload attachments
            for (int i = 0; i < config.Include.Count; i++)
            {
                ConfigAttachment attachment = config.Include[i];
                if (!string.IsNullOrWhiteSpace(attachment.File) && File.Exists(attachment.File))
                {
                    if (!string.IsNullOrWhiteSpace(attachment.EntryLinkPattern))
                    {
                        Match fileMatch = Regex.Match(entryText, attachment.EntryLinkPattern);
                        string fileHash = GetFileHash(attachment.File);
                        string remoteDirectory = Path.Combine(config.UploadDirectory, attachment.Directory ?? "");
                        string remoteFileName = Path.GetFileName(attachment.File);
                        entryText = ReplaceVHash(fileMatch, new FileInfo(attachment.File), fileHash, entryText);
                        ftpClient.Upload(attachment.File, remoteDirectory, remoteFileName, FileExistsAction.Overwrite);
                    }
                }
            }

            // Update entry file with new hashes
            if (entryText != originalEntryText)
            {
                File.WriteAllText(config.Entry, entryText);
            }

            // Upload entry file
            ftpClient.Upload(config.Entry, config.UploadDirectory, Path.GetFileName(config.Entry), FileExistsAction.Overwrite);
        }

        private static string ReplaceVHash(Match match, FileInfo file, string hash, string indexText)
        {
            int groupIndex = match.Groups[1].Index;
            int groupLength = match.Groups[1].Length;
            string href = file.Name + "?v=" + hash;
            indexText = indexText.Remove(groupIndex, groupLength);
            indexText = indexText.Insert(groupIndex, href);
            return indexText;
        }

        private static string GetFileHash(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                string fileHash = "";
                byte[] computedBytes = new Crc32().ComputeHash(buffer);
                foreach (byte b in computedBytes)
                {
                    fileHash += b.ToString("x2").ToUpper();
                }
                return fileHash;
            }
        }
    }
}
