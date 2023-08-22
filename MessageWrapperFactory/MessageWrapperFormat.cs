using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MessageWrapperFactory
{
    class MessageWrapperFormat
    {
        const string BaseDir = "MessageWrapperFormats";
        const string FormatExtension = "txt";

        public static readonly string MessageHandler = $"{BaseDir}{Path.DirectorySeparatorChar}MessageHandler.{FormatExtension}";
        public static readonly string MessageHandlerItem = $"{BaseDir}{Path.DirectorySeparatorChar}MessageHandlerItem.{FormatExtension}";
        public static readonly string MessageManager = $"{BaseDir}{Path.DirectorySeparatorChar}MessageManager.{FormatExtension}";
        public static readonly string PacketTypeEnumItemFormat = $"{BaseDir}{Path.DirectorySeparatorChar}PacketTypeEnumItemFormat.{FormatExtension}";
        public static readonly string MessageManagerMapping = $"{BaseDir}{Path.DirectorySeparatorChar}MessageManagerMapping.{FormatExtension}";
        public static readonly string MessageManagerInit = $"{BaseDir}{Path.DirectorySeparatorChar}MessageManagerInit.{FormatExtension}";
    
        public static Dictionary<string, string> ReadAllFiles()
        {
            Dictionary<string, string> dict = new();
            foreach (string file in GetAllFileNames())
            {
                dict.Add(file, File.ReadAllText(file, Encoding.UTF8));
            }
            return dict;
        }

        public static string[] GetAllFileNames()
        {
            FieldInfo[] infos = typeof(MessageWrapperFormat).GetFields(BindingFlags.Public | BindingFlags.Static);
            var stringFields = infos.Where(field => field.FieldType == typeof(string));
            return stringFields.Select(field => (field.GetValue(null) as string)).ToArray();
        }

        public static bool CheckFiles()
        {
            bool flag = true;
            foreach (string file in GetAllFileNames())
            {
                if (File.Exists(file) == false)
                {
                    Console.WriteLine($"Can not find a format file: {file}");
                    flag = false;
                }
            }
            return flag;
        }
    }
}
