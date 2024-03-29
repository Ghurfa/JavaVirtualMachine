﻿namespace JavaVirtualMachine.ConstantPoolItems
{
    class CIntegerInfo : CPInfo
    {
        public int IntValue { get; private set; }
        public CIntegerInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            IntValue = (int)span.ReadFour();
        }
        public override void Update(CPInfo[] constants) { }
    }
}
