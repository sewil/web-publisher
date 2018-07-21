# Features
- For the entry file, if configured, applies a query on each attachment URL with the value of the attachment file's CRC-32 checksum.
- Uploads and overwrites existing web files to the given host and directory through FTP.

# Configuration
## App
In the app config file `WebPublisher.exe.config`, you can configure the name of the project configuration file name along with the FTP Server credentials. Example:
```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <configSections>
        <sectionGroup name="userSettings" type="System.Configuration.UserSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" >
            <section name="WebPublisher.Settings" type="System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" allowExeDefinition="MachineToLocalUser" requirePermission="false" />
        </sectionGroup>
        <sectionGroup name="applicationSettings" type="System.Configuration.ApplicationSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" >
            <section name="WebPublisher.Settings" type="System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
        </sectionGroup>
    </configSections>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5.1" />
    </startup>
    <userSettings>
        <WebPublisher.Settings>
        </WebPublisher.Settings>
    </userSettings>
    <applicationSettings>
        <WebPublisher.Settings>
            <setting name="configFilename" serializeAs="String">
                <value>wpconfig</value>
            </setting>
            <setting name="ftpUsername" serializeAs="String">
                <value>jonas</value>
            </setting>
            <setting name="ftpPassword" serializeAs="String">
                <value>badpassword123</value>
            </setting>
            <setting name="ftpHost" serializeAs="String">
                <value>ftp.example.com</value>
            </setting>
        </WebPublisher.Settings>
    </applicationSettings>
</configuration>
```
## Project
In the project configuration file, which should be located in your web project root, you can configure the name of your entry file, the FTP upload directory path, and other files to include in the upload. To generate an example configuration file, type `webpublisher init` in your project root. Example:
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

- entry: The entry file, preferably your web index file.
- uploadDirectory: The relative FTP directory where the files will be uploaded.
- include: Array of files to include in the upload.
  - file: The local path of the file to upload.
  - entryLinkPattern (Optional): If the entry file references this file, this regex pattern will be used to apply the CRC-32 checksum of this file as a query to the reference URL.

# Technologies
- [Microsoft .NET Framework 4.5.1](https://www.microsoft.com/sv-se/download/details.aspx?id=40779)

# Dependencies
- [crcsharp](https://github.com/sewil/crcsharp)
- [ftpclient-csharp](https://github.com/sewil/ftpclient-csharp)
