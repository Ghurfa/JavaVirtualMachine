using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine.Attributes
{
    public enum VerificationType
    {
        TopVariable,
        IntegerVariable,
        FloatVariable,
        DoubleVariable,
        LongVariable,
        NullVariable,
        UninitializedThisVariable,
        ObjectVariable,
        UninitializedVariable
    }

    public struct VerificationTypeInfo
    {
        public byte Tag;
        public VerificationType Type;
        public CClassInfo ObjectType;
        public ushort Offset;
        public VerificationTypeInfo(ref ReadOnlySpan<byte> data, CPInfo[] constants)
        {
            Tag = data.ReadOne();
            Type = (VerificationType)Tag;
            ObjectType = null;
            Offset = 0;
            switch (Type)
            {
                case VerificationType.TopVariable:
                case VerificationType.IntegerVariable:
                case VerificationType.FloatVariable:
                case VerificationType.LongVariable:
                case VerificationType.DoubleVariable:
                case VerificationType.NullVariable:
                case VerificationType.UninitializedThisVariable:
                    break;
                case VerificationType.ObjectVariable:
                    ushort cPoolIndex = data.ReadTwo();
                    ObjectType = ((CClassInfo)constants[cPoolIndex]);
                    break;
                case VerificationType.UninitializedVariable:
                    Offset = data.ReadTwo();
                    break;
            }
        }
    }
    public enum StackMapFrameType
    {
        SameFrame,
        SameLocals1StackItemFrame,
        SameLocals1StackItemFrameExtended,
        ChopFrame,
        SameFrameExtended,
        AppendFrame,
        FullFrame
    }

    public struct StackMapFrame
    {
        public byte Tag;
        public StackMapFrameType FrameType;
        public ushort OffsetDelta;
        public VerificationTypeInfo[] Stack;
        public int K; //?
        public VerificationTypeInfo[] Locals;

        public StackMapFrame(ref ReadOnlySpan<byte> data, CPInfo[] constants)
        {
            Tag = data.ReadOne();
            Stack = null;
            Locals = null;
            K = 0;
            if(Tag < 64)
            {
                FrameType = StackMapFrameType.SameFrame;
                OffsetDelta = Tag;
            }
            else if(Tag < 128)
            {
                FrameType = StackMapFrameType.SameLocals1StackItemFrame;
                OffsetDelta = (ushort)(Tag - 64);
                Stack = new VerificationTypeInfo[1] { new VerificationTypeInfo(ref data, constants) };
            }
            else if(Tag < 247)
            {
                throw new NotImplementedException();
            }
            else if(Tag == 247)
            {
                FrameType = StackMapFrameType.SameLocals1StackItemFrameExtended;
                OffsetDelta = data.ReadTwo();
                Stack = new VerificationTypeInfo[1] { new VerificationTypeInfo(ref data, constants) };
            }
            else if(Tag < 251)
            {
                FrameType = StackMapFrameType.ChopFrame;
                OffsetDelta = data.ReadTwo();
                K = 251 - Tag;
            }
            else if(Tag == 251)
            {
                FrameType = StackMapFrameType.SameFrameExtended;
                OffsetDelta = data.ReadTwo();
            }
            else if(Tag < 255)
            {
                FrameType = StackMapFrameType.AppendFrame;
                OffsetDelta = data.ReadTwo();
                Locals = new VerificationTypeInfo[Tag - 251];
                for(int i = 0; i < Locals.Length; i++)
                {
                    Locals[i] = new VerificationTypeInfo(ref data, constants);
                }
            }
            else
            {
                FrameType = StackMapFrameType.FullFrame;
                OffsetDelta = data.ReadTwo();

                ushort numberOfLocals = data.ReadTwo();
                Locals = new VerificationTypeInfo[numberOfLocals];
                for(int i = 0; i < numberOfLocals; i++)
                {
                    Locals[i] = new VerificationTypeInfo(ref data, constants);
                }

                ushort numberOfStackItems = data.ReadTwo();
                Stack = new VerificationTypeInfo[numberOfStackItems];
                for (int i = 0; i < numberOfStackItems; i++)
                {
                    Stack[i] = new VerificationTypeInfo(ref data, constants);
                }
            }
        }
    }

    public class StackMapTableAttribute : AttributeInfo
    {
        public ushort NumberOfEntries;
        public StackMapFrame[] Entries;

        public StackMapTableAttribute(ref ReadOnlySpan<byte> data, CPInfo[] constants) : base(ref data, constants)
        {
            //https://docs.oracle.com/javase/specs/jvms/se7/html/jvms-4.html#jvms-4.7.4
            ReadOnlySpan<byte> infoAsSpan = info.AsSpan();
            NumberOfEntries = infoAsSpan.ReadTwo();
            Entries = new StackMapFrame[NumberOfEntries];
            for (int i = 0; i < NumberOfEntries; i++)
            {
                Entries[i] = new StackMapFrame(ref infoAsSpan, constants);
            }
        }
    }
}
