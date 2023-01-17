using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine.Attributes
{
    public struct ClassTableEntry
    {
        public ushort InnerClassInfoIndex;
        public ushort OuterClassInfoIndex;
        public ushort InnerNameIndex;
        public ushort InnerClassAccessFlags;
        public CClassInfo InnerClassInfo;
        public CClassInfo OuterClassInfo;
        public string InnerClassName;

        public ClassTableEntry(ref ReadOnlySpan<byte> data, CPInfo[] constants)
        {
            InnerClassInfoIndex = data.ReadTwo();
            OuterClassInfoIndex = data.ReadTwo();
            InnerNameIndex = data.ReadTwo();
            InnerClassAccessFlags = data.ReadTwo();
            InnerClassInfo = (CClassInfo)constants[InnerClassInfoIndex];
            OuterClassInfo = (CClassInfo)constants[OuterClassInfoIndex];
            if(InnerNameIndex != 0)
            {
                InnerClassName = ((CUtf8Info)constants[InnerNameIndex]).String;
            }
            else
            {
                InnerClassName = null;
            }
        }
    }

    public class InnerClassesAttribute : AttributeInfo
    {
        public ushort NumberOfClasses;
        public ClassTableEntry[] Classes;
        public InnerClassesAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();

            NumberOfClasses = infoAsSpan.ReadTwo();
            Classes = new ClassTableEntry[NumberOfClasses];
            for (int i = 0; i < NumberOfClasses; i++)
            {
                Classes[i] = new ClassTableEntry(ref infoAsSpan, constants);
            }
        }
    }
}
