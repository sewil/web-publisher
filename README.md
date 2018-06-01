# Features
- Applies a CRC-32 checksum to the `v`-parameter for each matching web file in the index file.
- Uploads and overwrites existing web files to the given host and directory using FTP.
- Stores the host configuration credentials in the user's AppData.

# Technologies
- [Microsoft .NET Framework 4.5.1](https://www.microsoft.com/sv-se/download/details.aspx?id=40779)

# Dependencies
- [FileTools.NET](https://github.com/sewil/FileTools.NET)
- [Network.NET](https://github.com/sewil/Network.NET)
  - [Renci.SshNet.Async@1.2.0](https://github.com/JohnTheGr8/Renci.SshNet.Async)
  - [SSH.NET@2016.0.0](https://github.com/sshnet/SSH.NET)
