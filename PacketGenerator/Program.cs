﻿using System;
using System.IO;
using System.Xml;

namespace PacketGenerator
{
    
    class Program
    {
        enum Param
        {
            PDL = 0,
            DummyClient = 1,
            Server = 2,
            Client = 3,
        }

        static string genPackets;
        static ushort packetId;
        static string packetEnums;

        static string clientRegister;
        static string serverRegister;

        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            string pdlPath = "PDL.xml";
            if ( args.Length >= 1 )
            {
                pdlPath = args[0];
            }

            // using scope을 벗어나면 XmlReader.Dispose()가 자동 호출됨.
            // XmlReader.Dispose(); <- Resource Release
            using (XmlReader r = XmlReader.Create(pdlPath, settings))
            {
                //Header 를 무시함. 
                //곧바로 <packet>으로 이동함.
                r.MoveToContent();

                // LineStream으로 Content를 읽는다.
                while (r.Read())
                {
                    // r.Name = TagName
                    // r["Attribute"] = Attribute
                    // r.Depth = 계층 Level
                    // r.NodeType = Tag의 종류(XmlNodeType.Element = 여는 태그)
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                }

                string fileText = string.Format(PacketFormat.fileFormat, packetEnums, genPackets);
                File.WriteAllText("GenPackets.cs", fileText);
                
                string clientManagerText = string.Format(PacketFormat.managerFormat, clientRegister);
                File.WriteAllText("ClientPacketManager.cs", clientManagerText);
                
                string serverManagerText = string.Format(PacketFormat.managerFormat, serverRegister);
                File.WriteAllText("ServerPacketManager.cs", serverManagerText);

                PacketCopy("GenPackets.cs", args);
                PacketManagerCopy("ClientPacketManager.cs", args[(int) Param.DummyClient]);
                PacketManagerCopy("ServerPacketManager.cs", args[(int) Param.Server]);
                
                if ( (args.Length - 1) == (int) Param.Client)
                {
                    PacketCopy("GenPackets.cs", args[(int) Param.Client]);
                    PacketManagerCopy("ClientPacketManager.cs", args[(int) Param.Client]);
                }

                File.Delete("GenPackets.cs");
                File.Delete("ClientPacketManager.cs");
                File.Delete("ServerPacketManager.cs");
            }
        }

        public static void PacketCopy(string fileName, string[] args)
        {
            if (args.Length < 3)
                return;

            for (int i = 1; i < args.Length; i++)
            {
                PacketCopy(fileName, args[i]);
            }
        }

        public static void PacketCopy(string fileName, string path)
        {
            File.Copy(fileName, $"{path}/{fileName}", true);
        }

        public static void PacketManagerCopy(string fileName, string path)
        {
            File.Copy(fileName, $"{path}/{fileName}", true);
        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)
                return;

            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invalid packet node");
                return;
            }

            string usage = r["usage"];
            string packetName = r["name"];

            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }
            if (string.IsNullOrEmpty(usage))
            {
                Console.WriteLine("Packet without usage");
                return;
            }

            Tuple<string, string, string> t = ParseMembers(r);
            genPackets += string.Format(PacketFormat.packetFormat, packetName , t.Item1 , t.Item2, t.Item3);
            packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId) + Environment.NewLine + "\t";

            if (usage.ToLower() == "server" )
                clientRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;
            else
                serverRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;
        }

        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write
        public static Tuple<string, string, string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            // r.Depth => 상위 Node의 Depth
            // r.Depth + 1 => 현재 Node의 Depth
            int depth = r.Depth + 1;
            while (r.Read()) 
            {
                if (r.Depth != depth)
                    break;

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                // 라인 끝마다 엔터를 붙임
                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine;
                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine;
                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine;

                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "class":
                        {
                            Tuple<string, string, string> t = ParseClass(r);
                            memberCode += t.Item1;
                            readCode += t.Item2;
                            writeCode += t.Item3;
                        }
                        break;
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                        writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        {
                            Tuple<string, string, string> t = ParseList(r);
                            memberCode += t.Item1;
                            readCode += t.Item2;
                            writeCode += t.Item3;
                        }
                        break;
                    default:
                        break;
                }
            }

            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            Tuple<string, string, string> t = ParseMembers(r);

            string memberCode = string.Format(PacketFormat.memberListFormat
                                    , FirstCharToUpper(listName)
                                    , FirstCharToLower(listName)
                                    , t.Item1, t.Item2, t.Item3);

            string readCode = string.Format(PacketFormat.readListFormat
                                    , FirstCharToUpper(listName)
                                    , FirstCharToLower(listName));

            string writeCode = string.Format(PacketFormat.writeListFormat
                                    , FirstCharToUpper(listName)
                                    , FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseClass(XmlReader r)
        {
            string className = r["name"];
            if (string.IsNullOrEmpty(className))
            {
                Console.WriteLine("Class without name");
                return null;
            }

            Tuple<string, string, string> t = ParseMembers(r);

            string memberCode = string.Format(PacketFormat.memberClassFormat
                                    , FirstCharToUpper(className)
                                    , FirstCharToLower(className)
                                    , t.Item1, t.Item2, t.Item3);

            string readCode = string.Format(PacketFormat.readClassFormat
                                    , FirstCharToUpper(className)
                                    , FirstCharToLower(className));

            string writeCode = string.Format(PacketFormat.writeClassFormat
                                    , FirstCharToUpper(className)
                                    , FirstCharToLower(className));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch(memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}
