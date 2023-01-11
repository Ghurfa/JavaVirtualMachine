using System.Text;

namespace JavaVirtualMachine.StackTrace
{
    internal class ColoredStream
    {
        private Stream Stream;
        private UnicodeEncoding streamEncoding;

        public ColoredStream(Stream ioStream)
        {
            Stream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public (byte Color, string String) ReadString()
        {
            byte colorByte = (byte)Stream.ReadByte();
            int highLen = Stream.ReadByte();
            int lowLen = Stream.ReadByte();
            int len = (highLen << 8) | lowLen;
            var inBuffer = new byte[len];
            Stream.Read(inBuffer, 0, len);

            return (colorByte, streamEncoding.GetString(inBuffer));
        }

        public int WriteString(byte color, string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > ushort.MaxValue)
            {
                len = ushort.MaxValue;
            }
            Stream.WriteByte(color);
            Stream.WriteByte((byte)(len >> 8));
            Stream.WriteByte((byte)(len & 0xFF));
            Stream.Write(outBuffer, 0, len);
            Stream.Flush();

            return outBuffer.Length + 3;
        }
    }
}
