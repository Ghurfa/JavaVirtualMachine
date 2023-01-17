using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine.Attributes
{
    public class ExceptionsAttribute : AttributeInfo
    {
        public readonly ushort NumOfExceptions;
        public readonly CClassInfo[] ExceptionsTable;
        public ExceptionsAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();

            NumOfExceptions = infoAsSpan.ReadTwo();

            ExceptionsTable = new CClassInfo[NumOfExceptions];
            for(int i = 0; i < NumOfExceptions; i++)
            {
                ExceptionsTable[i] = (CClassInfo)constants[infoAsSpan.ReadTwo()];
            }
        }
    }
}
