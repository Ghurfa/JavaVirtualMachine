using JavaVirtualMachine.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    class CInvokeDynamicInfo : CPInfo
    {
        public readonly ushort BootstrapMethodAttrIndex;
        public readonly ushort NameAndTypeIndex;
        public CNameAndTypeInfo NameAndTypeInfo;
        public BootstrapMethod BootstrapMethod;
        public CInvokeDynamicInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            BootstrapMethodAttrIndex = span.ReadTwo();
            NameAndTypeIndex = span.ReadTwo();
        }
        public override void Update(CPInfo[] constants)
        {
            NameAndTypeInfo = (CNameAndTypeInfo)constants[NameAndTypeIndex];
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
