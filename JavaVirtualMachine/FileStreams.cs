namespace JavaVirtualMachine
{
    public static class FileStreams
    {
        private static Dictionary<string, FileStream> streams = new Dictionary<string, FileStream>();
        private static StreamReader ConsoleInputReader = new StreamReader(Console.OpenStandardInput());
        private static Stream ConsoleOutputStream = Console.OpenStandardOutput();
        private static Stream ConsoleErrorStream = Console.OpenStandardError();

        public static bool OpenRead(string file)
        {
            try
            {
                FileStream fileStream = File.OpenRead(file);
                streams.Add(file, fileStream);
                return false;
            }
            catch (FileNotFoundException)
            {
                return true;
            }
        }

        public static int ReadBytes(string file, Span<byte> span)
        {
            FileStream fileStream = streams[file];
            return fileStream.Read(span);
        }

        public static int ReadBytesFromConsole(Span<byte> span)
        {
            return ConsoleInputReader.BaseStream.Read(span);
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

        public static bool OpenWrite(string file)
        {
            try
            {
                FileStream fileStream = File.Open(file, FileMode.Open);
                streams.Add(file, fileStream);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public static void WriteBytes(string file, ReadOnlySpan<byte> span, bool append)
        {
            FileStream fileStream = streams[file];
            if(append)
            {
                fileStream.Position = fileStream.Length;
                fileStream.Write(span);
            }
            else
            {
                fileStream.Position = 0;
                fileStream.Write(span);
            }
            fileStream.Flush();
        }

        public static void WriteBytesToConsole(ReadOnlySpan<byte> span)
        {
            Console.ForegroundColor = ConsoleColor.White;
            ConsoleOutputStream.Write(span);
        }

        public static void WriteBytesToError(ReadOnlySpan<byte> span)
        {
            Console.ForegroundColor = ConsoleColor.White;
            ConsoleErrorStream.Write(span);
        }

        public static void Close(string file)
        {
            FileStream fileStream = streams[file];
            fileStream.Close();
            streams.Remove(file);
        }
    }
}
