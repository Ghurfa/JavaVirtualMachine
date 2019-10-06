using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    public class CMethodHandleInfo : CPInfo
    {
        public readonly byte ReferenceKind;
        public readonly ushort ReferenceIndex;
        public CPInfo Reference;
        public CMethodHandleInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            ReferenceKind = span.ReadOne();
            ReferenceIndex = span.ReadTwo();
        }

        public override void Update(CPInfo[] constants)
        {
            Reference = constants[ReferenceIndex];
            //need to do referenceKind check things
        }
    }
}
