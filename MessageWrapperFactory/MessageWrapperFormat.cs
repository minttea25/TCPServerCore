using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MessageWrapperFactory
{
    [Serializable]
    public class FactoryConfig
    {
        public string? CommonNamespace { get; set; }
        public string TargetDirectoryPath { get; set; }

        public string? OutputServerCodeDirectoryPath { get; set; }
        public string? OutputClientCodeDirectoryPath { get; set; }

        public string? ServerPacketExcludePrefix { get; set; }
        public string? ClientPacketExcludePrefix { get; set; }
        public string? ProtoFileExtension { get; set; }

        public static FactoryConfig GetDefault()
        {
            return new()
            {
                CommonNamespace = "",
                TargetDirectoryPath = "",
                OutputServerCodeDirectoryPath = "server",
                OutputClientCodeDirectoryPath = "client",
                ServerPacketExcludePrefix = "C",
                ClientPacketExcludePrefix = "S",
                ProtoFileExtension = "proto"
            };
        }

    }

    class MessageWrapperFormat
    {
        const string BaseDir = "MessageWrapperFormats";
        const string FormatExtension = "txt";

        public static readonly string MessageHandler = $"{BaseDir}{Path.DirectorySeparatorChar}MessageHandler.{FormatExtension}";
        public static readonly string MessageHandlerItem = $"{BaseDir}{Path.DirectorySeparatorChar}MessageHandlerItem.{FormatExtension}";
        public static readonly string MessageManager = $"{BaseDir}{Path.DirectorySeparatorChar}MessageManager.{FormatExtension}";
        public static readonly string MessageTypeEnumFormat = $"{BaseDir}{Path.DirectorySeparatorChar}MessageTypeEnumFormat.{FormatExtension}";
        public static readonly string MessageManagerMapping_uint = $"{BaseDir}{Path.DirectorySeparatorChar}MessageManagerMapping_uint.{FormatExtension}";
        public static readonly string MessageManagerMapping_ushort = $"{BaseDir}{Path.DirectorySeparatorChar}MessageManagerMapping_ushort.{FormatExtension}";
        public static readonly string MessageManagerInit = $"{BaseDir}{Path.DirectorySeparatorChar}MessageManagerInit.{FormatExtension}";
        public static readonly string MessageTypes = $"{BaseDir}{Path.DirectorySeparatorChar}MessageTypes.{FormatExtension}";
    
        public static Dictionary<string, string> ReadAllFiles()
        {
            if (CheckFiles() == false) return null;

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

        static bool CheckFiles()
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
