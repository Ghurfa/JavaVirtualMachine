using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine.Attributes
{
    public class EnclosingMethodAttribute : AttributeInfo
    {
        public ushort ClassIndex;
        public ushort MethodIndex;
        public CClassInfo Class;
        public CNameAndTypeInfo Method;
        public EnclosingMethodAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();

            ClassIndex = infoAsSpan.ReadTwo();
            MethodIndex = infoAsSpan.ReadTwo();

            Class = (CClassInfo)constants[ClassIndex];
            Method = (CNameAndTypeInfo)constants[MethodIndex];
        }
    }
}
