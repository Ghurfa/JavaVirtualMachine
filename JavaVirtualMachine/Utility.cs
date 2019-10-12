using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace JavaVirtualMachine
{
    public enum ReturnType
    {
        @byte,
        @char,
        @double,
        @float,
        @int,
        @long,
        reference,
        @short,
        boolean,
        array,
        @void
    }
    static class Utility
    {
        public static ushort SwapEndian(this ref ushort data) => data = (ushort)((data << 8) | (data >> 8));
        public static uint SwapEndian(this ref uint data) => data = ((data << 24) | ((data << 8) & 0xFF0000) | ((data >> 8) & 0xFF00) | (data >> 24));
        public static byte ReadOne(this ref ReadOnlySpan<byte> span)
        {
            byte nibble = span[0];
            span = span.Slice(1);
            return nibble;
        }
        public static ushort ReadTwo(this ref ReadOnlySpan<byte> span)
        {
            ushort dwarf = MemoryMarshal.Cast<byte, ushort>(span)[0];
            span = span.Slice(2);
            return dwarf.SwapEndian();
        }
        public static ushort ReadTwo(this ref ReadOnlyMemory<byte> memory)
        {
            ushort dwarf = MemoryMarshal.Cast<byte, ushort>(memory.Span)[0];
            memory = memory.Slice(2);
            return dwarf.SwapEndian();
        }
        public static uint ReadFour(this ref ReadOnlySpan<byte> span)
        {
            uint num = MemoryMarshal.Cast<byte, uint>(span)[0];
            span = span.Slice(4);
            return num.SwapEndian();
        }
        public static uint ReadFour(this ref ReadOnlyMemory<byte> memory)
        {
            uint num = MemoryMarshal.Cast<byte, uint>(memory.Span)[0];
            memory = memory.Slice(4);
            return num.SwapEndian();
        }

        public static int NumOfArgs(this MethodInfo method) => numOfArgs(method.Descriptor);
        public static int NumOfArgs(this CMethodRefInfo methodRef) => numOfArgs(methodRef.Descriptor);
        public static int NumOfArgs(this CInterfaceMethodRefInfo interfaceMethodRef) => numOfArgs(interfaceMethodRef.Descriptor);
        private static int numOfArgs(string descriptor)
        {
            int numOfArgs = 0;
            int i;
            for (i = 1; descriptor[i] != ')'; )
            {
                if (descriptor[i] == 'J' || descriptor[i] == 'D')
                {
                    numOfArgs += 2;
                    i++;
                }
                else
                {
                    numOfArgs++;

                    //Move to next arg
                    if (descriptor[i] == '[')
                    {
                        i++;
                    }
                    if (descriptor[i] == 'L')
                    {
                        for (i++; descriptor[i] != ';'; i++) { }
                    }
                    i++;
                }
            }
            return numOfArgs;
            /*
            i++;
            switch (descriptor[i])
            {
                case 'B':
                    return (numOfArgs, ReturnType.@byte);
                case 'C':
                    return (numOfArgs, ReturnType.@char);
                case 'D':
                    return (numOfArgs, ReturnType.@double);
                case 'F':
                    return (numOfArgs, ReturnType.@float);
                case 'I':
                    return (numOfArgs, ReturnType.@int);
                case 'J':
                    return (numOfArgs, ReturnType.@long);
                case 'L':
                    return (numOfArgs, ReturnType.@reference);
                case 'S':
                    return (numOfArgs, ReturnType.@short);
                case 'Z':
                    return (numOfArgs, ReturnType.boolean);
                case '[':
                    return (numOfArgs, ReturnType.array);
                case 'V':
                    return (numOfArgs, ReturnType.@void);
                default:
                    throw new InvalidOperationException();
            }*/
        }

        public static void Push(ref uint[] stack, ref int stackPointer, uint value)
        {
            stack[stackPointer++] = value;
        }
        public static void Push(ref uint[] stack, ref int stackPointer, ulong value)
        {
            stack[stackPointer++] = (uint)(value >> (8 * sizeof(int)));
            stack[stackPointer++] = (uint)(value & 0xFFFFFFFF);
        }
        public static void Push(ref uint[] stack, ref int stackPointer, float value)
        {
            stack[stackPointer++] = FloatToStoredFloat(value);
        }
        public static void Push(ref uint[] stack, ref int stackPointer, double value)
        {
            Push(ref stack, ref stackPointer, DoubleToStoredDouble(value));
        }

        public static uint PopInt(uint[] stack, ref int stackPointer)
        {
            return stack[--stackPointer];
        }
        public static uint PeekInt(uint[] stack, int stackPointer, int offset = 0)
        {
            return stack[stackPointer - 1 - offset];
        }
        public static ulong PeekLong(uint[] stack, int stackPointer, int offset = 0)
        {
            uint lowInt = PeekInt(stack, stackPointer, offset);
            uint highInt = PeekInt(stack, stackPointer, offset + 1);
            return (highInt, lowInt).ToULong();
        }
        public static ulong PopLong(uint[] stack, ref int stackPointer)
        {
            uint lowInt = PopInt(stack, ref stackPointer);
            uint highInt = PopInt(stack, ref stackPointer);
            return (highInt, lowInt).ToULong();
        }
        public static float PopFloat(uint[] stack, ref int stackPointer)
        {
            uint storedValue = stack[--stackPointer];
            return StoredFloatToFloat(storedValue);
        }
        public static double PopDouble(uint[] stack, ref int stackPointer)
        {
            ulong storedValue = PopLong(stack, ref stackPointer);
            return StoredDoubleToDouble(storedValue);
        }

        public static MethodFrame Peek(this Stack<MethodFrame> stack, int depth)
        {
            MethodFrame[] asArray = stack.ToArray();
            return asArray[depth];
        }

        public static ulong ToULong(this (uint high, uint low) pair)
        {
            return (((ulong)pair.high) << 32) | (ulong)pair.low;
        }
        public static long ToLong(this (int high, int low) pair)
        {
            return (((long)pair.high) << 32) | (long)pair.low;
        }
        public static (uint high, uint low) Split(this ulong value)
        {
            return ((uint)(value >> 32), (uint)(value & 0xFFFFFFFF));
        }

        public static byte[] AsByteArray(this long value)
        {
            byte[] array = new byte[sizeof(long) / sizeof(byte)];
            for(int i = 0; i < array.Length; i++)
            {
                int shiftAmount = 8 * (array.Length - i - 1);
                long shifted = value >> shiftAmount;

                array[i] = (byte)(shifted & 0xFF);
            }
            return array;
            //return new byte[] { (byte)(value >> 56), (byte)(value >> 48), (byte)(value >> 40), (byte)(value >> 32), (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
        }
        public static byte[] AsByteArray(this int value)
        {
            byte[] array = new byte[sizeof(int) / sizeof(byte)];
            for (int i = 0; i < array.Length; i++)
            {
                int shiftAmount = 8 * (array.Length - i - 1);
                long shifted = value >> shiftAmount;

                array[i] = (byte)(shifted & 0xFF);
            }
            return array;
            // return new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value) };
        }

        public static int ToInt(this byte[] array)
        {
            if (array.Length != 4) throw new InvalidOperationException();
            int value = 0;
            for (int i = 0; i < sizeof(int) / sizeof(byte); i++)
            {
                value <<= 8;
                value |= array[i];
            }
            return value;
        }
        public static long ToLong(this byte[] array)
        {
            if (array.Length != 8) throw new InvalidOperationException();
            long value = 0;
            for (int i = 0; i < sizeof(long) / sizeof(byte); i++)
            {
                value <<= 8;
                value |= array[i];
            }
            return value;
        }

        public static int CreateJavaStringLiteral(string @string)
        {
            
            HeapArray charArray = new HeapArray(@string.ToCharArray(), ClassObjectManager.GetClassObjectAddr("char"));
            int charArrAddr = Heap.AddItem(charArray);
            HeapObject stringObj = new HeapObject(ClassFileManager.GetClassFile("java/lang/String"));
            stringObj.SetField("value", "[C", new FieldReferenceValue(charArrAddr));
            int stringObjAddr = Heap.AddItem(stringObj);
            return StringPool.Intern(stringObjAddr);
            
        }

        public static string ReadJavaString(int address)
        {
            HeapObject stringObj = Heap.GetObject(address);
            FieldReferenceValue charArrayReference = (FieldReferenceValue)stringObj.GetField("value", "[C");
            HeapArray charArray = (HeapArray)Heap.GetItem(charArrayReference.Address);
            return new string((char[])charArray.Array);
        }

        public static string ReadJavaString(FieldReferenceValue reference)
        {
            HeapObject stringObj = Heap.GetObject(reference.Address);
            FieldReferenceValue charArrayReference = (FieldReferenceValue)stringObj.GetField("value", "[C");
            HeapArray charArray = (HeapArray)Heap.GetItem(charArrayReference.Address);
            return new string((char[])charArray.Array);
        }

        public static string ReadJavaString(HeapObject stringObj)
        {
            FieldReferenceValue charArrayReference = (FieldReferenceValue)stringObj.GetField("value", "[C");
            HeapArray charArray = (HeapArray)Heap.GetItem(charArrayReference.Address);
            return new string((char[])charArray.Array);
        }

        public static void ReturnValue(int retVal)
        {
            DebugWriter.ReturnedValueDebugWrite(retVal);
            Program.MethodFrameStack.Pop();
            if (Program.MethodFrameStack.Count >= 0)
            {
                MethodFrame parentFrame = Program.MethodFrameStack.Peek();
                Push(ref parentFrame.Stack, ref parentFrame.sp, (uint)retVal);
            }
        }
        public static void ReturnLargeValue(long retVal)
        {
            DebugWriter.ReturnedValueDebugWrite(retVal);
            Program.MethodFrameStack.Pop();
            if (Program.MethodFrameStack.Count >= 0)
            {
                MethodFrame parentFrame = Program.MethodFrameStack.Peek();
                Push(ref parentFrame.Stack, ref parentFrame.sp, (ulong)retVal);
            }
        }
        public static void ReturnVoid()
        {
            DebugWriter.ReturnedVoidDebugWrite();
            Program.MethodFrameStack.Pop();
        }

        public static void RunJavaFunction(MethodInfo methodInfo, params int[] arguments)
        {
            DebugWriter.CallFuncDebugWrite(methodInfo, arguments);
            if (methodInfo.HasFlag(MethodInfoFlag.Native))
            {
                NativeMethodFrame methodFrame = new NativeMethodFrame(methodInfo);
                arguments.CopyTo(methodFrame.Locals, 0);
                methodFrame.Execute();
            }
            else
            {
                MethodFrame methodFrame = new MethodFrame(methodInfo);
                arguments.CopyTo(methodFrame.Locals, 0);
                methodFrame.Execute();
            }
        }

        public static void ThrowJavaException(string type)
        {
            ClassFile exceptionCFile = ClassFileManager.GetClassFile(type);
            int objRef = Heap.AddItem(new HeapObject(exceptionCFile));

            MethodInfo initMethod = exceptionCFile.MethodDictionary[("<init>", "()V")];
            RunJavaFunction(initMethod, objRef);

            MethodFrame frame = Program.MethodFrameStack.Peek();
            frame.Stack = new uint[frame.Stack.Length];
            frame.Stack[0] = (uint)objRef;
            frame.sp = 1;
            throw new JavaException(exceptionCFile);
        }

        public static bool IsArray(this CClassInfo classInfo)
        {
            return classInfo.Name[0] == '[';
        }

        public static bool IsArray(this string className)
        {
            return className[0] == '[';
        }

        public static float StoredFloatToFloat(uint storedFloat)
        {
            float asFloat;
            unsafe
            {
                uint* storedFloatPtr = &storedFloat;
                float* asFloatPtr = (float*)storedFloatPtr;
                asFloat = *asFloatPtr;
            }
            return asFloat;
        }

        public static uint FloatToStoredFloat(float floatValue)
        {
            uint storedFloat;
            unsafe
            {
                float* floatValPtr = &floatValue;
                uint* storedFloatPtr = (uint*)floatValPtr;
                storedFloat = *storedFloatPtr;
            }
            return storedFloat;
        }

        public static double StoredDoubleToDouble(ulong storedDouble)
        {
            double asDouble;
            unsafe
            {
                ulong* storedDoublePtr = &storedDouble;
                double* asDoublePtr = (double*)storedDoublePtr;
                asDouble = *asDoublePtr;
            }
            return asDouble;
        }

        public static ulong DoubleToStoredDouble(double doubleValue)
        {
            ulong storedDouble;
            unsafe
            {
                double* doubleValPtr = &doubleValue;
                ulong* storedDoublePtr = (ulong*)doubleValPtr;
                storedDouble = *storedDoublePtr;
            }
            return storedDouble;
        }

        public static bool ImplementsInterface(this ClassFile classFileToCheck, ClassFile interfaceToCheck)
        {
            if (classFileToCheck == interfaceToCheck) return true;

            Stack<ClassFile> supers = new Stack<ClassFile>(); //Super-classes and interfaces and super-interfaces
            supers.Push(classFileToCheck);

            while (supers.Count > 0)
            {
                ClassFile classFile = supers.Pop();

                //Don't need to check classFile == interfaceToCheck because everything is checked before being pushed to the stack

                if (classFile.SuperClass != null)
                {
                    if (classFile.SuperClass == interfaceToCheck) return true;
                    supers.Push(classFile.SuperClass);
                }

                //Check the names of the interfaces first so that no interfaces are loaded if one of them is the interface to check
                foreach (string interfaceName in classFile.InterfaceNames)
                {
                    if (interfaceName == interfaceToCheck.Name) return true;
                }
                foreach (string interfaceName in classFile.InterfaceNames)
                {
                    supers.Push(ClassFileManager.GetClassFile(interfaceName));
                }
            }
            return false;
        }

        /// <summary>
        /// Returns if classFile is the same as or is a subclass of potentialSuperClass
        /// </summary>
        public static bool IsSubClassOf(this ClassFile classFile, ClassFile potentialSuperClass)
        {
            ClassFile temp = classFile;
            while (temp != null)
            {
                if (temp == potentialSuperClass) return true;
                temp = temp.SuperClass;
            }
            return false;
        }
        public static string PrimitiveFullName(string name)
        {
            switch (name)
            {
                case "Z":
                    return "boolean";
                case "B":
                    return "byte";
                case "C":
                    return "char";
                case "D":
                    return "double";
                case "F":
                    return "float";
                case "I":
                    return "int";
                case "J":
                    return "long";
                case "S":
                    return "short";
                default:
                    throw new ArgumentException();
            }
        }
    }
}
