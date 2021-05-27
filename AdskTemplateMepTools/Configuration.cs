using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AdskTemplateMepTools.Commands.AutoNumerate;
using AdskTemplateMepTools.Commands.CheckADSK;
using AdskTemplateMepTools.Commands.CopyADSK.Commands;
using AdskTemplateMepTools.Commands.CreateDuctSystemViews;
using AdskTemplateMepTools.Commands.CreatePipeSystemViews;
using AdskTemplateMepTools.Commands.CreateSpaces;
using IniParser;
using IniParser.Model;

namespace AdskTemplateMepTools
{
    public static class Configuration
    {
        private static FileIniDataParser _iniParser;
        private static IniData _iniData;

        public static void Init()
        {
            _iniParser = new FileIniDataParser();
            _iniParser.Parser.Configuration.CommentString = "//";
            _iniParser.Parser.Configuration.NewLineStr = Environment.NewLine;
            СheckBasicConfiguration();
        }

        public static string GetConfigurationDirectory()
        {
            var addinSubFolder = Resources.Localization.Configuration.PluginFolder;
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var addinName = Assembly.GetExecutingAssembly().GetName().Name;
            return Path.Combine(userProfile, addinSubFolder, addinName);
        }

        private static string GetConfigurationFilePath()
        {
            var fileName = Resources.Localization.Configuration.PluginSettingsFile;
            return Path.Combine(GetConfigurationDirectory(), fileName);
        }

        private static void СheckBasicConfiguration()
        {
            if (!File.Exists($@"{GetConfigurationFilePath()}"))
            {
                Directory.CreateDirectory(GetConfigurationDirectory());
                File.Create($@"{GetConfigurationFilePath()}").Close();
            }

            _iniData = _iniParser.ReadFile(GetConfigurationFilePath());

            WriteKey(nameof(Application), Resources.Localization.Configuration.TabNameKey, "", false);
            WriteComment(nameof(Application), Resources.Localization.Configuration.TabNameKey, Resources.Localization.Configuration.TabNameComment);
            WriteKey(nameof(AutoNumerate), Resources.Localization.Configuration.ShowButtonKey, "true", false);
            WriteKey(nameof(CheckAdsk), Resources.Localization.Configuration.ShowButtonKey, "true", false);
            WriteKey(nameof(CopyAdsk), Resources.Localization.Configuration.ShowButtonKey, "true", false);
            WriteKey(nameof(CopyAdsk), Resources.Localization.CopyAdskCommand.SettingsPathKey, "", false);
            WriteComment(nameof(CopyAdsk), Resources.Localization.CopyAdskCommand.SettingsPathKey, Resources.Localization.CopyAdskCommand.SettingsPathComment);
            WriteKey(nameof(CreateDuctSystemViews), Resources.Localization.Configuration.ShowButtonKey, "true", false);
            WriteKey(nameof(CreatePipeSystemViews), Resources.Localization.Configuration.ShowButtonKey, "true", false);
            WriteKey(nameof(CreateSpaces), Resources.Localization.Configuration.ShowButtonKey, "true", false);
        }

        private static bool KeyExists(string section, string key)
        {
            return _iniData[section].ContainsKey(key);
        }

        public static bool TryReadKey(string section, string key, out string value)
        {
            if (KeyExists(section, key))
            {
                value = _iniData[section][key];
                return true;
            }

            value = "";
            return false;
        }

        public static void WriteKey(string section, string key, string value, bool rewriting = true)
        {
            if (KeyExists(section, key) && rewriting == false) return;
            _iniData[section][key] = value;
            _iniParser.WriteFile($@"{GetConfigurationFilePath()}", _iniData);
        }

        private static void WriteComment(string section, string key, string value, bool rewriting = true)
        {
            if (!KeyExists(section, key)) return;
            if (KeyExists(section, key) && rewriting == false) return;
            _iniData[section].GetKeyData(key).Comments = new List<string> {value};
            _iniParser.WriteFile($@"{GetConfigurationFilePath()}", _iniData);
        }
    }
}