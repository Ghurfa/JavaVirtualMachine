using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine.Attributes
{
    public class SignatureAttribute : AttributeInfo
    {
        public ushort SignatureIndex;
        public string Signature;

        public SignatureAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();
            SignatureIndex = infoAsSpan.ReadTwo();
            Signature = ((CUtf8Info)constants[SignatureIndex]).String;
        }
    }
}
