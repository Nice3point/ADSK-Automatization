using System;
using System.Collections.Generic;
using System.IO;
using IniParser;
using IniParser.Model;
using Nice3point.Revit.ADSK.MEP.Commands;
using Nice3point.Revit.ADSK.MEP.Commands.AutoNumerate;
using Nice3point.Revit.ADSK.MEP.Commands.CheckADSK;
using Nice3point.Revit.ADSK.MEP.Commands.CopyADSK;
using Nice3point.Revit.ADSK.MEP.Commands.CreateDuctSystemViews;
using Nice3point.Revit.ADSK.MEP.Commands.CreatePipeSystemViews;
using Nice3point.Revit.ADSK.MEP.Commands.CreateSpaces;

namespace Nice3point.Revit.ADSK.MEP
{
    public static class Configuration
    {
        public static readonly string ConfigurationDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".RevitAddins");

        private static readonly string ConfigurationFile = Path.Combine(ConfigurationDirectory, "config.ini");
        private static FileIniDataParser _iniParser;
        private static IniData _iniData;

        public static void Init()
        {
            _iniParser = new FileIniDataParser();
            _iniParser.Parser.Configuration.CommentString = "//";
            _iniParser.Parser.Configuration.NewLineStr = Environment.NewLine;
            СheckBasicConfiguration();
        }

        private static void СheckBasicConfiguration()
        {
            if (!File.Exists($@"{ConfigurationFile}"))
            {
                Directory.CreateDirectory(ConfigurationDirectory);
                File.Create($@"{ConfigurationFile}").Close();
            }

            _iniData = _iniParser.ReadFile(ConfigurationFile);
            
            WriteKey(nameof(Application), "Ribbon tab name", "", false);
            WriteComment(nameof(Application), "Ribbon tab name",
                "Название вкладки на ленте. Если значение не задано, используется вкладка \"Надстройки\"");
            WriteKey(nameof(AutoNumerate), "Tab visibility", "true", false);
            WriteComment(nameof(AutoNumerate), "Tab visibility", "Отображение вкладки на ленте");
            WriteKey(nameof(CheckAdsk), "Tab visibility", "true", false);
            WriteComment(nameof(CheckAdsk), "Tab visibility", "Отображение вкладки на ленте");
            WriteKey(nameof(CopyAdsk), "Tab visibility", "true", false);
            WriteComment(nameof(CopyAdsk), "Tab visibility", "Отображение вкладки на ленте");
            WriteKey(nameof(CopyAdsk), "Profile path", "", false);
            WriteComment(nameof(CopyAdsk), "Profile path", "Путь к файлу с конфигурацией  копирования");
            WriteKey(nameof(CreateDuctSystemViews), "Tab visibility", "true", false);
            WriteComment(nameof(CreateDuctSystemViews), "Tab visibility", "Отображение вкладки на ленте");
            WriteKey(nameof(CreatePipeSystemViews), "Tab visibility", "true", false);
            WriteComment(nameof(CreatePipeSystemViews), "Tab visibility", "Отображение вкладки на ленте");
            WriteKey(nameof(CreateSpaces), "Tab visibility", "true", false);
            WriteComment(nameof(CreateSpaces), "Tab visibility", "Отображение вкладки на ленте");
        }

        private static bool KeyExists(string section, string key) => _iniData[section].ContainsKey(key);

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
            _iniParser.WriteFile($@"{ConfigurationFile}", _iniData);
        }

        private static void WriteComment(string section, string key, string value, bool rewriting = true)
        {
            if (!KeyExists(section, key)) return;
            if (KeyExists(section, key) && rewriting == false) return;
            _iniData[section].GetKeyData(key).Comments = new List<string> {value};
            _iniParser.WriteFile($@"{ConfigurationFile}", _iniData);
        }
    }
}