﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JavaVirtualMachine
{
    public static class FileStreams
    {
        private static Dictionary<string, FileStream> streams = new Dictionary<string, FileStream>();
        private static StreamReader ConsoleInputReader = new StreamReader(Console.OpenStandardInput());
        public static void OpenRead(string file)
        {
            FileStream fileStream = File.OpenRead(file);
            streams.Add(file, fileStream);
        }

        public static int ReadBytes(string file, byte[] array, int offset, int length)
        {
            FileStream fileStream = streams[file];
            return fileStream.Read(array, offset, length);
        }

        public static int ReadBytesFromConsole(byte[] array, int offset, int length)
        {
            //TextReader textReader = Console.In;
            //textReader.ReadToEnd();
            return ConsoleInputReader.BaseStream.Read(array, offset, length);
        }

        public static int AvailableBytes(string file)
        {
            FileStream fileStream = streams[file];
            return (int)(fileStream.Length - fileStream.Position);
        }

        public static int AvailableBytesFromConsole()
        {
            return ConsoleInputReader.EndOfStream ? 0 : 1;
        }

        public static void Close(string file)
        {
            FileStream fileStream = streams[file];
            fileStream.Close();
            streams.Remove(file);
        }
    }
}
