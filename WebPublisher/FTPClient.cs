using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace WebPublisher
{
    public class FTPClient
    {
        public delegate void FTPTransferProgressEventHandler(double bytesPerSecond);
        public delegate void FTPMultipleTransferProgressChangedEventHandler(int progress);
        public delegate void FTPTransferCanceledEventHandler(WebException exception);
        private Uri _uri;
        private NetworkCredential _credentials;
        private Stopwatch _stopWatch;

        public FTPClient(string host, string userName, string password)
        {
            _uri = new Uri($"ftp://{host}");
            _credentials = new NetworkCredential(userName, password);
        }

        public void Upload(string localPath, string remoteDirectory, string remoteFileName, FileExistsAction fileExistsAction)
        {
            if (fileExistsAction == FileExistsAction.Overwrite)
            {
                try
                {
                    Delete(Path.Combine(remoteDirectory, remoteFileName));
                }
                catch { }
            }
            Upload(localPath, remoteDirectory, remoteFileName, null, null, null);
        }

        public void Upload(string localPath, string directory, string fileName, FTPTransferProgressEventHandler progressChanged, FTPTransferCanceledEventHandler uploadCanceled, CancellationTokenSource cts)
        {
            try
            {
                Upload(localPath, directory, fileName, progressChanged, cts);
            }
            catch (WebException e)
            {
                uploadCanceled?.Invoke(e);
            }
        }

        public byte[] Upload(string localPath, string directory, string fileName, FTPTransferProgressEventHandler progressChanged, CancellationTokenSource cts)
        {
            using (WebClient client = new WebClient())
            {
                client.Credentials = _credentials;
                client.UploadProgressChanged += (sender, e) =>
                {
                    double seconds = _stopWatch.ElapsedMilliseconds / 1000;
                    double bps = 0;
                    if (seconds > 0)
                    {
                        bps = e.BytesSent / seconds;
                    }
                    progressChanged?.Invoke(bps);
                };
                cts?.Token.Register(client.CancelAsync);

                try
                {
                    GetBytes(WebRequestMethods.Ftp.MakeDirectory, directory);
                }
                catch { }

                string url = $"{_uri.ToString()}{directory}{(string.IsNullOrWhiteSpace(fileName) ? "" : "/" + fileName)}".Replace("//", "/").Replace("ftp:/", "ftp://");
                Uri uri = new Uri(url);
                _stopWatch = Stopwatch.StartNew();
                byte[] task = client.UploadFileTaskAsync(uri, "STOR", localPath.ToString()).GetAwaiter().GetResult();
                _stopWatch.Stop();
                return task;
            }
        }

        public void Delete(string path)
        {
            var response = GetResponse(WebRequestMethods.Ftp.DeleteFile, path);
            if (response.StatusCode != FtpStatusCode.FileActionOK)
            {
                throw new Exception(response.StatusDescription);
            }
        }

        public FtpWebResponse GetResponse(string method, string path)
        {
            if (!Regex.IsMatch(path, "[^\\/]"))
            {
                path = "";
            }
            while (Regex.IsMatch(path, "(\\/\\/)"))
            {
                path = Regex.Replace(path, "(\\/\\/)", "/");
            }
            if (path.StartsWith("/") && path.Length >= 2)
            {
                path = path.Substring(1);
            }

            Uri uri = new Uri($"{_uri.ToString()}{path}");
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.Credentials = _credentials;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            return response;
        }


        public IList<string> GetLines(string method, string path)
        {
            IList<string> lines = new List<string>();

            FtpWebResponse response = GetResponse(method, path);

            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    lines.Add(line);
                }

                return lines;
            }
        }

        public byte[] GetBytes(string method, string path)
        {
            FtpWebResponse response = GetResponse(method, path);

            using (Stream stream = response.GetResponseStream())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count = 0;
                do
                {
                    byte[] block = new byte[1024];
                    count = stream.Read(block, 0, 1024);
                    memoryStream.Write(block, 0, count);
                } while (stream.CanRead && count > 0);

                return memoryStream.ToArray();
            }
        }
    }

    public struct DirectoryDetails
    {
        public bool IsDirectory
        {
            get
            {
                return Permissions.StartsWith("d");
            }
        }
        public bool IsLink
        {
            get
            {
                return Permissions.StartsWith("l");
            }
        }
        public string Permissions { get; set; }
        public DateTime DateModified { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public string Name { get; set; }
    }

    public enum FileExistsAction
    {
        DoNothing,
        Overwrite
    }
}
