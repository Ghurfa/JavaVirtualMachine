namespace JavaVirtualMachine.ConstantPoolItems
{
    public class CClassInfo : CPInfo
    {
        public ushort NameIndex { get; private set; }
        public string Name;
        public CClassInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            NameIndex = span.ReadTwo();
        }
        public override void Update(CPInfo[] constants)
        {
            Name = ((CUtf8Info)constants[NameIndex]).String;
        }
    }
}
