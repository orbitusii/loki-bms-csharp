﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;

namespace loki_bms_csharp.Settings
{
    public abstract class SerializableSettings<T>
    {
        public static T? LoadFromFile(string filename)
        {
            if(File.Exists(filename))
            {
                using (FileStream stream = new(filename, FileMode.Open))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(T));
                    var result = (T)ser.Deserialize(stream);

                    if (result is SerializableSettings<T>) return result;
                }
            }
            return default;
        }

        public static void SaveToFile (string filename, T settings)
        {
            if (!File.Exists(filename)) File.Create(filename).Close();

            using (FileStream stream = new(filename, FileMode.Truncate))
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                ser.Serialize(stream, settings);
            }
        }
    }
}
