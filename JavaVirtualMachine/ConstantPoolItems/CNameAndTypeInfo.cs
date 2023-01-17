namespace JavaVirtualMachine.ConstantPoolItems
{
    public class CNameAndTypeInfo : CPInfo
    {
        public ushort NameIndex { get; private set; }
        public ushort DescriptorIndex { get; private set; }
        public string Name;
        public string Descriptor;
        public CNameAndTypeInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            NameIndex = span.ReadTwo();
            DescriptorIndex = span.ReadTwo();
        }
        public override void Update(CPInfo[] constants)
        {
            Name = ((CUtf8Info)constants[NameIndex]).String;
            Descriptor = ((CUtf8Info)constants[DescriptorIndex]).String;
        }
    }
}
