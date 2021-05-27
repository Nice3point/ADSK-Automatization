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
            const string addinSubFolder = ".RevitAddins";
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var addinName = Assembly.GetExecutingAssembly().GetName().Name;
            return Path.Combine(userProfile, addinSubFolder, addinName);
        }

        private static string GetConfigurationFilePath()
        {
            const string fileName = "config.ini";
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

            WriteKey(nameof(Application), "Название вкладки на ленте", "", false);
            WriteComment(nameof(Application), "Название вкладки на ленте",
                "Если значение не задано, используется вкладка \"Надстройки\"");
            WriteKey(nameof(AutoNumerate), "Показывать кнопку", "true", false);
            WriteKey(nameof(CheckAdsk), "Показывать кнопку", "true", false);
            WriteKey(nameof(CopyAdsk), "Показывать кнопку", "true", false);
            WriteKey(nameof(CopyAdsk), "Путь к файлу настроек", "", false);
            WriteComment(nameof(CopyAdsk), "Путь к файлу настроек", "Автогенерируемый .json файл");
            WriteKey(nameof(CreateDuctSystemViews), "Показывать кнопку", "true", false);
            WriteKey(nameof(CreatePipeSystemViews), "Показывать кнопку", "true", false);
            WriteKey(nameof(CreateSpaces), "Показывать кнопку", "true", false);
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