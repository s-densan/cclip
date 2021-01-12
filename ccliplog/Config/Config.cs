using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccliplog.Config
{
    class ConfigData
    {
        public string outPath { get; set; } = "";

        public string ToJson()
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this);
            return json;
        }
    }
    class Config
    {
        readonly string filePath = "";
        public ConfigData data;

        public Config(string filePath)
        {
            this.filePath = filePath;
            this.data = this.Load();
            this.Save();
            
        }
        public void Save()
        {
            var dir = Directory.GetParent(this.filePath)?.Parent;
            if(dir == null)
            {
                return;
            }
            if (!dir.Exists)
            {
                dir.Create();

            }
            File.WriteAllText(filePath, data.ToJson());
        }
        public ConfigData Load()
        {
            if (File.Exists(filePath))
            {
                var dataText = File.ReadAllText(filePath);
                if (dataText != null)
                {
                    var data = System.Text.Json.JsonSerializer.Deserialize<ConfigData>(dataText)!;
                    return data;
                }
                else
                {
                    return new ConfigData();
                }
            }
            else
            {
                var data = new ConfigData
                {
                    outPath = Directory.GetParent(this.filePath)?.FullName ?? ""
                };
                return data;
            }
        }
    }
}
