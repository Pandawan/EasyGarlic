using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    public static class IO {
        public static string ReadFile(string path)
        {
            // If that file exists
            if (File.Exists(path))
            {
                // Open the file and read it
                string readText = File.ReadAllText(path);

                return readText;
            }

            return null;
        }

        public static T ReadTo<T>(string path)
        {
            string json = ReadFile(path);

            if (String.IsNullOrEmpty(json))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void SaveAsJson(string path, object obj)
        {
            string json = JsonConvert.SerializeObject(obj);

            File.WriteAllText(path, json);
        }
    }
}
