namespace JavaVirtualMachine.ConstantPoolItems
{
    class CMethodTypeInfo : CPInfo
    {
        public readonly ushort DescriptorIndex;
        public CUtf8Info Descriptor; 
        public CMethodTypeInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            DescriptorIndex = span.ReadTwo();
        }
        public override void Update(CPInfo[] constants)
        {
            Descriptor = (CUtf8Info)constants[DescriptorIndex];
        }
    }
}
