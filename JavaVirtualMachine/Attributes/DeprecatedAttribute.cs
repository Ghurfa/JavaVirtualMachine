using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine.Attributes
{
    public class DeprecatedAttribute : AttributeInfo
    {
        public DeprecatedAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
        }
    }
}
