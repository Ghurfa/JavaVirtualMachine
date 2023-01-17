namespace JavaVirtualMachine.ConstantPoolItems
{
    public abstract class CPInfo
    {
        public byte Tag { get; private set; }
        public CPInfo(ref ReadOnlySpan<byte> span)
        {
            Tag = span.ReadOne();
        }
        public abstract void Update(CPInfo[] constants);
    }
}
