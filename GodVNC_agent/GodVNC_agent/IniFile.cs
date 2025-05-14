using System;
using System.Collections.Generic;
using System.IO;

namespace IniReader
{
    public class IniFile
    {
        private string _filePath;
        private Dictionary<string, Dictionary<string, string>> _sections;

        public IniFile(string filePath)
        {
            _filePath = filePath;
            _sections = new Dictionary<string, Dictionary<string, string>>();
            ReadFile();
        }

        // Чтение файла и разбор его на секции и ключи/значения
        private void ReadFile()
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException("Файл config.ini не найден. Создайте его на основе config.example.ini.", _filePath);
            }

            string[] lines = File.ReadAllLines(_filePath);
            string currentSection = string.Empty;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                // Пропускаем пустые строки и комментарии
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";"))
                {
                    continue;
                }

                // Если строка начинается с [ секции ], то начинаем новую секцию
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    _sections[currentSection] = new Dictionary<string, string>();
                }
                else
                {
                    // Разделение ключ=значение
                    string[] keyValue = trimmedLine.Split(new char[] { '=' }, 2);

                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim();
                        string value = keyValue[1].Trim();
                        _sections[currentSection][key] = value;
                    }
                }
            }
        }

        // Получение значения по секции и ключу
        public string GetValue(string section, string key)
        {
            if (_sections.ContainsKey(section) && _sections[section].ContainsKey(key))
            {
                return _sections[section][key];
            }
            return null;
        }
    }
}
