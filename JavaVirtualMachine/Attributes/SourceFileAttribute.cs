using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine.Attributes
{
    public class SourceFileAttribute : AttributeInfo
    {
        public ushort SourceFileIndex;
        public string SourceFileName;
        public SourceFileAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();
            SourceFileIndex = infoAsSpan.ReadTwo();
            SourceFileName = ((CUtf8Info)constants[SourceFileIndex]).String;
        }
    }
}
