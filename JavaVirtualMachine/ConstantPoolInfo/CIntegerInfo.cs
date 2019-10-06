using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    class CIntegerInfo : CPInfo
    {
        public int Bytes { get; private set; }
        public CIntegerInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            Bytes = (int)span.ReadFour();
        }
        public override void Update(CPInfo[] constants) { }
    }
}
