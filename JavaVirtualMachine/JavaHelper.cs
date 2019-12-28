using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    public static class JavaHelper
    {
        public static int NumOfArgs(this MethodInfo method) => NumOfArgs(method.Descriptor);
        public static int NumOfArgs(this CMethodRefInfo methodRef) => NumOfArgs(methodRef.Descriptor);
        public static int NumOfArgs(this CInterfaceMethodRefInfo interfaceMethodRef) => NumOfArgs(interfaceMethodRef.Descriptor);
        public static int NumOfArgs(string descriptor)
        {
            int numOfArgs = 0;
            int i;
            for (i = 1; descriptor[i] != ')';)
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
                Utility.Push(ref parentFrame.Stack, ref parentFrame.sp, retVal);
            }
        }
        public static void ReturnLargeValue(long retVal)
        {
            DebugWriter.ReturnedValueDebugWrite(retVal);
            Program.MethodFrameStack.Pop();
            if (Program.MethodFrameStack.Count >= 0)
            {
                MethodFrame parentFrame = Program.MethodFrameStack.Peek();
                Utility.Push(ref parentFrame.Stack, ref parentFrame.sp, retVal);
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
                    throw new ArgumentException("Unrecognized primitive name", nameof(name));
            }
        }

        public static bool IsPrimitiveType(string type)
        {
            return  type == "boolean" ||
                    type == "byte" ||
                    type == "char" ||
                    type == "double" ||
                    type == "float" ||
                    type == "int" ||
                    type == "long" ||
                    type == "short" ||
                    type == "void";
        }

        public static string ClassObjectName(int classObjAddr)
        {
            return ClassObjectName(Heap.GetObject(classObjAddr));
        }
        public static string ClassObjectName(HeapObject classObj)
        {
            FieldReferenceValue nameField = (FieldReferenceValue)classObj.GetField("name", "Ljava/lang/String;");
            return ReadJavaString(nameField.Address);
        }
    }
}
