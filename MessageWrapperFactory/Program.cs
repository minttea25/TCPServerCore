using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MessageWrapperFactory
{
    class Program
    {
        enum ExcludeOption
        {
            None, Server, Client, Both
        }

        const string ExcludePrefixFromServer = "-s"; // prefix (It will checks the letter following the prefix starts with an uppercase letter.
        const string ExcludePrefixFromClient = "-c"; // prefix (It will checks the letter following the prefix starts with an uppercase letter.
        const string ProtoFileSuffixExtension = ".proto";

        const string TargetServerDirectory = @"Server [exclude prefix={0}]";
        const string TargetClientDirectory = @"Client [exclude prefix={0}]";
        const string MessageManagerFile = "MessageManager.cs";
        const string MessageHandlerFile = "MessageHandler.cs";

        const string NamespaceRegex = @"^[a-zA-Z_][a-zA-Z0-9_]*$";

        const string MessageToken = "message";
        const char Braces_Open = '{';
        const char Braces_Close = '}';

        static void Main(string[] args)
        {
            if (MessageWrapperFormat.CheckFiles() == false)
            {
                return;
            }

            string excludeOptionPrefixClient = null;
            string excludeOptionPrefixServer = null;

            // check args
            {
                for(int i=0; i<args.Length; i++)
                {
                    if (args[i] == ExcludePrefixFromClient)
                    {
                        if (i + 1 >= args.Length || args[i + 1] == ExcludePrefixFromServer)
                        {
                            Console.WriteLine(@$"A prefix string for client must come as a argument after '{ExcludePrefixFromClient}'");
                            return;
                        }
                        else excludeOptionPrefixClient = args[i + 1];
                    }
                    else if (args[i] == ExcludePrefixFromServer)
                    {
                        if (i + 1 >= args.Length || args[i + 1] == ExcludePrefixFromClient)
                        {
                            Console.WriteLine(@$"A prefix string for server must come as a argument after '{ExcludePrefixFromServer}'");
                            return;
                        }
                        else excludeOptionPrefixServer = args[i + 1];
                    }
                }
            }

            int argStartIndex = (string.IsNullOrEmpty(excludeOptionPrefixClient) ? 0 : 2) + (string.IsNullOrEmpty(excludeOptionPrefixServer) ? 0 : 2);
            if (args.Length < argStartIndex + 2)
            {
                Console.WriteLine(@"The arguments should contains name of namespace and an input directory that contains proto files.");
                return;
            }

            string namespaceName = args[argStartIndex];
            if (Regex.IsMatch(namespaceName, NamespaceRegex) == false)
            {
                Console.WriteLine(@$"The {namespaceName} is not invalid name for csharp-namespace.");
                return;
            }

            string inputDirectory = args[argStartIndex + 1];
            if (Directory.Exists(inputDirectory) == false)
            {
                Console.WriteLine($@"Input Directory={inputDirectory} does not exist.");
                return;
            }
            List<string> inputs = new List<string>();
            foreach(string file in Directory.GetFiles(inputDirectory))
            {
                if (file.EndsWith(ProtoFileSuffixExtension) == true)
                {
                    inputs.Add(file);
                }
            }
            if (inputs.Count == 0)
            {
                Console.WriteLine($@"There is no proto file in InputDirectory={inputDirectory}.");
                return;
            }

            // print the all options
            {
                if (string.IsNullOrEmpty(excludeOptionPrefixServer) == false)
                {
                    Console.WriteLine(@$"The prefix of server-class: {excludeOptionPrefixServer}");
                }
                if (string.IsNullOrEmpty(excludeOptionPrefixClient) == false)
                {
                    Console.WriteLine(@$"The prefix of client-class: {excludeOptionPrefixClient}");
                }
                Console.WriteLine(@$"Namespace: {namespaceName}");
                Console.WriteLine(@"Input proto files");
                foreach (string filePath in inputs)
                {
                    Console.WriteLine(@$"{Path.GetFileName(filePath)}");
                }
            }

            List<string> messages = new List<string>();
            Stack<char> stack = new Stack<char>();
            foreach (string filePath in inputs)
            {
                string text = File.ReadAllText(filePath);

                int startIndex = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == Braces_Open)
                    {
                        stack.Push(Braces_Open);
                        if (stack.Count == 1)
                        {
                            int endIndex = text.IndexOf(Braces_Close, i);
                            if (endIndex != -1) // if endIndex == -1 => error syntax file
                            {
                                string messageBlock = text.Substring(startIndex, endIndex - startIndex + 1).Trim();

                                if (messageBlock.StartsWith(MessageToken))
                                {
                                    int idx = messageBlock.IndexOf(Braces_Open, MessageToken.Length + 1);
                                    string name = messageBlock.Substring(MessageToken.Length + 1, idx - MessageToken.Length - 1).Trim();
                                    messages.Add(name);
                                }
                            }
                            else
                            {
                                Console.WriteLine(@"The file may have error(s): e.g. syntax error");
                                return;
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
                }

                stack.Clear();
            }

            ExcludeOption option = ExcludeOption.None;
            if (string.IsNullOrEmpty(excludeOptionPrefixServer) == false && string.IsNullOrEmpty(excludeOptionPrefixClient) == false)
            {
                string clientDir = string.Format(TargetClientDirectory, excludeOptionPrefixClient);
                string serverDir = string.Format(TargetServerDirectory, excludeOptionPrefixServer);

                if (Directory.Exists(clientDir) == false) Directory.CreateDirectory(clientDir);
                if (Directory.Exists(serverDir) == false) Directory.CreateDirectory(serverDir);

                option = ExcludeOption.Both;
                Console.WriteLine($"Client Exclude Prefix Option: {excludeOptionPrefixClient}");
                Console.WriteLine($"Server Exclude Prefix Option: {excludeOptionPrefixServer}");
            }
            else if (string.IsNullOrEmpty(excludeOptionPrefixClient) == false)
            {
                string clientDir = string.Format(TargetClientDirectory, excludeOptionPrefixClient);
                if (Directory.Exists(clientDir) == false) Directory.CreateDirectory(clientDir);

                option = ExcludeOption.Client;
                Console.WriteLine($"Client Exclude Prefix Option: {excludeOptionPrefixClient}");
            }
            else if (string.IsNullOrEmpty(excludeOptionPrefixServer) == false)
            {
                string serverDir = string.Format(TargetServerDirectory, excludeOptionPrefixServer);
                if (Directory.Exists(serverDir) == false) Directory.CreateDirectory(serverDir);

                option = ExcludeOption.Server;
                Console.WriteLine($"Server Exclude Prefix Option: {excludeOptionPrefixServer}");
            }
            
            Dictionary<string, string> formats = MessageWrapperFormat.ReadAllFiles();

            if (option == ExcludeOption.Both || option == ExcludeOption.Server)
            {
                string serverPathManager = $"{string.Format(TargetServerDirectory, excludeOptionPrefixServer)}{Path.DirectorySeparatorChar}{MessageManagerFile}";
                WriteMessageManager(serverPathManager, namespaceName, messages,
                    formats[MessageWrapperFormat.MessageManager],
                    formats[MessageWrapperFormat.PacketTypeEnumItemFormat],
                    formats[MessageWrapperFormat.MessageManagerMapping],
                    formats[MessageWrapperFormat.MessageManagerInit],
                    excludeOptionPrefixServer);
                Console.WriteLine($"{serverPathManager} is created.");

                string serverPathHandler = $"{string.Format(TargetServerDirectory, excludeOptionPrefixServer)}{Path.DirectorySeparatorChar}{MessageHandlerFile}";
                WriteMessageHandler(serverPathHandler, namespaceName, messages,
                    formats[MessageWrapperFormat.MessageHandler], formats[MessageWrapperFormat.MessageHandlerItem],
                    excludeOptionPrefixServer);
                Console.WriteLine($"{serverPathHandler} is created.");
            }
            if (option == ExcludeOption.Both || option == ExcludeOption.Client) 
            { 
                string clientPathManager = $"{string.Format(TargetClientDirectory, excludeOptionPrefixClient)}{Path.DirectorySeparatorChar}{MessageManagerFile}";
                WriteMessageManager(clientPathManager, namespaceName, messages,
                    formats[MessageWrapperFormat.MessageManager],
                    formats[MessageWrapperFormat.PacketTypeEnumItemFormat],
                    formats[MessageWrapperFormat.MessageManagerMapping],
                    formats[MessageWrapperFormat.MessageManagerInit],
                    excludeOptionPrefixClient);
                Console.WriteLine($"{clientPathManager} is created.");

                string clientPathHandler = $"{string.Format(TargetClientDirectory, excludeOptionPrefixClient)}{Path.DirectorySeparatorChar}{MessageHandlerFile}";
                WriteMessageHandler(clientPathHandler, namespaceName, messages,
                    formats[MessageWrapperFormat.MessageHandler], formats[MessageWrapperFormat.MessageHandlerItem],
                    excludeOptionPrefixClient);
                Console.WriteLine($"{clientPathHandler} is created.");
            }
            if (option == ExcludeOption.None)
            {
                WriteMessageManager(MessageManagerFile, namespaceName, messages,
                    formats[MessageWrapperFormat.MessageManager],
                    formats[MessageWrapperFormat.PacketTypeEnumItemFormat],
                    formats[MessageWrapperFormat.MessageManagerMapping],
                    formats[MessageWrapperFormat.MessageManagerInit]);
                Console.WriteLine($"{MessageManagerFile} is created.");

                WriteMessageHandler(MessageHandlerFile, namespaceName, messages,
                    formats[MessageWrapperFormat.MessageHandler], formats[MessageWrapperFormat.MessageHandlerItem]);
                Console.WriteLine($"{MessageHandlerFile} is created.");
            }
        }

        static bool CheckName(string name, string prefix)
        {
            if (prefix == null || name.Length <= prefix.Length)
                return false;

            if (name.StartsWith(prefix) && char.IsUpper(name[prefix.Length]))
                return true;

            return false;
        }

        static void WriteMessageManager(string filepath, string namespaceName, List<string> messages, 
            string messageManagerFormat, string packetTypeEnumFormat, string messageManagerMappingFormat, string messageManagerInitFormat,
            string excludePrefix = null)
        {
            string packetEnums = "";
            string packetMapping = "";
            string packetInit = "";
            for (int i=0; i<messages.Count; i++)
            {
                if (CheckName(messages[i], excludePrefix) == true) continue;

                packetEnums += string.Format(
                    packetTypeEnumFormat,
                    messages[i], i + 1);
                packetEnums += Environment.NewLine;

                packetMapping += string.Format(
                    messageManagerMappingFormat,
                    messages[i]);
                packetMapping += Environment.NewLine;

                packetInit += string.Format(
                    messageManagerInitFormat,
                    messages[i]);
                packetInit += Environment.NewLine;
            }

            File.WriteAllText(filepath, string.Format(
                messageManagerFormat,
                namespaceName,
                packetEnums.Replace(Environment.NewLine, $"{Environment.NewLine}        "),
                packetMapping.Replace(Environment.NewLine, $"{Environment.NewLine}            "),
                packetInit.Replace(Environment.NewLine, $"{Environment.NewLine}            ")));
        }

        static void WriteMessageHandler(string filepath, string namespaceName, List<string> messages,
            string messageHandlerFormat, string messageHandlerItemFormat,
            string excludePrefix = null)
        {
            string packetHandlerItem = "";
            foreach(string name in messages)
            {
                if (CheckName(name, excludePrefix) == true) continue;

                packetHandlerItem += string.Format(
                    messageHandlerItemFormat,
                    name);
                packetHandlerItem += Environment.NewLine;
            }
            File.WriteAllText(filepath, string.Format(
                messageHandlerFormat,
                namespaceName,
                packetHandlerItem.Replace(Environment.NewLine, $"{Environment.NewLine}        ")));
        }
    }
}
