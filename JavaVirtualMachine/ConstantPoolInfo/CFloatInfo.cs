using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    class CFloatInfo : CPInfo
    {
        public uint IntValue { get; private set; }
        public readonly float FloatValue;
        public CFloatInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            IntValue = span.ReadFour();
            unsafe
            {
                uint temp = IntValue;
                FloatValue = *(float*)(&temp);
            }
        }
        public override void Update(CPInfo[] constants) { }
    }
}
