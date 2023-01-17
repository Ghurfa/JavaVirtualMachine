namespace JavaVirtualMachine.ConstantPoolItems
{
    class CDoubleInfo : CPInfo
    {
        public uint HighBytes { get; private set; }
        public uint LowBytes { get; private set; }
        public readonly long LongValue;
        public readonly double DoubleValue;
        public CDoubleInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            HighBytes = span.ReadFour();
            LowBytes = span.ReadFour();
            LongValue = ((long)HighBytes << 32) | LowBytes;
            unsafe
            {
                long temp = LongValue;
                DoubleValue = *(double*)(&temp);
            }
        }
        public override void Update(CPInfo[] constants) { }
    }
}
