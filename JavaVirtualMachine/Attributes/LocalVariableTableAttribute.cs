using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine.Attributes
{
    public struct LocalVariableInfo
    {
        public ushort StartPc;
        public ushort Length;
        public ushort NameIndex;
        public ushort DescriptorIndex;
        public ushort Index;

        public string Name;
        public string Descriptor;

        public LocalVariableInfo(ref ReadOnlySpan<byte> data, CPInfo[] constants)
        {
            StartPc = data.ReadTwo();
            Length = data.ReadTwo();
            NameIndex = data.ReadTwo();
            Name = ((CUtf8Info)constants[NameIndex]).String;
            DescriptorIndex = data.ReadTwo();
            Descriptor = ((CUtf8Info)constants[DescriptorIndex]).String;
            Index = data.ReadTwo();
        }
    }
    public class LocalVariableTableAttribute : AttributeInfo
    {
        public ushort LocalVariableTableLength;
        public LocalVariableInfo[] LocalVariableTable;
        
        public LocalVariableTableAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();

            LocalVariableTableLength = infoAsSpan.ReadTwo();
            LocalVariableTable = new LocalVariableInfo[LocalVariableTableLength];
            for (int i = 0; i < LocalVariableTableLength; i++)
            {
                LocalVariableTable[i] = new LocalVariableInfo(ref infoAsSpan, constants);
            }
        }
    }
}
