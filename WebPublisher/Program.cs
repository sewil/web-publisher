using FileTools.NET.Extensions;
using Network.NET.Clients;
using Network.NET.Enums;
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
            if (string.IsNullOrWhiteSpace(Settings.Default.index))
            {
                throw new Exception("index file name not set");
            }
            if (string.IsNullOrWhiteSpace(Settings.Default.ftpHost))
            {
                Console.Write("FTP Host: ");
                Settings.Default.ftpHost = Console.ReadLine();
            }
            if (string.IsNullOrWhiteSpace(Settings.Default.ftpUsername))
            {
                Console.Write("FTP Username: ");
                Settings.Default.ftpUsername = Console.ReadLine();
            }
            if (string.IsNullOrWhiteSpace(Settings.Default.ftpPassword))
            {
                Console.Write("FTP Password: ");
                Settings.Default.ftpPassword = Console.ReadLine();
            }
            if (string.IsNullOrWhiteSpace(Settings.Default.uploadDirectory))
            {
                Console.Write("FTP Upload Directory: ");
                Settings.Default.uploadDirectory = Console.ReadLine();
            }
            Settings.Default.Save();
            FileInfo indexFile = new FileInfo(Settings.Default.index);
            string originalIndexText = File.ReadAllText(Settings.Default.index);
            string indexText = originalIndexText;
            var filesToUpload = new List<string>();
            filesToUpload.Add(Settings.Default.index);
            for (int i = 0; i < Settings.Default.files.Count; i++)
            {
                string file = Settings.Default.files[i];
                filesToUpload.Add(file);
                if (Settings.Default.filesRegexes.Count - 1 >= i)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    Match fileMatch = Regex.Match(indexText, Settings.Default.filesRegexes[i]);
                    string fileHash = fileInfo.GetCRC32();
                    indexText = ReplaceVHash(fileMatch, fileInfo, fileHash, indexText);
                }
            }
            if (indexText != originalIndexText)
            {
                File.WriteAllText(Settings.Default.index, indexText);
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
                ftpClient.Upload(fileToUpload, Path.Combine(Settings.Default.uploadDirectory, directory), fileName, FileExistsAction.Overwrite);
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
