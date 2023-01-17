using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine.Attributes
{
    public class LineNumberTableAttribute : AttributeInfo
    {
        public ushort LineNumberTableLength;
        public (ushort startIp, ushort lineNumber)[] LineNumberTable;
        public LineNumberTableAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();
            LineNumberTableLength = infoAsSpan.ReadTwo();
            LineNumberTable = new (ushort, ushort)[LineNumberTableLength];
            for (int i = 0; i < LineNumberTableLength; i++)
            {
                ushort startIp = infoAsSpan.ReadTwo();
                ushort lineNumber = infoAsSpan.ReadTwo();
                LineNumberTable[i] = (startIp, lineNumber);
            }
        }
    }
}
