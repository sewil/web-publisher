# Features
- Applies a CRC-32 checksum to the `v`-parameter for each matching web file in the index file.
- Uploads and overwrites existing web files to the given host and directory using FTP.
- Stores the host configuration credentials in the user's AppData.

# Configuration
The configuration file is called `wpconfig.json` (The file name can be configured in WebPublisher.exe.config) and consists of the properties entry, uploadDirectory, and the files to include in the upload. To generate an example configuration file, type `webpublisher init` in your project root. Here's an example:
```
{
  "entry": "index.html",
  "uploadDirectory": "/www/example.com/public_html",
  "include": [
    {
      "file": "index.css",
      "entryLinkPattern": "<link rel=\"stylesheet\" href=\"(index\\.css[\\?v=A-Fa-f\\d]*)\" />"
    },
    {
      "file": "index.js",
      "entryLinkPattern": "<script src=\"(index\\.js[\\?v=A-Fa-f\\d]*)\"></script>"
    }
  ]
}
```

- entry: The entry file that may also link to one or several of the included files.
- uploadDirectory: The relative FTP directory where the files will be uploaded.
- include: Array of files to include in the upload.
  - file: The path of the file to upload.
  - entryLinkPattern (Optional): If the entry links to this file, this pattern will be used to replace the link reference in the entry file with the latest file hash.

# Technologies
- [Microsoft .NET Framework 4.5.1](https://www.microsoft.com/sv-se/download/details.aspx?id=40779)

# Dependencies
- [FileTools.NET](https://github.com/sewil/FileTools.NET)
- [Network.NET](https://github.com/sewil/Network.NET)
  - [Renci.SshNet.Async@1.2.0](https://github.com/JohnTheGr8/Renci.SshNet.Async)
  - [SSH.NET@2016.0.0](https://github.com/sshnet/SSH.NET)
