using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JavaVirtualMachine
{
    public static class FileStreams
    {
        private static Dictionary<string, FileStream> streams = new Dictionary<string, FileStream>();
        private static StreamReader ConsoleInputReader = new StreamReader(Console.OpenStandardInput());
        private static Stream ConsoleOutputStream = Console.OpenStandardOutput();
        private static Stream ConsoleErrorStream = Console.OpenStandardError();
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
            return ConsoleInputReader.BaseStream.Read(array, offset, length);
        }

        public static int AvailableBytes(string file)
        {
            FileStream fileStream = streams[file];
            return (int)(fileStream.Length - fileStream.Position);
        }

        public static int AvailableBytesFromConsole()
        {
            return 0;
        }

        public static void OpenWrite(string file)
        {
            FileStream fileStream = File.Open(file, FileMode.Open);
            streams.Add(file, fileStream);
        }
        public static void WriteBytes(string file, byte[] array, int offset, int length, bool append)
        {
            FileStream fileStream = streams[file];
            if(append)
            {
                fileStream.Position = fileStream.Length;
                fileStream.Write(array, offset, length);
            }
            else
            {
                fileStream.Position = 0;
                fileStream.Write(array, offset, length);
            }
            fileStream.Flush();
        }

        public static void WriteBytesToConsole(byte[] array, int offset, int length)
        {
            ConsoleOutputStream.Write(array, offset, length);
        }
        public static void WriteBytesToError(byte[] array, int offset, int length)
        {
            ConsoleErrorStream.Write(array, offset, length);
        }

        public static void Close(string file)
        {
            FileStream fileStream = streams[file];
            fileStream.Close();
            streams.Remove(file);
        }
    }
}
