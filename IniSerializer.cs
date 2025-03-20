using IniParser;
using IniParser.Model;
using System;
using System.Globalization;
using System.Reflection;

namespace IniBuilder
{
    public class IniSerializer
    {
        private readonly FileIniDataParser _parser;
        private readonly CultureInfo _cultureInfo;

        public IniSerializer()
        {
            _parser = new();
            _cultureInfo = CultureInfo.InvariantCulture; // Для корректного форматирования чисел с плавающей запятой
        }

        /// <summary>
        /// Exploration serialization in INI File
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="obj">The object.</param>
        public void Serialize<T>(string filePath, T obj)
        {
            var data = new IniData();
            SerializeObject(data, typeof(T).Name, obj);
            _parser.WriteFile(filePath, data);
        }

        private void SerializeObject(IniData data, string sectionName, object obj)
        {
            if (obj == null) return;

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<IniIgnoreAttribute>() != null)
                    continue;

                var value = property.GetValue(obj);

                if (value == null)
                {
                    data[sectionName][property.Name] = null;
                    continue;
                }

                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    string nestedSectionName = $"{sectionName}.{property.Name}";
                    SerializeObject(data, nestedSectionName, value);
                }
                else
                {
                    if (property.PropertyType == typeof(float) || property.PropertyType == typeof(double))
                    {
                        data[sectionName][property.Name] = Convert.ToString(value, _cultureInfo);
                    }
                    else
                    {
                        data[sectionName][property.Name] = value.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Deserialization from the Ini file to the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public T Deserialize<T>(string filePath) where T : new()
        {
            var data = _parser.ReadFile(filePath);
            return (T)DeserializeObject(data, typeof(T).Name, typeof(T));
        }

        private object DeserializeObject(IniData data, string sectionName, Type type)
        {
            var obj = Activator.CreateInstance(type);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<IniIgnoreAttribute>() != null)
                    continue;

                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    // Обработка вложенных объектов
                    string nestedSectionName = $"{sectionName}.{property.Name}";
                    if (data.Sections.Contains(nestedSectionName))
                    {
                        var nestedObj = DeserializeObject(data, nestedSectionName, property.PropertyType);
                        property.SetValue(obj, nestedObj);
                    }
                }
                else if (data[sectionName].ContainsKey(property.Name))
                {
                    // Обработка простых типов
                    var value = data[sectionName][property.Name];
                    
                    if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(obj, value);
                    }                  
                    else if (property.PropertyType.IsEnum)  // Обработка Enum
                    {
                        property.SetValue(obj, Enum.Parse(property.PropertyType, value)); // Десериализация Enum из строки
                    }
                    else if (property.PropertyType == typeof(sbyte))
                    {
                        property.SetValue(obj, sbyte.Parse(value));
                    }
                    else if (property.PropertyType == typeof(byte))
                    {
                        property.SetValue(obj, byte.Parse(value));
                    }
                    else if (property.PropertyType == typeof(ushort))
                    {
                        property.SetValue(obj, ushort.Parse(value));
                    }
                    else if (property.PropertyType == typeof(short))
                    {
                        property.SetValue(obj, short.Parse(value));
                    }
                    else if (property.PropertyType == typeof(uint))
                    {
                        property.SetValue(obj, uint.Parse(value));
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(obj, int.Parse(value));
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(obj, bool.Parse(value));
                    }
                    else if (property.PropertyType == typeof(float))
                    {
                        property.SetValue(obj, float.Parse(value, _cultureInfo));
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        property.SetValue(obj, double.Parse(value, _cultureInfo));
                    }
                }
            }

            return obj;
        }
    }
}