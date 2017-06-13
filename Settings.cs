using System;
using Newtonsoft.Json;

namespace SDE_GUI
{
    public class Settings
    {
        const string FileName = @"\SDE_GUI.txt";

        [JsonProperty]
        public string SDEPath { get; set; }
        [JsonProperty]
        public string GDBServerPath { get; set; }
        [JsonProperty]
        public string GDBPath { get; set; }
        [JsonProperty]
        public string CMDPath { get; set; }
        [JsonProperty]
        public string Args { get; set; }
        [JsonProperty]
        public int Selection { get; set; }

        public string Serialize()
        {
            string js = JsonConvert.SerializeObject(this);
            return js;
        }

        static Settings Deserialize(string js)
        {
            return JsonConvert.DeserializeObject<Settings>(js);
        }

        static string GetFullName()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + FileName;
        }

        static public bool Load(out Settings settings)
        {
            bool status = false;
            settings = new Settings();
            try
            {
                string text = System.IO.File.ReadAllText(GetFullName());
                status = true;
                settings = Deserialize(text);
            }
            catch (System.IO.FileNotFoundException)
            {
                
            }

            return status;
        }

        public bool Save()
        {
            bool status = false;
            System.IO.File.WriteAllText(GetFullName(), Serialize());
            return status;
        }

    }
}
