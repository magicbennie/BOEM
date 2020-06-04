using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace BlackOpsErrorMonitor
{
    public class ConfigManager
    {
        private string Path = null;
        private string FileName;
        private string BaseLoc = "";
        private const string Delimiter = "=";
        private readonly Version ConfigManagerVersion = new Version(1, 1, 0, 0);

        private List<ConfigItem> ConfigItems = new List<ConfigItem>();

        public ConfigManager(string location, string fileName)
        {
            BaseLoc = location;
            FileName = fileName;
            Path = location + "\\" + FileName;

            if (!ConfigFileExists())
            {
                CreateDefaultConfig();
            }

            ReadFile();
        }

        public bool ConfigFileExists()
        {
            return File.Exists(Path);
        }

        /// <summary>
        /// Tests if we have the appropriate permissions to create files.
        /// </summary>
        /// <returns>Returns whether we have permissions or not.</returns>
        public bool TestFilePermissions()
        {
            string TestFile = BaseLoc + "\\test.txt";

            try
            {
                if (File.Exists(TestFile))
                {
                    File.Delete(TestFile);
                }

                FileStream Test = File.Create(TestFile);

                Test.Dispose();

                File.Delete(TestFile);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void CreateDefaultConfig()
        {
            //The default settings
            ConfigItems.Add(new ConfigItem("ConfigVersion", ConfigManagerVersion.ToString()));
            ConfigItems.Add(new ConfigItem("RefreshTime", "500"));
            ConfigItems.Add(new ConfigItem("Module_LevelTime", "1"));
            ConfigItems.Add(new ConfigItem("Module_numGEntitiesUsed", "1"));
            ConfigItems.Add(new ConfigItem("Module_numSnapshotEntitiesP1", "1"));
            ConfigItems.Add(new ConfigItem("Module_LastNetSnapEntities", "1"));
            ConfigItems.Add(new ConfigItem("Module_comFrameTime", "1"));

            //Write the file to the disk
            WriteFile();
        }

        /// <summary>
        /// Forcibly saves all settings to the configuration file.
        /// </summary>
        public void SaveConfig()
        {
            WriteFile();
        }

        /// <summary>
        /// Forcibly reloads the configuration file.
        /// </summary>
        public void ReloadConfig()
        {
            ReadFile();
        }

        /// <summary>
        /// Gets the value of the specified setting.
        /// </summary>
        /// <param name="SettingName">The name of the setting to get.</param>
        /// <returns>The settings value, or null if it doesn't exist.</returns>
        public string GetSetting(string SettingName)
        {
            //Input validation
            if (string.IsNullOrEmpty(SettingName))
            {
                throw new ArgumentException("SettingName is null or empty in GetSetting!");
            }

            foreach (ConfigItem CurrentConfigItem in ConfigItems)
            {
                if (SettingName == CurrentConfigItem.Name)
                {
                    return CurrentConfigItem.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Records the provided setting.
        /// </summary>
        /// <param name="SettingName">The name of the setting to record.</param>
        /// <param name="SettingValue">The value of the setting to record.</param>
        public void SetSetting(string SettingName, string SettingValue)
        {
            //Input validation
            if (string.IsNullOrEmpty(SettingName) || string.IsNullOrEmpty(SettingValue))
            {
                throw new ArgumentException("SettingName or SettingValue is null or empty in SetSetting!");
            }

            //Check if it already exists first
            foreach (ConfigItem CurrentConfigItem in ConfigItems)
            {
                if (SettingName == CurrentConfigItem.Name)
                {
                    CurrentConfigItem.Value = SettingValue;

                    //Update config file
                    WriteFile();

                    return;
                }
            }

            //Otherwise, create a new one

            ConfigItems.Add(new ConfigItem(SettingName, SettingValue));

            //Update config file
            WriteFile();
        }

        /// <summary>
        /// Removes a setting from the config file.
        /// </summary>
        /// <param name="SettingName">The name of the setting to remove.</param>
        /// <returns>Returns whether or not there was a setting found and removed.</returns>
        public bool DeleteSetting(string SettingName)
        {
            //Input validation
            if (string.IsNullOrEmpty(SettingName))
            {
                throw new ArgumentException("SettingName is null or empty in DeleteSetting!");
            }

            //Check if it already exists
            for (int i = 0; i < ConfigItems.Count; i++)
            {
                if (SettingName == ConfigItems[i].Name)
                {
                    //Remove it
                    ConfigItems.RemoveAt(i);

                    //Update config file
                    WriteFile();

                    return true;
                }
            }

            //Not found
            return false;
        }

        /// <summary>
        /// Removes a setting from the config file.
        /// </summary>
        /// <param name="SettingName">The name of the setting to remove.</param>
        /// <returns>Returns whether or not there was a setting found and removed.</returns>
        public bool RemoveSetting(string SettingName)
        {
            //Input validation
            if (string.IsNullOrEmpty(SettingName))
            {
                throw new ArgumentException("SettingName is null or empty in RemoveSetting!");
            }

            return DeleteSetting(SettingName);
        }

        public bool ParseStringAsBoolean(string Input)
        {
            if (string.IsNullOrEmpty(Input))
            {
                return false;
            }

            switch (Input.ToLower())
            {
                case "true":
                case "yes":
                case "ye":
                case "chur":
                case "yeah":
                case "yea":
                case "1":
                    return true;
                default:
                    return false;
            }
        }

        private void ReadFile()
        {
            //Check if it even exists
            if (!File.Exists(Path))
            {
                //Create it
                CreateDefaultConfig();

                return;
            }

            string FileContents = File.ReadAllText(Path);

            //Load information
            List<ConfigItem> LoadedConfigItems = new List<ConfigItem>();

            string[] Lines = FileContents.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string CurrentLine in Lines)
            {
                if (CurrentLine == "" || CurrentLine == Environment.NewLine)
                {
                    continue;
                }

                string[] Frags = CurrentLine.Split(new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);

                if (Frags.Length != 2)
                {
                    throw new Exception("Config file corrupt!");
                }

                LoadedConfigItems.Add(new ConfigItem(Frags[0], Frags[1]));
            }

            ConfigItems = LoadedConfigItems;
        }

        private void WriteFile()
        {
            string ToWrite = "";

            foreach (ConfigItem CurrentConfigItem in ConfigItems)
            {
                ToWrite += CurrentConfigItem.Name + Delimiter + CurrentConfigItem.Value + Environment.NewLine;
            }

            File.WriteAllText(Path, ToWrite);
        }
    }

    class ConfigItem
    {
        public string Name;
        public string Value;

        public ConfigItem(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
