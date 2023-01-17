namespace JavaVirtualMachine.ConstantPoolItems
{
    public class CFieldRefInfo : CRefInfo
    {
        public CFieldRefInfo(ref ReadOnlySpan<byte> span) : base(ref span) { }
    }
}
