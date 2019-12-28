using JavaVirtualMachine.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    class CInvokeDynamicInfo : CPInfo
    {
        private readonly ushort BootstrapMethodAttrIndex;
        private readonly ushort NameAndTypeIndex;
        private CNameAndTypeInfo NameAndTypeInfo;
        public BootstrapMethod BootstrapMethod;
        public string Name;
        public string Descriptor;
        public CInvokeDynamicInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            BootstrapMethodAttrIndex = span.ReadTwo();
            NameAndTypeIndex = span.ReadTwo();
        }
        public override void Update(CPInfo[] constants)
        {
            NameAndTypeInfo = (CNameAndTypeInfo)constants[NameAndTypeIndex];
            NameAndTypeInfo.Update(constants);
            Name = NameAndTypeInfo.Name;
            Descriptor = NameAndTypeInfo.Descriptor;
        }
        public void UpdateWithAttributes(AttributeInfo[] attributes)
        {
            foreach (AttributeInfo attribute in attributes)
            {
                if (attribute.GetType() == typeof(BootstrapMethodsAttribute))
                {
                    BootstrapMethod = ((BootstrapMethodsAttribute)attribute).BootstrapMethods[BootstrapMethodAttrIndex];
                }
            }
        }
    }
}
