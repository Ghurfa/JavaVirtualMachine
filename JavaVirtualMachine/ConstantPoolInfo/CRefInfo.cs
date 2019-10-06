using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    public abstract class CRefInfo : CPInfo
    {
        public ushort ClassIndex { get; private set; }
        public ushort NameAndTypeIndex { get; private set; }
        public string ClassName;
        public string Name;
        public string Descriptor;
        public CRefInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            ClassIndex = span.ReadTwo();
            NameAndTypeIndex = span.ReadTwo();
        }
        public override void Update(CPInfo[] constants)
        {
            constants[ClassIndex].Update(constants);
            ClassName = ((CClassInfo)constants[ClassIndex]).Name;
            constants[NameAndTypeIndex].Update(constants);
            Name = ((CNameAndTypeInfo)constants[NameAndTypeIndex]).Name;
            Descriptor = ((CNameAndTypeInfo)constants[NameAndTypeIndex]).Descriptor;
        }
    }
}
