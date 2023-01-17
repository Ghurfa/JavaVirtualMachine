namespace JavaVirtualMachine.ConstantPoolItems
{
    public class CMethodRefInfo : CRefInfo
    {
        public CMethodRefInfo(ref ReadOnlySpan<byte> span) : base(ref span) { }
    }
}
