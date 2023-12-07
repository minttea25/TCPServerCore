using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text;
using System.Linq;

namespace MessageWrapperFactory
{
    class Program
    {
        readonly static string NewLine = Environment.NewLine;

        const string MessageManagerFile = "MessageManager.cs";
        const string MessageHandlerFile = "MessageHandler.cs";
        const string MessageTypesFile = "MessageTypes.cs";

        const string NamespaceRegex = @"^[a-zA-Z_][a-zA-Z0-9_.]*$";

        const string MessageToken = "message";
        const char Braces_Open = '{';
        const char Braces_Close = '}';

        const string ConfigPath = "message_wrapper_factory_config.json";

        readonly static List<string> ValidPacketMessagePrefix = new();

        static bool ValidateConfigs(FactoryConfig config)
        {
            if (Regex.IsMatch(config.CommonNamespace, NamespaceRegex) == false)
            {
                Console.WriteLine($"The '{config.CommonNamespace}' is not invalid name for csharp-namespace.");
                return false;
            }
            
            if (Directory.Exists(config.TargetDirectoryPath) == false)
            {
                Console.WriteLine($"Target Directory={config.TargetDirectoryPath} does not exist.");
                return false;
            }

            if (Directory.Exists(config.OutputServerCodeDirectoryPath) == false)
            {
                Directory.CreateDirectory(config.OutputServerCodeDirectoryPath);
            }

            if (Directory.Exists(config.OutputClientCodeDirectoryPath) == false)
            {
                Directory.CreateDirectory(config.OutputClientCodeDirectoryPath);
            }

            return true;
        }

        static List<string> ListFoundProtos(string targetPath, string extension)
        {
            string ex = $".{extension}";
            List<string> protos = new List<string>();
            foreach (string file in Directory.GetFiles(targetPath))
            {
                if (file.EndsWith(ex) == true)
                {
                    protos.Add(file);
                }
            }

            if (protos.Count == 0)
            {
                Console.WriteLine($"There is no proto file in TargetDirectory={targetPath}.");
                return null;
            }

            return protos;
        }

        static void PrintInfos(FactoryConfig config, List<string> protos)
        {
            Console.WriteLine($"Namespace: {config.CommonNamespace}");
            Console.WriteLine($"Target path: {config.TargetDirectoryPath}");
            Console.WriteLine($"Proto file extension: {config.ProtoFileExtension}");
            Console.WriteLine($"The prefix of exclude-packet in server: {config.ServerPacketExcludePrefix}");
            Console.WriteLine($"The prefix of exclude-packet in client: {config.ClientPacketExcludePrefix}");
            Console.WriteLine($"Output path of server code: {config.OutputServerCodeDirectoryPath}");
            Console.WriteLine($"Output path of client code: {config.OutputClientCodeDirectoryPath}");

            Console.WriteLine();

            Console.WriteLine($"Found .{config.ProtoFileExtension} files in {config.TargetDirectoryPath} ({protos.Count} files) : ");
            foreach(var p in protos)
            {
                Console.WriteLine(p);
            }

            Console.WriteLine();
        }

        static Dictionary<string, List<string>> FindMessages(List<string> protos)
        {
            Stack<char> stack = new Stack<char>();
            Dictionary<string, List<string>> messages = new Dictionary<string, List<string>>();
            foreach (string filePath in protos)
            {
                string text = File.ReadAllText(filePath);
                messages.Add(filePath, new List<string>());

                int startIndex = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == Braces_Open)
                    {
                        stack.Push(Braces_Open);
                        if (stack.Count == 1)
                        {
                            int endIndex = text.IndexOf(Braces_Close, i);
                            if (endIndex != -1) // if endIndex == -1 => syntax error in file
                            {
                                string messageBlock = text.Substring(startIndex, endIndex - startIndex + 1).Trim();

                                if (messageBlock.StartsWith(MessageToken))
                                {
                                    int idx = messageBlock.IndexOf(Braces_Open, MessageToken.Length + 1);
                                    string name = messageBlock.Substring(MessageToken.Length + 1, idx - MessageToken.Length - 1).Trim();
                                    messages[filePath].Add(name);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"The file[{filePath}] may have error(s): e.g. syntax error");
                                return null;
                            }
                        }
                    }
                    else if (text[i] == Braces_Close)
                    {
                        stack.Pop();
                        if (stack.Count == 0)
                        {
                            startIndex = i + 1;
                        }
                    }
                    else if (stack.Count == 0 && text[i] == ';') startIndex = i + 1;
                }

                stack.Clear();
            }

            if (messages.Count == 0)
            {
                Console.WriteLine($"There is no found message in the proto files.");
                return null;
            }

            Console.WriteLine("Found messages (top-level, except nested)");
            foreach (string file in protos)
            {
                Console.WriteLine($"### {Path.GetFileName(file)} ({messages[file].Count}): ");
                foreach (string name in messages[file])
                {
                    Console.WriteLine($"- {name}");
                }
            }
            Console.WriteLine();

            return messages;
        }

        static void CreateLogFile(FactoryConfig config, Dictionary<string, List<string>> messages)
        {
            StringBuilder sb = new();

            sb.AppendLine($"MessageWrapperFactory at {DateTime.Now}");

            sb.AppendLine();

            sb.AppendLine($"Namespace: {config.CommonNamespace}");
            sb.AppendLine($"Target path: {config.TargetDirectoryPath}");
            sb.AppendLine($"Proto file extension: {config.ProtoFileExtension}");
            sb.AppendLine($"The prefix of exclude-packet in server: {config.ServerPacketExcludePrefix}");
            sb.AppendLine($"The prefix of exclude-packet in client: {config.ClientPacketExcludePrefix}");
            sb.AppendLine($"Output path of server code: {config.OutputServerCodeDirectoryPath}");
            sb.AppendLine($"Output path of client code: {config.OutputClientCodeDirectoryPath}");

            sb.AppendLine();

            sb.AppendLine($"Found .{config.ProtoFileExtension} files in {config.TargetDirectoryPath} ({messages.Count} files) : ");
            foreach (var p in messages.Keys)
            {
                sb.AppendLine(p);
            }

            sb.AppendLine();

            sb.AppendLine("Found messages (top-level, except nested)");
            foreach (string file in messages.Keys)
            {
                sb.AppendLine($"### {Path.GetFileName(file)} ({messages[file].Count}): ");
                foreach (string name in messages[file])
                {
                    sb.AppendLine($"- {name}");
                }
            }
            sb.AppendLine();

            File.WriteAllText($"./MessageWrapperFactory_{DateTime.Now:yyyy-MM-dd-HH-mm}.log", sb.ToString());
        }


        static void Main()
        {
            if (File.Exists(ConfigPath) == false)
            {
                var defaultConfig = FactoryConfig.GetDefault();
                string text = JsonSerializer.Serialize(defaultConfig);
                File.WriteAllText(ConfigPath, text);
                Console.WriteLine($"{ConfigPath} is created.");
                return;
            }

            var config = JsonSerializer.Deserialize<FactoryConfig>(File.ReadAllText(ConfigPath));

            // check format files
            Dictionary<string, string> formats = MessageWrapperFormat.ReadAllFiles();
            if (formats == null) return;

            // validate configs
            if (ValidateConfigs(config) == false) return;

            // list found proto files
            List<string> protos = ListFoundProtos(config.TargetDirectoryPath, config.ProtoFileExtension);
            if (protos == null) return;

            // print the all options
            PrintInfos(config, protos);

            // find messages and print them all
            Dictionary<string, List<string>> messages = FindMessages(protos);
            if (messages == null) return;

            List<string> msgList = messages.SelectMany(pair => pair.Value).ToList();

            ValidPacketMessagePrefix.Add(config.ClientPacketExcludePrefix);
            ValidPacketMessagePrefix.Add(config.ServerPacketExcludePrefix);
            

            string messageTypesPath_server = $"{config.OutputServerCodeDirectoryPath}{Path.DirectorySeparatorChar}{MessageTypesFile}";
            string messageTypesPath_client = $"{config.OutputClientCodeDirectoryPath}{Path.DirectorySeparatorChar}{MessageTypesFile}";

            string messageManagerPath_server = $"{config.OutputServerCodeDirectoryPath}{Path.DirectorySeparatorChar}{MessageManagerFile}";
            string messageManagerPath_client = $"{config.OutputClientCodeDirectoryPath}{Path.DirectorySeparatorChar}{MessageManagerFile}";

            string messageHandlerPath_server = $"{config.OutputServerCodeDirectoryPath}{Path.DirectorySeparatorChar}{MessageHandlerFile}";
            string messageHandlerPath_client = $"{config.OutputClientCodeDirectoryPath}{Path.DirectorySeparatorChar}{MessageHandlerFile}";


            WriteMessageTypes(messageTypesPath_server, messageTypesPath_client, config.CommonNamespace, msgList,
                formats[MessageWrapperFormat.MessageTypes], formats[MessageWrapperFormat.MessageTypeEnumFormat]);

            WriteMessageManager(messageManagerPath_server, config.CommonNamespace, msgList,
                formats[MessageWrapperFormat.MessageManager],
                formats[MessageWrapperFormat.MessageManagerMapping_uint],
                formats[MessageWrapperFormat.MessageManagerMapping_ushort],
                formats[MessageWrapperFormat.MessageManagerInit],
                config.ClientPacketExcludePrefix);

            WriteMessageManager(messageManagerPath_client, config.CommonNamespace, msgList,
                formats[MessageWrapperFormat.MessageManager],
                formats[MessageWrapperFormat.MessageManagerMapping_uint],
                formats[MessageWrapperFormat.MessageManagerMapping_ushort],
                formats[MessageWrapperFormat.MessageManagerInit],
                config.ServerPacketExcludePrefix);

            WriteMessageHandler(messageHandlerPath_server, config.CommonNamespace, msgList,
                formats[MessageWrapperFormat.MessageHandler], formats[MessageWrapperFormat.MessageHandlerItem], config.ClientPacketExcludePrefix);

            WriteMessageHandler(messageHandlerPath_client, config.CommonNamespace, msgList,
                formats[MessageWrapperFormat.MessageHandler], formats[MessageWrapperFormat.MessageHandlerItem], config.ServerPacketExcludePrefix);

            CreateLogFile(config, messages);

            Console.WriteLine("All files are created successfully.");

            Console.WriteLine($"{messageTypesPath_server}");
            Console.WriteLine($"{messageTypesPath_client}");

            Console.WriteLine($"{messageManagerPath_server}");
            Console.WriteLine($"{messageManagerPath_client}");

            Console.WriteLine($"{messageHandlerPath_server}");
            Console.WriteLine($"{messageHandlerPath_client}");


        }

        static bool CheckExclude(string name, string prefix)
        {
            if (name.StartsWith(prefix) == true && Char.IsUpper(name[prefix.Length]) == true) return false;
            else return true;
        }

        static bool ContainPacketEnum(string name)
        {
            foreach(string prefix in ValidPacketMessagePrefix)
            {
                if (name.StartsWith(prefix) ==  true)
                {
                    return true;
                }
            }
            return false;
        }

        static void WriteMessageTypes(string filepath1, string filepath2, string namespaceName, List<string> messages,
            string messageTypesFormat, string messageTypeEnumFormat)
        {
            string enums = "";
            int v = 1;
            foreach (string msg in messages)
            {
                if (ContainPacketEnum(msg) == false) continue;

                enums += "        ";
                enums += string.Format(
                    messageTypeEnumFormat,
                    msg, v++);
                enums += NewLine;
            }

            string text = string.Format(
                messageTypesFormat,
                namespaceName,
                enums);

            File.WriteAllText(filepath1, text);
            File.WriteAllText(filepath2, text);
        }

        static void WriteMessageManager(string filepath, string namespaceName, List<string> messages, 
            string messageManagerFormat,
            string messageManagerMappingFormat_uint,
            string messageManagerMappingFormat_ushort,
            string messageManagerInitFormat,
            string targetPrefix)
        {
            string packetMapping_uint = ""; // 1
            string packetMapping_ushort = ""; // 2
            string packetInit = ""; // 3

            foreach (var msg in messages)
            {
                if (ContainPacketEnum(msg) == false) continue;

                packetMapping_uint += "            ";
                packetMapping_uint += string.Format(
                    messageManagerMappingFormat_uint,
                    msg);
                packetMapping_uint += NewLine;

                packetMapping_ushort += "            ";
                packetMapping_ushort += string.Format(
                    messageManagerMappingFormat_ushort,
                    msg);
                packetMapping_ushort += NewLine;
            }

            foreach (var msg in messages)
            {
                if (CheckExclude(msg, targetPrefix) == true) continue;

                packetInit += "            ";
                packetInit += string.Format(
                    messageManagerInitFormat,
                    msg);
                packetInit += NewLine;
            }

            File.WriteAllText(filepath, string.Format(
                messageManagerFormat,
                namespaceName,
                packetMapping_uint,
                packetMapping_ushort,
                packetInit));
        }

        static void WriteMessageHandler(string filepath, string namespaceName, List<string> messages,
            string messageHandlerFormat, string messageHandlerItemFormat,
            string targetPrefix)
        {
            string packetHandlerItem = "";
            foreach(string msg in messages)
            {
                if (CheckExclude(msg, targetPrefix) == true) continue;

                packetHandlerItem += "        ";
                packetHandlerItem += string.Format(
                    messageHandlerItemFormat,
                    msg);
                packetHandlerItem += NewLine;
            }
            File.WriteAllText(filepath, string.Format(
                messageHandlerFormat,
                namespaceName,
                packetHandlerItem));
        }
    }
}
