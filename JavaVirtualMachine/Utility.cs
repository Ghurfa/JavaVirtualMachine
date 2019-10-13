using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace JavaVirtualMachine
{
    public static class Utility
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

        public static void Push(ref int[] stack, ref int stackPointer, int value)
        {
            stack[stackPointer++] = value;
        }
        public static void Push(ref int[] stack, ref int stackPointer, int[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                stack[stackPointer++] = values[i];
            }
        }
        public static void Push(ref int[] stack, ref int stackPointer, long value)
        {
            stack[stackPointer++] = (int)(value >> (8 * sizeof(int)));
            stack[stackPointer++] = (int)(value & 0xFFFFFFFF);
        }
        public static void Push(ref int[] stack, ref int stackPointer, float value)
        {
            stack[stackPointer++] = FloatToStoredFloat(value);
        }
        public static void Push(ref int[] stack, ref int stackPointer, double value)
        {
            Push(ref stack, ref stackPointer, DoubleToStoredDouble(value));
        }

        public static int PopInt(int[] stack, ref int stackPointer)
        {
            return stack[--stackPointer];
        }
        public static int PeekInt(int[] stack, int stackPointer, int offset = 0)
        {
            return stack[stackPointer - 1 - offset];
        }
        public static long PeekLong(int[] stack, int stackPointer, int offset = 0)
        {
            int lowInt = PeekInt(stack, stackPointer, offset);
            int highInt = PeekInt(stack, stackPointer, offset + 1);
            return (((long)highInt) << 32) | (long)lowInt;
        }
        public static long PopLong(int[] stack, ref int stackPointer)
        {
            int lowInt = PopInt(stack, ref stackPointer);
            int highInt = PopInt(stack, ref stackPointer);
            return (((long)highInt) << 32) | (long)lowInt;
        }
        public static float PopFloat(int[] stack, ref int stackPointer)
        {
            int storedValue = stack[--stackPointer];
            return StoredFloatToFloat(storedValue);
        }
        public static double PopDouble(int[] stack, ref int stackPointer)
        {
            long storedValue = PopLong(stack, ref stackPointer);
            return StoredDoubleToDouble(storedValue);
        }

        public static MethodFrame Peek(this Stack<MethodFrame> stack, int depth)
        {
            MethodFrame[] asArray = stack.ToArray();
            return asArray[depth];
        }

        public static long ToLong(this (int high, int low) pair)
        {
            return (((long)pair.high) << 32) | (long)pair.low;
        }
        public static (int high, int low) Split(this long value)
        {
            return ((int)(value >> 32), (int)(value & 0xFFFFFFFF));
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
                Push(ref parentFrame.Stack, ref parentFrame.sp, retVal);
            }
        }
        public static void ReturnLargeValue(long retVal)
        {
            DebugWriter.ReturnedValueDebugWrite(retVal);
            Program.MethodFrameStack.Pop();
            if (Program.MethodFrameStack.Count >= 0)
            {
                MethodFrame parentFrame = Program.MethodFrameStack.Peek();
                Push(ref parentFrame.Stack, ref parentFrame.sp, retVal);
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
            frame.Stack = new int[frame.Stack.Length];
            frame.Stack[0] = objRef;
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

        public static float StoredFloatToFloat(int storedFloat)
        {
            float asFloat;
            unsafe
            {
                int* storedFloatPtr = &storedFloat;
                float* asFloatPtr = (float*)storedFloatPtr;
                asFloat = *asFloatPtr;
            }
            return asFloat;
        }

        public static int FloatToStoredFloat(float floatValue)
        {
            int storedFloat;
            unsafe
            {
                float* floatValPtr = &floatValue;
                int* storedFloatPtr = (int*)floatValPtr;
                storedFloat = *storedFloatPtr;
            }
            return storedFloat;
        }

        public static double StoredDoubleToDouble(long storedDouble)
        {
            double asDouble;
            unsafe
            {
                long* storedDoublePtr = &storedDouble;
                double* asDoublePtr = (double*)storedDoublePtr;
                asDouble = *asDoublePtr;
            }
            return asDouble;
        }

        public static long DoubleToStoredDouble(double doubleValue)
        {
            long storedDouble;
            unsafe
            {
                double* doubleValPtr = &doubleValue;
                long* storedDoublePtr = (long*)doubleValPtr;
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
