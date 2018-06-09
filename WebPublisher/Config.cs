using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebPublisher
{
    class Config
    {
        public static Config Default
        {
            get
            {
                var config = new Config();
                config.Entry = "index.html";
                config.UploadDirectory = "/www/example.com/public_html";
                config.Include = new List<ConfigAttachment>()
                {
                    new ConfigAttachment { File = "index.css", EntryLinkPattern = @"<link rel=""stylesheet"" href=""(index\.css[\?v=A-Fa-f\d]*)"" />" }
                };
                return config;
            }
        }

        [JsonProperty(PropertyName = "entry")]
        public string Entry { get; set; }

        [JsonProperty(PropertyName = "uploadDirectory")]
        public string UploadDirectory { get; set; }

        [JsonProperty(PropertyName = "include", NullValueHandling = NullValueHandling.Ignore)]
        public IList<ConfigAttachment> Include { get; set; }

        public Config()
        {
        }
    }
    class ConfigAttachment
    {
        [JsonProperty(PropertyName = "file", NullValueHandling = NullValueHandling.Ignore)]
        public string File { get; set; }

        [JsonProperty(PropertyName = "entryLinkPattern", NullValueHandling = NullValueHandling.Ignore)]
        public string EntryLinkPattern { get; set; }
    }
}
