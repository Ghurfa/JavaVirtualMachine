using JavaVirtualMachine.Attributes;
using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace JavaVirtualMachine
{
    public enum OpCodes
    {
        nop = 0x0,
        aconst_null = 0x1,
        iconst_m1 = 0x2,
        iconst_0 = 0x3,
        iconst_1 = 0x4,
        iconst_2 = 0x5,
        iconst_3 = 0x6,
        iconst_4 = 0x7,
        iconst_5 = 0x8,
        lconst_0 = 0x9,
        lconst_1 = 0x0a,
        fconst_0 = 0x0b,
        fconst_1 = 0x0c,
        fconst_2 = 0x0d,
        dconst_0 = 0x0e,
        dconst_1 = 0x0f,
        bipush = 0x10,
        sipush = 0x11,
        ldc = 0x12,
        ldc_w = 0x13,
        ldc2_w = 0x14,
        iload = 0x15,
        lload = 0x16,
        fload = 0x17,
        dload = 0x18,
        aload = 0x19,
        iload_0 = 0x1a,
        iload_1 = 0x1b,
        iload_2 = 0x1c,
        iload_3 = 0x1d,
        lload_0 = 0x1e,
        lload_1 = 0x1f,
        lload_2 = 0x20,
        lload_3 = 0x21,
        fload_0 = 0x22,
        fload_1 = 0x23,
        fload_2 = 0x24,
        fload_3 = 0x25,
        dload_0 = 0x26,
        dload_1 = 0x27,
        dload_2 = 0x28,
        dload_3 = 0x29,
        aload_0 = 0x2a,
        aload_1 = 0x2b,
        aload_2 = 0x2c,
        aload_3 = 0x2d,
        iaload = 0x2e,
        laload = 0x2f,
        faload = 0x30,
        daload = 0x31,
        aaload = 0x32,
        baload = 0x33,
        caload = 0x34,
        saload = 0x35,
        istore = 0x36,
        lstore = 0x37,
        fstore = 0x38,
        dstore = 0x39,
        astore = 0x3a,
        istore_0 = 0x3b,
        istore_1 = 0x3c,
        istore_2 = 0x3d,
        istore_3 = 0x3e,
        lstore_0 = 0x3f,
        lstore_1 = 0x40,
        lstore_2 = 0x41,
        lstore_3 = 0x42,
        fstore_0 = 0x43,
        fstore_1 = 0x44,
        fstore_2 = 0x45,
        fstore_3 = 0x46,
        dstore_0 = 0x47,
        dstore_1 = 0x48,
        dstore_2 = 0x49,
        dstore_3 = 0x4a,
        astore_0 = 0x4b,
        astore_1 = 0x4c,
        astore_2 = 0x4d,
        astore_3 = 0x4e,
        iastore = 0x4f,
        lastore = 0x50,
        fastore = 0x51,
        dastore = 0x52,
        aastore = 0x53,
        bastore = 0x54,
        castore = 0x55,
        sastore = 0x56,
        pop = 0x57,
        pop2 = 0x58,
        dup = 0x59,
        dup_x1 = 0x5a,
        dup_x2 = 0x5b,
        dup2 = 0x5c,
        dup2_x1 = 0x5d,
        dup2_x2 = 0x5e,
        swap = 0x5f,
        iadd = 0x60,
        ladd = 0x61,
        fadd = 0x62,
        dadd = 0x63,
        isub = 0x64,
        lsub = 0x65,
        fsub = 0x66,
        dsub = 0x67,
        imul = 0x68,
        lmul = 0x69,
        fmul = 0x6a,
        dmul = 0x6b,
        idiv = 0x6c,
        ldiv = 0x6d,
        fdiv = 0x6e,
        ddiv = 0x6f,
        irem = 0x70,
        lrem = 0x71,
        frem = 0x72,
        drem = 0x73,
        ineg = 0x74,
        lneg = 0x75,
        fneg = 0x76,
        dneg = 0x77,
        ishl = 0x78,
        lshl = 0x79,
        ishr = 0x7a,
        lshr = 0x7b,
        iushr = 0x7c,
        lushr = 0x7d,
        iand = 0x7e,
        land = 0x7f,
        ior = 0x80,
        lor = 0x81,
        ixor = 0x82,
        lxor = 0x83,
        iinc = 0x84,
        i2l = 0x85,
        i2f = 0x86,
        i2d = 0x87,
        l2i = 0x88,
        l2f = 0x89,
        l2d = 0x8a,
        f2i = 0x8b,
        f2l = 0x8c,
        f2d = 0x8d,
        d2i = 0x8e,
        d2l = 0x8f,
        d2f = 0x90,
        i2b = 0x91,
        i2c = 0x92,
        i2s = 0x93,
        lcmp = 0x94,
        fcmpl = 0x95,
        fcmpg = 0x96,
        dcmpl = 0x97,
        dcmpg = 0x98,
        ifeq = 0x99,
        ifne = 0x9a,
        iflt = 0x9b,
        ifge = 0x9c,
        ifgt = 0x9d,
        ifle = 0x9e,
        if_icmpeq = 0x9f,
        if_icmpne = 0xa0,
        if_icmplt = 0xa1,
        if_icmpge = 0xa2,
        if_icmpgt = 0xa3,
        if_icmple = 0xa4,
        if_acmpeq = 0xa5,
        if_acmpne = 0xa6,
        @goto = 0xa7,
        jsr = 0xa8,
        ret = 0xa9,
        tableswitch = 0xaa,
        lookupswitch = 0xab,
        ireturn = 0xac,
        lreturn = 0xad,
        freturn = 0xae,
        dreturn = 0xaf,
        areturn = 0xb0,
        @return = 0xb1,
        getstatic = 0xb2,
        putstatic = 0xb3,
        getfield = 0xb4,
        putfield = 0xb5,
        invokevirtual = 0xb6,
        invokespecial = 0xb7,
        invokestatic = 0xb8,
        invokeinterface = 0xb9,
        invokedynamic = 0xba,
        @new = 0xbb,
        newarray = 0xbc,
        anewarray = 0xbd,
        arraylength = 0xbe,
        athrow = 0xbf,
        checkcast = 0xc0,
        instanceof = 0xc1,
        monitorenter = 0xc2,
        monitorexit = 0xc3,
        wide = 0xc4,
        multianewarray = 0xc5,
        ifnull = 0xc6,
        ifnonnull = 0xc7,
        goto_w = 0xc8,
        jsr_w = 0xc9,
        breakpoint = 0xca,
        impdep1 = 0xfe,
        impdep2 = 0xff
    }
    public enum ArrayTypeCodes
    {
        T_BOOLEAN = 4,
        T_CHAR = 5,
        T_FLOAT = 6,
        T_DOUBLE = 7,
        T_BYTE = 8,
        T_SHORT = 9,
        T_INT = 10,
        T_LONG = 11
    }

    public class MethodFrame
    {
        public readonly ushort MaxStack;
        public readonly ushort MaxLocals;
        public int[] Stack;
        public int[] Locals;
        public readonly ReadOnlyMemory<byte> Code;
        public readonly ExceptionHandlerInfo[] ExceptionTable;
        public int sp;
        public readonly ClassFile ClassFile;
        public readonly MethodInfo MethodInfo;
        private int ip;

        public MethodFrame(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            MaxStack = methodInfo.MaxStack;
            MaxLocals = methodInfo.MaxLocals;
            if (methodInfo.CodeAttribute != null)
            {
                Code = methodInfo.CodeAttribute.Code;
                ExceptionTable = methodInfo.CodeAttribute.ExceptionTable;
            }
            if (methodInfo.HasFlag(MethodInfoFlag.Native))
            {
                Stack = new int[2];
            }
            else
            {
                Stack = new int[MaxStack];
            }
            Locals = new int[MaxLocals];
            ip = 0;
            sp = 0;
            ClassFile = methodInfo.ClassFile;
        }
        public virtual void Execute()
        {
            Program.MethodFrameStack.Push(this);
            ReadOnlySpan<byte> code = Code.Span;

            while (true)
            {
                try
                {
                    //TODO: Fix system.out.println
                    int oldIp = ip;
                    byte opCode = code[ip++];
                    bool wide = opCode == (int)OpCodes.wide;
                    if(wide)
                    {
                        opCode = code[ip++];
                    }
                    switch ((OpCodes)opCode)
                    {
                        case OpCodes.nop:
                            break;
                        case OpCodes.aconst_null:
                            Utility.Push(ref Stack, ref sp, 0);
                            break;
                        case OpCodes.iconst_m1:
                        case OpCodes.iconst_0:
                        case OpCodes.iconst_1:
                        case OpCodes.iconst_2:
                        case OpCodes.iconst_3:
                        case OpCodes.iconst_4:
                        case OpCodes.iconst_5:
                            Utility.Push(ref Stack, ref sp, opCode - 0x03);
                            break;
                        case OpCodes.lconst_0:
                            Utility.Push(ref Stack, ref sp, 0L);
                            break;
                        case OpCodes.lconst_1:
                            Utility.Push(ref Stack, ref sp, 1L);
                            break;
                        case OpCodes.fconst_0:
                            Utility.Push(ref Stack, ref sp, 0);
                            break;
                        case OpCodes.fconst_1:
                            Utility.Push(ref Stack, ref sp, 0x3f800000);
                            break;
                        case OpCodes.fconst_2:
                            Utility.Push(ref Stack, ref sp, 0x40000000);
                            break;
                        case OpCodes.dconst_0:
                            Utility.Push(ref Stack, ref sp, 0.0d);
                            break;
                        case OpCodes.dconst_1:
                            Utility.Push(ref Stack, ref sp, 1.0d);
                            break;
                        case OpCodes.bipush:
                            Utility.Push(ref Stack, ref sp, (sbyte)code[ip++]);
                            break;
                        case OpCodes.sipush:
                            Utility.Push(ref Stack, ref sp, (short)((code[ip++] << 8) | code[ip++]));
                            break;
                        case OpCodes.ldc:
                            {
                                CPInfo value = ClassFile.Constants[code[ip++]];

                                if (value.GetType() == typeof(CIntegerInfo))
                                {
                                    Utility.Push(ref Stack, ref sp, ((CIntegerInfo)value).Bytes);
                                }
                                else if (value.GetType() == typeof(CFloatInfo))
                                {
                                    Utility.Push(ref Stack, ref sp, (int)(((CFloatInfo)value).Bytes));
                                }
                                else if (value.GetType() == typeof(CStringInfo))
                                {
                                    string @string = ((CStringInfo)value).String;
                                    Utility.Push(ref Stack, ref sp, JavaHelper.CreateJavaStringLiteral(@string));
                                }
                                else if (value.GetType() == typeof(CClassInfo))
                                {
                                    Utility.Push(ref Stack, ref sp, ClassObjectManager.GetClassObjectAddr(((CClassInfo)value).Name));
                                }
                                else throw new NotImplementedException("Not supported");
                            }
                            break;
                        case OpCodes.ldc_w:
                            {
                                short index = readShort(code, ref ip);
                                CPInfo value = ClassFile.Constants[index];

                                if (value.GetType() == typeof(CIntegerInfo))
                                {
                                    Utility.Push(ref Stack, ref sp, ((CIntegerInfo)value).Bytes);
                                }
                                else if (value.GetType() == typeof(CFloatInfo))
                                {
                                    Utility.Push(ref Stack, ref sp, Stack[sp++] = (int)((CFloatInfo)value).Bytes);
                                }
                                else if (value.GetType() == typeof(CStringInfo))
                                {
                                    string @string = ((CStringInfo)value).String;
                                    Utility.Push(ref Stack, ref sp, JavaHelper.CreateJavaStringLiteral(@string));
                                }
                                else if (value.GetType() == typeof(CClassInfo))
                                {
                                    //fix based off ldc
                                    ClassFile cFile = ClassFileManager.GetClassFile(((CClassInfo)value).Name);
                                    Utility.Push(ref Stack, ref sp, Heap.AddItem(new HeapObject(cFile)));
                                }
                                else throw new NotImplementedException("Not supported");
                            }
                            break;
                        case OpCodes.ldc2_w:
                            {
                                short index = readShort(code, ref ip);
                                CPInfo value = ClassFile.Constants[index];
                                if (value.GetType() == typeof(CLongInfo))
                                {
                                    Utility.Push(ref Stack, ref sp, ((CLongInfo)value).LongValue);
                                }
                                else if (value.GetType() == typeof(CDoubleInfo))
                                {
                                    Utility.Push(ref Stack, ref sp, Stack[sp++] = (int)((CDoubleInfo)value).LongValue);
                                }
                                else throw new NotImplementedException("Not supported");
                            }
                            break;
                        case OpCodes.iload:
                        case OpCodes.fload:
                        case OpCodes.aload:
                            if(wide)
                            {
                                Utility.Push(ref Stack, ref sp, Locals[readShort(code, ref ip)]);
                            }
                            else
                            {
                                Utility.Push(ref Stack, ref sp, Locals[code[ip++]]);
                            }
                            break;
                        case OpCodes.lload:
                        case OpCodes.dload:
                            {
                                short index = wide ? readShort(code, ref ip) : code[ip++];
                                int high = Locals[index];
                                int low = Locals[index + 1];
                                Utility.Push(ref Stack, ref sp, high);
                                Utility.Push(ref Stack, ref sp, low);
                            }
                            break;
                        case OpCodes.iload_0:
                        case OpCodes.iload_1:
                        case OpCodes.iload_2:
                        case OpCodes.iload_3:
                            Utility.Push(ref Stack, ref sp, Locals[opCode - 0x1A]);
                            break;
                        case OpCodes.lload_0:
                        case OpCodes.lload_1:
                        case OpCodes.lload_2:
                        case OpCodes.lload_3:
                            {
                                int index = opCode - 0x1e;
                                int high = Locals[index];
                                int low = Locals[index + 1];
                                Utility.Push(ref Stack, ref sp, high);
                                Utility.Push(ref Stack, ref sp, low);
                                break;
                            }
                        case OpCodes.fload_0:
                        case OpCodes.fload_1:
                        case OpCodes.fload_2:
                        case OpCodes.fload_3:
                            Utility.Push(ref Stack, ref sp, Locals[opCode - 0x22]);  //Floats already stored as int
                            break;
                        case OpCodes.dload_0:
                        case OpCodes.dload_1:
                        case OpCodes.dload_2:
                        case OpCodes.dload_3:
                            {
                                int index = opCode - 0x26;
                                int high = Locals[index];
                                int low = Locals[index + 1];
                                Utility.Push(ref Stack, ref sp, high); //Doubles already stored as long
                                Utility.Push(ref Stack, ref sp, low);
                                break;
                            }
                        case OpCodes.aload_0:
                        case OpCodes.aload_1:
                        case OpCodes.aload_2:
                        case OpCodes.aload_3:
                            Utility.Push(ref Stack, ref sp, Locals[opCode - 0x2A]);  //References already stored as int
                            break;
                        case OpCodes.iaload:
                        case OpCodes.aaload:
                            {
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                Utility.Push(ref Stack, ref sp, ((int[])array.Array)[index]);
                            }
                            break;
                        case OpCodes.laload:
                            {
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                Utility.Push(ref Stack, ref sp, ((long[])array.Array)[index]);
                            }
                            break;
                        case OpCodes.faload:
                            {
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                Utility.Push(ref Stack, ref sp, ((float[])array.Array)[index]);
                            }
                            break;
                        case OpCodes.daload:
                            {
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                Utility.Push(ref Stack, ref sp, ((double[])array.Array)[index]);
                            }
                            break;
                        case OpCodes.baload:
                            {
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                if (array.Array is byte[] byteArr)
                                {
                                    Utility.Push(ref Stack, ref sp, (byteArr)[index]);
                                }
                                else
                                {
                                    Utility.Push(ref Stack, ref sp, ((bool[])array.Array)[index] ? 1 : 0);
                                }
                            }
                            break;
                        case OpCodes.caload:
                            {
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                Utility.Push(ref Stack, ref sp, ((char[])array.Array)[index]);
                                //make char obj?
                            }
                            break;
                        case OpCodes.saload:
                            {
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                Utility.Push(ref Stack, ref sp, ((short[])array.Array)[index]);
                            }
                            break;
                        case OpCodes.istore:
                        case OpCodes.fstore:
                        case OpCodes.astore:
                            {
                                int index = wide ? readShort(code, ref ip) : code[ip++];
                                int value = Utility.PopInt(Stack, ref sp);
                                Locals[index] = value;
                            }
                            break;
                        case OpCodes.lstore:
                        case OpCodes.dstore:
                            {
                                int index = wide ? readShort(code, ref ip) : code[ip++];
                                (int high, int low) = Utility.PopLong(Stack, ref sp).Split();
                                Locals[index] = high;
                                Locals[index + 1] = low;
                            }
                            break;
                        case OpCodes.istore_0:
                        case OpCodes.istore_1:
                        case OpCodes.istore_2:
                        case OpCodes.istore_3:
                            Locals[opCode - 0x3B] = Stack[--sp];
                            break;
                        case OpCodes.lstore_0:
                        case OpCodes.lstore_1:
                        case OpCodes.lstore_2:
                        case OpCodes.lstore_3:
                            {
                                (int high, int low) = Utility.PopLong(Stack, ref sp).Split();
                                Locals[opCode - 0x3F] = high;
                                Locals[opCode - 0x3E] = low;
                            }
                            break;
                        case OpCodes.fstore_0:
                        case OpCodes.fstore_1:
                        case OpCodes.fstore_2:
                        case OpCodes.fstore_3:
                            Locals[opCode - 0x43] = Stack[--sp];
                            break;
                        case OpCodes.dstore_0:
                        case OpCodes.dstore_1:
                        case OpCodes.dstore_2:
                        case OpCodes.dstore_3:
                            {
                                (int high, int low) = Utility.PopLong(Stack, ref sp).Split();
                                Locals[opCode - 0x47] = high;
                                Locals[opCode - 0x46] = low;
                            }
                            break;
                        case OpCodes.astore_0:
                        case OpCodes.astore_1:
                        case OpCodes.astore_2:
                        case OpCodes.astore_3:
                            Locals[opCode - 0x4B] = Utility.PopInt(Stack, ref sp);
                            break;
                        case OpCodes.iastore:
                        case OpCodes.aastore:
                            {
                                int value = Utility.PopInt(Stack, ref sp);
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                ((int[])array.Array)[index] = value;
                            }
                            break;
                        case OpCodes.lastore:
                            {
                                long value = Utility.PopLong(Stack, ref sp);
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                ((long[])array.Array)[index] = value;
                            }
                            break;
                        case OpCodes.fastore:
                            {
                                int valueAsInt = Utility.PopInt(Stack, ref sp);
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                ((float[])array.Array)[index] = JavaHelper.StoredFloatToFloat(valueAsInt);
                            }
                            break;
                        case OpCodes.dastore:
                            {
                                long valueAsLong = Utility.PopLong(Stack, ref sp);
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                ((double[])array.Array)[index] = JavaHelper.StoredDoubleToDouble(valueAsLong);
                            }
                            break;
                        case OpCodes.bastore:
                            {
                                int valueAsInt = Utility.PopInt(Stack, ref sp);
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                if (array.Array is byte[] byteArr)
                                {
                                    (byteArr)[index] = (byte)valueAsInt;
                                }
                                else
                                {
                                    ((bool[])array.Array)[index] = valueAsInt != 0;
                                }
                            }
                            break;
                        case OpCodes.castore:
                            {
                                int valueAsInt = Utility.PopInt(Stack, ref sp);
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                ((char[])array.Array)[index] = (char)valueAsInt;
                            }
                            break;
                        case OpCodes.sastore:
                            {
                                int valueAsInt = Utility.PopInt(Stack, ref sp);
                                int index = Utility.PopInt(Stack, ref sp);
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                if (arrayRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                ((short[])array.Array)[index] = (short)valueAsInt;
                            }
                            break;
                        case OpCodes.pop:
                            sp--;
                            break;
                        case OpCodes.pop2:
                            sp -= 2;
                            break;
                        #region Stack Copy & Reverse Op-Codes
                        case OpCodes.dup:
                            Utility.Push(ref Stack, ref sp, Utility.PeekInt(Stack, sp));
                            break;
                        case OpCodes.dup_x1:
                            Stack[sp] = Stack[sp - 1];
                            Stack[sp - 1] = Stack[sp - 2];
                            Stack[sp - 2] = Stack[sp++];
                            break;
                        case OpCodes.dup_x2:
                            Stack[sp] = Stack[sp - 1];
                            Stack[sp - 1] = Stack[sp - 2];
                            Stack[sp - 2] = Stack[sp - 3];
                            Stack[sp - 3] = Stack[sp++];
                            break;
                        case OpCodes.dup2:
                            Utility.Push(ref Stack, ref sp, Utility.PeekInt(Stack, sp, 1));
                            Utility.Push(ref Stack, ref sp, Utility.PeekInt(Stack, sp, 1));
                            break;
                        case OpCodes.dup2_x1:
                            Utility.Push(ref Stack, ref sp, Utility.PeekInt(Stack, sp, 1));
                            Utility.Push(ref Stack, ref sp, Utility.PeekInt(Stack, sp, 1));
                            Stack[sp - 3] = Stack[sp - 5];
                            Stack[sp - 4] = Stack[sp - 1];
                            Stack[sp - 5] = Stack[sp - 2];
                            break;
                        case OpCodes.dup2_x2:
                            Utility.Push(ref Stack, ref sp, Utility.PeekInt(Stack, sp, 1));
                            Utility.Push(ref Stack, ref sp, Utility.PeekInt(Stack, sp, 1));
                            Stack[sp - 3] = Stack[sp - 5];
                            Stack[sp - 4] = Stack[sp - 6];
                            Stack[sp - 5] = Stack[sp - 1];
                            Stack[sp - 6] = Stack[sp - 2];
                            break;
                        case OpCodes.swap:
                            {
                                int temp = Stack[sp - 2];
                                Stack[sp - 2] = Stack[sp - 1];
                                Stack[sp - 1] = temp;
                            }
                            break;
                        #endregion
                        #region Arithmetic Op-Codes
                        case OpCodes.iadd:
                            {
                                int second = Utility.PopInt(Stack, ref sp);
                                int first = Utility.PopInt(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first + second);
                            }
                            break;
                        case OpCodes.ladd:
                            {
                                long second = Utility.PopLong(Stack, ref sp);
                                long first = Utility.PopLong(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first + second);
                            }
                            break;
                        case OpCodes.fadd:
                            {
                                float second = Utility.PopFloat(Stack, ref sp);
                                float first = Utility.PopFloat(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first + second);
                            }
                            break;
                        case OpCodes.dadd:
                            {
                                double second = Utility.PopDouble(Stack, ref sp);
                                double first = Utility.PopDouble(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first + second);
                            }
                            break;
                        case OpCodes.isub:
                            {
                                int second = Utility.PopInt(Stack, ref sp);
                                int first = Utility.PopInt(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first - second);
                            }
                            break;
                        case OpCodes.lsub:
                            {
                                long second = Utility.PopLong(Stack, ref sp);
                                long first = Utility.PopLong(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first - second);
                            }
                            break;
                        case OpCodes.fsub:
                            {
                                float second = Utility.PopFloat(Stack, ref sp);
                                float first = Utility.PopFloat(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first - second);
                            }
                            break;
                        case OpCodes.dsub:
                            {
                                double second = Utility.PopDouble(Stack, ref sp);
                                double first = Utility.PopDouble(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first - second);
                            }
                            break;
                        case OpCodes.imul:
                            {
                                int second = Utility.PopInt(Stack, ref sp);
                                int first = Utility.PopInt(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first * second);
                            }
                            break;
                        case OpCodes.lmul:
                            {
                                long second = Utility.PopLong(Stack, ref sp);
                                long first = Utility.PopLong(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first * second);
                            }
                            break;
                        case OpCodes.fmul:
                            {
                                float second = Utility.PopFloat(Stack, ref sp);
                                float first = Utility.PopFloat(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first * second);
                            }
                            break;
                        case OpCodes.dmul:
                            {
                                double second = Utility.PopDouble(Stack, ref sp);
                                double first = Utility.PopDouble(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first * second);
                            }
                            break;
                        case OpCodes.idiv:
                            {
                                int second = Utility.PopInt(Stack, ref sp);
                                int first = Utility.PopInt(Stack, ref sp);
                                if (second == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/ArithmeticException");
                                }
                                Utility.Push(ref Stack, ref sp, first / second);
                            }
                            break;
                        case OpCodes.ldiv:
                            {
                                long second = Utility.PopLong(Stack, ref sp);
                                long first = Utility.PopLong(Stack, ref sp);
                                if (second == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/ArithmeticException");
                                }
                                Utility.Push(ref Stack, ref sp, first / second);
                            }
                            break;
                        case OpCodes.fdiv:
                            {
                                float second = Utility.PopFloat(Stack, ref sp);
                                float first = Utility.PopFloat(Stack, ref sp);
                                if (second == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/ArithmeticException");
                                }
                                Utility.Push(ref Stack, ref sp, first / second);
                            }
                            break;
                        case OpCodes.ddiv:
                            {
                                double second = Utility.PopDouble(Stack, ref sp);
                                double first = Utility.PopDouble(Stack, ref sp);
                                if (second == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/ArithmeticException");
                                }
                                Utility.Push(ref Stack, ref sp, first / second);
                            }
                            break;
                        case OpCodes.irem:
                            {
                                int second = Utility.PopInt(Stack, ref sp);
                                int first = Utility.PopInt(Stack, ref sp);
                                if (second == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/ArithmeticException");
                                }
                                Utility.Push(ref Stack, ref sp, first % second);
                            }
                            break;
                        case OpCodes.lrem:
                            {
                                long second = Utility.PopLong(Stack, ref sp);
                                long first = Utility.PopLong(Stack, ref sp);
                                if (second == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/ArithmeticException");
                                }
                                else
                                {
                                    Utility.Push(ref Stack, ref sp, first % second);
                                }
                            }
                            break;
                        case OpCodes.frem:
                            {
                                float second = Utility.PopFloat(Stack, ref sp);
                                float first = Utility.PopFloat(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first % second);
                            }
                            break;
                        case OpCodes.drem:
                            {
                                double second = Utility.PopDouble(Stack, ref sp);
                                double first = Utility.PopDouble(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first % second);
                            }
                            break;
                        case OpCodes.ineg: //arithmetic negation, not bitwise
                            Stack[sp - 1] = -Stack[sp - 1];
                            break;
                        case OpCodes.lneg:
                            Utility.Push(ref Stack, ref sp, -Utility.PopLong(Stack, ref sp));
                            break;
                        case OpCodes.fneg:
                            Utility.Push(ref Stack, ref sp, -Utility.PopFloat(Stack, ref sp));
                            break;
                        case OpCodes.dneg:
                            Utility.Push(ref Stack, ref sp, -Utility.PopDouble(Stack, ref sp));
                            break;
                        #endregion
                        #region Bitwise Op-Codes
                        case OpCodes.ishl:
                            {
                                int shiftBy = Utility.PopInt(Stack, ref sp);
                                int num = Utility.PopInt(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, num << shiftBy);
                            }
                            break;
                        case OpCodes.lshl:
                            {
                                int shiftBy = Utility.PopInt(Stack, ref sp);
                                long num = Utility.PopLong(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, num << shiftBy);
                            }
                            break;
                        case OpCodes.ishr:
                            {
                                int shiftBy = Utility.PopInt(Stack, ref sp);
                                int num = Utility.PopInt(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, num >> shiftBy);
                            }
                            break;
                        case OpCodes.lshr:
                            {
                                int shiftBy = Utility.PopInt(Stack, ref sp);
                                long num = Utility.PopLong(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, num >> shiftBy);
                            }
                            break;
                        case OpCodes.iushr:
                            {
                                int shiftBy = Utility.PopInt(Stack, ref sp);
                                int num = Utility.PopInt(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, (int)((uint)num >> shiftBy));
                            }
                            break;
                        case OpCodes.lushr:
                            {
                                int shiftBy = Utility.PopInt(Stack, ref sp);
                                long num = Utility.PopLong(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, (long)((ulong)num >> shiftBy));
                            }
                            break;
                        case OpCodes.iand:
                            {
                                int second = Utility.PopInt(Stack, ref sp);
                                int first = Utility.PopInt(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first & second);
                            }
                            break;
                        case OpCodes.land:
                            {
                                long second = Utility.PopLong(Stack, ref sp);
                                long first = Utility.PopLong(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first & second);
                            }
                            break;
                        case OpCodes.ior:
                            {
                                int second = Utility.PopInt(Stack, ref sp);
                                int first = Utility.PopInt(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first | second);
                            }
                            break;
                        case OpCodes.lor:
                            {
                                long second = Utility.PopLong(Stack, ref sp);
                                long first = Utility.PopLong(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first | second);
                            }
                            break;
                        case OpCodes.ixor:
                            {
                                int second = Utility.PopInt(Stack, ref sp);
                                int first = Utility.PopInt(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first ^ second);
                            }
                            break;
                        case OpCodes.lxor:
                            {
                                long second = Utility.PopLong(Stack, ref sp);
                                long first = Utility.PopLong(Stack, ref sp);
                                Utility.Push(ref Stack, ref sp, first ^ second);
                            }
                            break;
                        case OpCodes.iinc:
                            {
                                short index = wide ? readShort(code, ref ip) : code[ip++];
                                sbyte incrementBy = (sbyte)code[ip++];
                                Locals[index] += incrementBy;
                            }
                            break;
                        #endregion
                        #region Stack Casting Op-Codes
                        case OpCodes.i2l:
                            Utility.Push(ref Stack, ref sp, (long)Utility.PopInt(Stack, ref sp));
                            break;
                        case OpCodes.i2f:
                            Utility.Push(ref Stack, ref sp, (float)Utility.PopInt(Stack, ref sp));
                            break;
                        case OpCodes.i2d:
                            Utility.Push(ref Stack, ref sp, (double)Utility.PopInt(Stack, ref sp));
                            break;
                        case OpCodes.l2i:
                            Utility.Push(ref Stack, ref sp, (int)Utility.PopLong(Stack, ref sp));
                            break;
                        case OpCodes.l2f:
                            Utility.Push(ref Stack, ref sp, (float)Utility.PopLong(Stack, ref sp));
                            break;
                        case OpCodes.l2d:
                            Utility.Push(ref Stack, ref sp, (double)Utility.PopLong(Stack, ref sp));
                            break;
                        case OpCodes.f2i:
                            Utility.Push(ref Stack, ref sp, (int)Utility.PopFloat(Stack, ref sp));
                            break;
                        case OpCodes.f2l:
                            Utility.Push(ref Stack, ref sp, (long)Utility.PopFloat(Stack, ref sp));
                            break;
                        case OpCodes.f2d:
                            Utility.Push(ref Stack, ref sp, (double)Utility.PopFloat(Stack, ref sp));
                            break;
                        case OpCodes.d2i:
                            Utility.Push(ref Stack, ref sp, (int)Utility.PopDouble(Stack, ref sp));
                            break;
                        case OpCodes.d2l:
                            Utility.Push(ref Stack, ref sp, (long)Utility.PopDouble(Stack, ref sp));
                            break;
                        case OpCodes.d2f:
                            Utility.Push(ref Stack, ref sp, (float)Utility.PopDouble(Stack, ref sp));
                            break;
                        case OpCodes.i2b:
                            Stack[sp - 1] = (byte)Stack[sp - 1];
                            break;
                        case OpCodes.i2c:
                            Stack[sp - 1] = (char)Stack[sp - 1];
                            break;
                        case OpCodes.i2s:
                            Stack[sp - 1] = (short)Stack[sp - 1];
                            break;
                        #endregion
                        #region Non-Int Compare Op-Codes
                        case OpCodes.lcmp:
                            {
                                long value2 = Utility.PopLong(Stack, ref sp);
                                long value1 = Utility.PopLong(Stack, ref sp);
                                if (value1 == value2)
                                {
                                    Utility.Push(ref Stack, ref sp, 0);
                                }
                                else if (value1 > value2)
                                {
                                    Utility.Push(ref Stack, ref sp, 1);
                                }
                                else
                                {
                                    Utility.Push(ref Stack, ref sp, -1);
                                }
                                break;
                            }
                        case OpCodes.fcmpl:
                        case OpCodes.fcmpg:
                            {
                                float value2 = Utility.PopFloat(Stack, ref sp);
                                float value1 = Utility.PopFloat(Stack, ref sp);
                                if (float.IsNaN(value1) || float.IsNaN(value2))
                                {
                                    if ((OpCodes)opCode == OpCodes.fcmpl)
                                    {
                                        Utility.Push(ref Stack, ref sp, -1);
                                    }
                                    else
                                    {
                                        Utility.Push(ref Stack, ref sp, 1);
                                    }
                                }
                                else
                                {
                                    if (value1 > value2)
                                    {
                                        Utility.Push(ref Stack, ref sp, 1);
                                    }
                                    else if (value1 == value2)
                                    {
                                        Utility.Push(ref Stack, ref sp, 0);
                                    }
                                    else if (value1 < value2)
                                    {
                                        Utility.Push(ref Stack, ref sp, -1);
                                    }
                                }
                                break;
                            }
                        case OpCodes.dcmpl:
                        case OpCodes.dcmpg:
                            {
                                double value2 = Utility.PopDouble(Stack, ref sp);
                                double value1 = Utility.PopDouble(Stack, ref sp);
                                if (double.IsNaN(value1) || double.IsNaN(value2))
                                {
                                    if ((OpCodes)opCode == OpCodes.fcmpl)
                                    {
                                        Utility.Push(ref Stack, ref sp, -1);
                                    }
                                    else
                                    {
                                        Utility.Push(ref Stack, ref sp, 1);
                                    }
                                }
                                else
                                {
                                    if (value1 > value2)
                                    {
                                        Utility.Push(ref Stack, ref sp, 1);
                                    }
                                    else if (value1 == value2)
                                    {
                                        Utility.Push(ref Stack, ref sp, 0);
                                    }
                                    else if (value1 < value2)
                                    {
                                        Utility.Push(ref Stack, ref sp, -1);
                                    }
                                }
                                break;
                            }
                        #endregion
                        #region Jump Op-Codes
                        case OpCodes.ifeq:
                            {
                                short offset = readShort(code, ref ip);
                                int value = Utility.PopInt(Stack, ref sp);
                                if (value == 0) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.ifne:
                            {
                                short offset = readShort(code, ref ip);
                                int value = Utility.PopInt(Stack, ref sp);
                                if (value != 0) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.iflt:
                            {
                                short offset = readShort(code, ref ip);
                                int value = Utility.PopInt(Stack, ref sp);
                                if (value < 0) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.ifge:
                            {
                                short offset = readShort(code, ref ip);
                                int value = Utility.PopInt(Stack, ref sp);
                                if (value >= 0) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.ifgt:
                            {
                                short offset = readShort(code, ref ip);
                                int value = Utility.PopInt(Stack, ref sp);
                                if (value > 0) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.ifle:
                            {
                                short offset = readShort(code, ref ip);
                                int value = Utility.PopInt(Stack, ref sp);
                                if (value <= 0) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.if_icmpeq:
                        case OpCodes.if_acmpeq:
                            {
                                short offset = readShort(code, ref ip);
                                int value2 = Utility.PopInt(Stack, ref sp);
                                int value1 = Utility.PopInt(Stack, ref sp);
                                if (value1 == value2) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.if_icmpne:
                        case OpCodes.if_acmpne:
                            {
                                short offset = readShort(code, ref ip);
                                int value2 = Utility.PopInt(Stack, ref sp);
                                int value1 = Utility.PopInt(Stack, ref sp);
                                if (value1 != value2) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.if_icmplt:
                            {
                                short offset = readShort(code, ref ip);
                                int value2 = Utility.PopInt(Stack, ref sp);
                                int value1 = Utility.PopInt(Stack, ref sp);
                                if (value1 < value2) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.if_icmpge:
                            {
                                short offset = readShort(code, ref ip);
                                int value2 = Utility.PopInt(Stack, ref sp);
                                int value1 = Utility.PopInt(Stack, ref sp);
                                if (value1 >= value2) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.if_icmpgt:
                            {
                                short offset = readShort(code, ref ip);
                                int value2 = Utility.PopInt(Stack, ref sp);
                                int value1 = Utility.PopInt(Stack, ref sp);
                                if (value1 > value2) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.if_icmple:
                            {
                                short offset = readShort(code, ref ip);
                                int value2 = Utility.PopInt(Stack, ref sp);
                                int value1 = Utility.PopInt(Stack, ref sp);
                                if (value1 <= value2) ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.@goto:
                            {
                                short offset = readShort(code, ref ip);
                                ip = oldIp + offset;
                            }
                            break;
                        #endregion
                        case OpCodes.jsr:
                            {
                                short offset = readShort(code, ref ip);
                                int retAddress = ip;
                                Utility.Push(ref Stack, ref sp, retAddress);
                                ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.ret:
                            {
                                short index = wide ? readShort(code, ref ip) : code[ip++];
                                int retAddress = Locals[index];
                                ip = retAddress;
                            }
                            break;
                        case OpCodes.tableswitch:
                            {
                                int index = Utility.PopInt(Stack, ref sp);
                                ip += (3 - ((ip - 1) % 4)); //Moves to next multiple of 4

                                int defaultOffset = readInt(code, ref ip); //What to do w/ this?

                                int low = readInt(code, ref ip);
                                int high = readInt(code, ref ip);

                                if (low > high) throw new InvalidDataException();

                                int numOfPairs = high - low + 1;

                                if (index < low || index > high)
                                {
                                    ip = oldIp + defaultOffset;
                                }
                                else
                                {
                                    ip += 4 * (index - low);
                                    int offset = readInt(code, ref ip);
                                    ip = oldIp + offset;
                                }
                                break;
                            }
                        case OpCodes.lookupswitch:
                            {
                                int searchKey = Utility.PopInt(Stack, ref sp);
                                ip += (3 - ((ip - 1) % 4)); //Moves to next multiple of 4

                                int defaultOffset = readInt(code, ref ip);

                                int numOfPairs = readInt(code, ref ip);

                                bool foundMatch = false;
                                for (int i = 0; i < numOfPairs; i++)
                                {
                                    int key = readInt(code, ref ip);
                                    int offset = readInt(code, ref ip);
                                    if (searchKey == key)
                                    {
                                        foundMatch = true;
                                        ip = oldIp + offset;
                                        break;
                                    }
                                }
                                if (!foundMatch)
                                {
                                    ip = oldIp + defaultOffset;
                                }
                                break;
                            }
                        case OpCodes.ireturn:
                        case OpCodes.freturn:
                        case OpCodes.areturn:
                            if (sp != 1) throw new InvalidOperationException("Wrong number of items in the stack");
                            JavaHelper.ReturnValue(Utility.PopInt(Stack, ref sp));
                            return;
                        case OpCodes.lreturn:
                        case OpCodes.dreturn:
                            {
                                if (sp != 2) throw new InvalidOperationException("Wrong number of items in the stack");
                                JavaHelper.ReturnLargeValue(Utility.PopLong(Stack, ref sp));
                                return;
                            }
                        case OpCodes.@return:
                            if (sp != 0) throw new InvalidOperationException("Wrong number of items in the stack");
                            JavaHelper.ReturnVoid();
                            return;
                        case OpCodes.getstatic:
                            {
                                short index = readShort(code, ref ip);
                                CFieldRefInfo fieldRef = (CFieldRefInfo)ClassFile.Constants[index];
                                ClassFile cFile = ClassFileManager.GetClassFile(fieldRef.ClassName);
                                ClassFileManager.InitializeClass(fieldRef.ClassName);

                                //todo: superinterface? https://docs.oracle.com/javase/specs/jvms/se7/html/jvms-5.html#jvms-5.4.3.2

                                FieldValue fieldValue;
                                while (!cFile.StaticFieldsDictionary.TryGetValue((fieldRef.Name, fieldRef.Descriptor), out fieldValue))
                                {
                                    cFile = cFile.SuperClass;
                                }

                                /*if (fieldRef.ClassName == "java/lang/ref/SoftReference" && fieldRef.Name == "clock" && fieldRef.Descriptor == "J")
                                {
                                    Utility.Push(ref Stack, ref sp, DateTime.Now.Ticks);
                                }*/
                                if (fieldValue.GetType() == typeof(FieldNumber))
                                {
                                    int value = ((FieldNumber)fieldValue).Value;
                                    Utility.Push(ref Stack, ref sp, value);
                                }
                                else if (fieldValue.GetType() == typeof(FieldLargeNumber))
                                {
                                    long value = ((FieldLargeNumber)fieldValue).Value;
                                    Utility.Push(ref Stack, ref sp, value);
                                }
                                else if (fieldValue.GetType() == typeof(FieldReferenceValue))
                                {
                                    int value = ((FieldReferenceValue)fieldValue).Address;
                                    Utility.Push(ref Stack, ref sp, value);
                                }
                            }
                            break;
                        case OpCodes.putstatic:
                            {
                                short index = readShort(code, ref ip);
                                CFieldRefInfo fieldRef = (CFieldRefInfo)ClassFile.Constants[index];
                                ClassFile cFile = ClassFileManager.GetClassFile(fieldRef.ClassName);
                                ClassFileManager.InitializeClass(fieldRef.ClassName);
                                switch (fieldRef.Descriptor[0])
                                {
                                    case 'Z':
                                    case 'B':
                                    case 'C':
                                    case 'S':
                                    case 'I':
                                    case 'F':
                                        {
                                            int value = Utility.PopInt(Stack, ref sp);
                                            cFile.StaticFieldsDictionary[(fieldRef.Name, fieldRef.Descriptor)] = new FieldNumber(value);
                                        }
                                        break;
                                    case 'D':
                                    case 'J':
                                        {
                                            long value = Utility.PopLong(Stack, ref sp);
                                            cFile.StaticFieldsDictionary[(fieldRef.Name, fieldRef.Descriptor)] = new FieldLargeNumber(value);
                                        }
                                        break;
                                    case 'L':
                                    case '[':
                                        {
                                            int valueRef = Utility.PopInt(Stack, ref sp);
                                            cFile.StaticFieldsDictionary[(fieldRef.Name, fieldRef.Descriptor)] = new FieldReferenceValue(valueRef);
                                        }
                                        break;
                                }
                            }
                            break;
                        case OpCodes.getfield:
                            {
                                short index = readShort(code, ref ip);
                                int objectRef = Utility.PopInt(Stack, ref sp);

                                if (objectRef == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }

                                CFieldRefInfo fieldRef = (CFieldRefInfo)ClassFile.Constants[index];
                                HeapObject heapObj = Heap.GetObject(objectRef);
                                var fieldValue = heapObj.GetField(fieldRef.Name, fieldRef.Descriptor);
                                if (fieldValue.GetType() == typeof(FieldNumber))
                                {
                                    Utility.Push(ref Stack, ref sp, ((FieldNumber)fieldValue).Value);
                                }
                                else if (fieldValue.GetType() == typeof(FieldLargeNumber))
                                {
                                    Utility.Push(ref Stack, ref sp, ((FieldLargeNumber)fieldValue).Value);
                                }
                                else if (fieldValue.GetType() == typeof(FieldReferenceValue))
                                {
                                    Utility.Push(ref Stack, ref sp, ((FieldReferenceValue)fieldValue).Address);
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }
                            }
                            break;
                        case OpCodes.putfield:
                            {
                                short index = readShort(code, ref ip);
                                CFieldRefInfo fieldRef = (CFieldRefInfo)ClassFile.Constants[index];
                                switch (fieldRef.Descriptor[0])
                                {
                                    case 'Z':
                                    case 'B':
                                    case 'C':
                                    case 'S':
                                    case 'I':
                                    case 'F':
                                        {
                                            int value = Utility.PopInt(Stack, ref sp);
                                            int objectRef = Utility.PopInt(Stack, ref sp);
                                            HeapObject heapObj = Heap.GetObject(objectRef);
                                            heapObj.SetField(fieldRef.Name, fieldRef.Descriptor, new FieldNumber(value));
                                        }
                                        break;
                                    case 'D':
                                    case 'J':
                                        {
                                            long value = Utility.PopLong(Stack, ref sp);
                                            int objectRef = Utility.PopInt(Stack, ref sp);
                                            HeapObject heapObj = Heap.GetObject(objectRef);
                                            heapObj.SetField(fieldRef.Name, fieldRef.Descriptor, new FieldLargeNumber(value));
                                        }
                                        break;
                                    case 'L':
                                    case '[':
                                        {
                                            int valueRef = Utility.PopInt(Stack, ref sp);
                                            int objectRef = Utility.PopInt(Stack, ref sp);
                                            HeapObject heapObj = Heap.GetObject(objectRef);
                                            heapObj.SetField(fieldRef.Name, fieldRef.Descriptor, new FieldReferenceValue(valueRef));
                                        }
                                        break;
                                }
                            }
                            break;
                        case OpCodes.invokevirtual:
                        case OpCodes.invokespecial:
                            {
                                //Get method ref
                                short index = readShort(code, ref ip);
                                CMethodRefInfo methodRef = (CMethodRefInfo)ClassFile.Constants[index];

                                //Get args
                                int[] arguments = new int[methodRef.NumOfArgs() + 1];
                                for (int i = arguments.Length - 1; i >= 0; i--)
                                {
                                    arguments[i] = Utility.PopInt(Stack, ref sp);
                                }

                                if (arguments[0] == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }

                                ClassFile cFile;
                                if ((OpCodes)opCode == OpCodes.invokevirtual)
                                {
                                    string objectRefClassFileName = ((HeapObject)(Heap.GetItem(arguments[0]))).ClassFile.Name;
                                    cFile = ClassFileManager.GetClassFile(objectRefClassFileName);
                                }
                                else
                                {
                                    CClassInfo cFileInfo = (CClassInfo)ClassFile.Constants[methodRef.ClassIndex];
                                    cFile = ClassFileManager.GetClassFile(cFileInfo.Name);
                                }

                                //Search for method in cFile's staticMethodDictionary. If it's not there, repeat search in cFile's super and so on
                                MethodInfo method;
                                while (!cFile.MethodDictionary.TryGetValue((methodRef.Name, methodRef.Descriptor), out method))
                                {
                                    cFile = cFile.SuperClass;
                                }

                                if (method.HasFlag(MethodInfoFlag.Native))
                                {
                                    DebugWriter.CallFuncDebugWrite(method, arguments);
                                    NativeMethodFrame nativeMethodFrame = new NativeMethodFrame(method)
                                    {
                                        Args = arguments
                                    };
                                    nativeMethodFrame.Execute();
                                }
                                else
                                {
                                    DebugWriter.CallFuncDebugWrite(method, arguments);
                                    MethodFrame methodFrame = new MethodFrame(method);
                                    arguments.CopyTo(methodFrame.Locals, 0);
                                    methodFrame.Execute();
                                }
                            }
                            break;
                        case OpCodes.invokestatic:
                            {
                                short index = readShort(code, ref ip);

                                CMethodRefInfo methodRef = (CMethodRefInfo)ClassFile.Constants[index];

                                ClassFile cFile = ClassFileManager.GetClassFile(methodRef.ClassName);
                                ClassFileManager.InitializeClass(methodRef.ClassName);

                                int[] arguments = new int[methodRef.NumOfArgs()];
                                for (int i = arguments.Length - 1; i >= 0; i--)
                                {
                                    arguments[i] = Utility.PopInt(Stack, ref sp);
                                }

                                //Search for method in cFile's staticMethodDictionary. If it's not there, repeat search in cFile's super and so on
                                MethodInfo method;
                                while (!cFile.MethodDictionary.TryGetValue((methodRef.Name, methodRef.Descriptor), out method))
                                {
                                    cFile = cFile.SuperClass;
                                }

                                if (method.Name == "searchFields")
                                {

                                }

                                if (!method.HasFlag(MethodInfoFlag.Native))
                                {
                                    DebugWriter.CallFuncDebugWrite(method, arguments);
                                    MethodFrame methodFrame = new MethodFrame(method);
                                    arguments.CopyTo(methodFrame.Locals, 0);
                                    methodFrame.Execute();
                                }
                                else
                                {
                                    DebugWriter.CallFuncDebugWrite(method, arguments);
                                    NativeMethodFrame nativeMethodFrame = new NativeMethodFrame(method)
                                    {
                                        Args = arguments
                                    };
                                    nativeMethodFrame.Execute();
                                }
                            }
                            break;
                        case OpCodes.invokeinterface:
                            {
                                short index = readShort(code, ref ip);
                                CInterfaceMethodRefInfo interfaceMethodRef = (CInterfaceMethodRefInfo)ClassFile.Constants[index];

                                byte count = code[ip++];
                                if (count == 0) throw new InvalidOperationException();

                                ip++; //Skip the zero

                                int[] arguments = new int[interfaceMethodRef.NumOfArgs() + 1];
                                for (int i = arguments.Length - 1; i >= 0; i--)
                                {
                                    arguments[i] = Utility.PopInt(Stack, ref sp);
                                }

                                if (arguments[0] == 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                                }

                                string objectRefClassFileName = ((HeapObject)(Heap.GetItem(arguments[0]))).ClassFile.Name;
                                ClassFile cFile = ClassFileManager.GetClassFile(objectRefClassFileName);

                                //Search for method in cFile's methodDictionary. If it's not there, repeat search in cFile's super and so on
                                MethodInfo method;
                                while (!cFile.MethodDictionary.TryGetValue((interfaceMethodRef.Name, interfaceMethodRef.Descriptor), out method))
                                {
                                    cFile = cFile.SuperClass;
                                    if (cFile == null)
                                    {
                                        JavaHelper.ThrowJavaException("java/lang/AbstractMethodError");
                                    }
                                }

                                if (!method.HasFlag(MethodInfoFlag.Native))
                                {
                                    DebugWriter.CallFuncDebugWrite(interfaceMethodRef, arguments, cFile.Name);
                                    MethodFrame methodFrame = new MethodFrame(method);
                                    arguments.CopyTo(methodFrame.Locals, 0);
                                    methodFrame.Execute();
                                }
                                else
                                {
                                    DebugWriter.CallFuncDebugWrite(interfaceMethodRef, arguments, cFile.Name);
                                    NativeMethodFrame nativeMethodFrame = new NativeMethodFrame(method)
                                    {
                                        Args = arguments
                                    };
                                    nativeMethodFrame.Execute();
                                }
                            }
                            break;
                        case OpCodes.@new:
                            {
                                short index = readShort(code, ref ip);
                                CClassInfo classInfo = (CClassInfo)ClassFile.Constants[index];
                                ClassFile cFile = ClassFileManager.GetClassFile(classInfo.Name);
                                ClassFileManager.InitializeClass(classInfo.Name);
                                HeapObject heapObject = new HeapObject(cFile);
                                Utility.Push(ref Stack, ref sp, Heap.AddItem(heapObject));
                            }
                            break;
                        case OpCodes.newarray:
                            {
                                byte aType = code[ip++];
                                int count = Utility.PopInt(Stack, ref sp);
                                if (count < 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NegativeArraySizeException");
                                }
                                switch ((ArrayTypeCodes)aType)
                                {
                                    case ArrayTypeCodes.T_BOOLEAN:
                                        Utility.Push(ref Stack, ref sp, Heap.AddItem(new HeapArray(new bool[count], ClassObjectManager.GetClassObjectAddr("boolean"))));
                                        break;
                                    case ArrayTypeCodes.T_CHAR:
                                        Utility.Push(ref Stack, ref sp, Heap.AddItem(new HeapArray(new char[count], ClassObjectManager.GetClassObjectAddr("char"))));
                                        break;
                                    case ArrayTypeCodes.T_FLOAT:
                                        Utility.Push(ref Stack, ref sp, Heap.AddItem(new HeapArray(new float[count], ClassObjectManager.GetClassObjectAddr("float"))));
                                        break;
                                    case ArrayTypeCodes.T_DOUBLE:
                                        Utility.Push(ref Stack, ref sp, Heap.AddItem(new HeapArray(new double[count], ClassObjectManager.GetClassObjectAddr("double"))));
                                        break;
                                    case ArrayTypeCodes.T_BYTE:
                                        Utility.Push(ref Stack, ref sp, Heap.AddItem(new HeapArray(new byte[count], ClassObjectManager.GetClassObjectAddr("byte"))));
                                        break;
                                    case ArrayTypeCodes.T_SHORT:
                                        Utility.Push(ref Stack, ref sp, Heap.AddItem(new HeapArray(new short[count], ClassObjectManager.GetClassObjectAddr("short"))));
                                        break;
                                    case ArrayTypeCodes.T_INT:
                                        Utility.Push(ref Stack, ref sp, Heap.AddItem(new HeapArray(new int[count], ClassObjectManager.GetClassObjectAddr("int"))));
                                        break;
                                    case ArrayTypeCodes.T_LONG:
                                        Utility.Push(ref Stack, ref sp, Heap.AddItem(new HeapArray(new long[count], ClassObjectManager.GetClassObjectAddr("long"))));
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                            break;
                        case OpCodes.anewarray:
                            {
                                short index = readShort(code, ref ip);
                                CClassInfo type = (CClassInfo)ClassFile.Constants[index];

                                int count = Utility.PopInt(Stack, ref sp);
                                if (count < 0)
                                {
                                    JavaHelper.ThrowJavaException("java/lang/NegativeArraySizeException");
                                }

                                Utility.Push(ref Stack, ref sp, Heap.AddItem(new HeapArray(new int[count], ClassObjectManager.GetClassObjectAddr(type.Name))));
                            }
                            break;
                        case OpCodes.arraylength:
                            {
                                int arrayRef = Utility.PopInt(Stack, ref sp);
                                HeapArray array = (HeapArray)Heap.GetItem(arrayRef);
                                Utility.Push(ref Stack, ref sp, array.GetLengthData());
                            }
                            break;
                        case OpCodes.athrow:
                            {
                                int objRef = Utility.PeekInt(Stack, sp);
                                Stack = new int[MaxStack];
                                Stack[0] = objRef;
                                sp = 1;
                                HeapObject obj = Heap.GetObject(objRef);
                                FieldReferenceValue message = (FieldReferenceValue)obj.GetField("detailMessage", "Ljava/lang/String;");
                                if (message.Address == 0)
                                {
                                    throw new JavaException(obj.ClassFile); //Handled in the same frame, outside of this switch
                                }
                                else
                                {
                                    throw new JavaException(obj.ClassFile, $"{JavaHelper.ReadJavaString(message)}");
                                }
                            }
                        case OpCodes.checkcast:
                        case OpCodes.instanceof:
                            {
                                //https://docs.oracle.com/javase/specs/jvms/se7/html/jvms-6.html#jvms-6.5.checkcast
                                short index = readShort(code, ref ip);
                                int objectRef;

                                if ((OpCodes)opCode == OpCodes.instanceof)
                                {
                                    objectRef = Utility.PopInt(Stack, ref sp);
                                }
                                else
                                {
                                    objectRef = Utility.PeekInt(Stack, sp);
                                }

                                if (objectRef == 0)
                                {
                                    if ((OpCodes)opCode == OpCodes.instanceof)
                                    {
                                        Utility.Push(ref Stack, ref sp, 0);
                                    }
                                }
                                else
                                {
                                    HeapObject objToCast = Heap.GetObject(objectRef);
                                    CClassInfo classToCastTo = (CClassInfo)ClassFile.Constants[index];
                                    int classObjAddr = ClassObjectManager.GetClassObjectAddr(classToCastTo);
                                    bool instanceOf = objToCast.IsInstance(classObjAddr); //for checkcast, this is canCast

                                    if ((OpCodes)opCode == OpCodes.instanceof)
                                    {
                                        int result = instanceOf ? 1 : 0;
                                        Utility.Push(ref Stack, ref sp, result);
                                    }
                                    else
                                    {
                                        if (!instanceOf)
                                        {
                                            JavaHelper.ThrowJavaException("java/lang/ClassCastException");
                                        }
                                    }
                                }
                            }
                            break;
                        case OpCodes.monitorenter:
                            {
                                int objectRef = Utility.PopInt(Stack, ref sp);
                                HeapItem item = Heap.GetItem(objectRef);
                            }
                            break;
                        case OpCodes.monitorexit:
                            {
                                int objectRef = Utility.PopInt(Stack, ref sp);
                                HeapItem item = Heap.GetItem(objectRef);
                            }
                            break;
                        case OpCodes.ifnull:
                            {
                                int offset = readShort(code, ref ip);
                                if (Heap.GetItem(Utility.PopInt(Stack, ref sp)) == null)
                                {
                                    ip = oldIp + offset;
                                }
                            }
                            break;
                        case OpCodes.ifnonnull:
                            {
                                int offset = readShort(code, ref ip);
                                if (Heap.GetItem(Utility.PopInt(Stack, ref sp)) != null)
                                {
                                    ip = oldIp + offset;
                                }
                            }
                            break;
                        case OpCodes.goto_w:
                            {
                                int offset = readInt(code, ref ip);
                                ip = oldIp + offset;
                            }
                            break;
                        case OpCodes.jsr_w:
                            {
                                int offset = readInt(code, ref ip);
                                int retAddress = ip;
                                Utility.Push(ref Stack, ref sp, retAddress);

                                ip = oldIp + offset;
                            }
                            break;
                        default:
                            throw new InvalidOperationException($"Missing Op Code: 0x{opCode:X2} = {Enum.GetName(typeof(OpCodes), opCode)}");
                    }

                }
                catch (JavaException ex)
                {
                    bool handled = false;
                    for (int i = 0; i < ExceptionTable.Length; i++)
                    {
                        ExceptionHandlerInfo handler = ExceptionTable[i];
                        if ((handler.CatchType == 0 || ex.ClassFile.IsSubClassOf(ClassFileManager.GetClassFile(handler.CatchClassType))) &&
                            ip >= handler.StartPc && ip < handler.EndPc)
                        {
                            ip = handler.HandlerPc;
                            handled = true;
                            break;
                        }
                    }
                    if (!handled)
                    {
                        DebugWriter.ExceptionThrownDebugWrite(ex);
                        if (Program.MethodFrameStack.Count > 1)
                        {
                            MethodFrame parentFrame = Program.MethodFrameStack.Peek(1);
                            parentFrame.Stack = new int[parentFrame.Stack.Length];
                            parentFrame.sp = 1;
                            parentFrame.Stack[0] = Utility.PopInt(Stack, ref sp);
                        }
                        Program.MethodFrameStack.Pop();
                        throw;
                    }
                }
            }
        }
        private short readShort(ReadOnlySpan<byte> code, ref int instructionPointer)
        {
            byte chomp = code[instructionPointer++]; //Larger byte
            byte nibble = code[instructionPointer++]; //Smaller byte
            return (short)((chomp << 8) | nibble);
        }
        private int readInt(ReadOnlySpan<byte> code, ref int instructionPointer)
        {
            byte byte0 = code[instructionPointer++];
            byte byte1 = code[instructionPointer++];
            byte byte2 = code[instructionPointer++];
            byte byte3 = code[instructionPointer++];
            return ((byte0 << 24) | (byte1 << 16) | (byte2 << 8) | byte3);
        }
    }
}
