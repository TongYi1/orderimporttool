using MS360.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DistributeOrderImportTool.BizProcessor
{
    public class ConfigDataProcessor
    {
        static string configFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration");
        static string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "configdata.xml");
        
        public static ConfigData GetConfigData()
        {
            ConfigData config = null;
            if (File.Exists(configFilePath))
            {
                config = SerializeHelper.LoadFromXml<ConfigData>(configFilePath);                
            }
            return config;
        }

        public static void SaveConfig(ConfigData config)
        {
            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }
            SerializeHelper.SaveToXml(configFilePath, config);
        }
    }
}
