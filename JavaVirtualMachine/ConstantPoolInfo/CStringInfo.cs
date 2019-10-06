using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    public class CStringInfo : CPInfo
    {
        public ushort StringIndex { get; private set; }
        public string String;
        public CStringInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            StringIndex = span.ReadTwo();
        }
        public override void Update(CPInfo[] constants)
        {
            String = ((CUtf8Info)constants[StringIndex]).String;
        }
    }
}
