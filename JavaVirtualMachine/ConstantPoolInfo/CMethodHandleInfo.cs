using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.ConstantPoolInfo
{
    public enum MethodHandleRefKind
    {
        getField = 1,
        getStatic = 2,
        putField = 3,
        putStatic = 4,
        invokeVirtual = 5,
        invokeStatic = 6,
        invokeSpecial = 7,
        newInvokeSpecial = 8,
        invokeInterface = 9
    }

    public class CMethodHandleInfo : CPInfo
    {
        private readonly ushort ReferenceIndex;
        public CPInfo Reference;
        public readonly MethodHandleRefKind Kind;
        public CMethodHandleInfo(ref ReadOnlySpan<byte> span) : base(ref span)
        {
            byte referenceKind = span.ReadOne();
            ReferenceIndex = span.ReadTwo();

            Kind = (MethodHandleRefKind)referenceKind;
        }

        public override void Update(CPInfo[] constants)
        {
            Reference = constants[ReferenceIndex];
            //need to do referenceKind check things
        }
    }
}
