using JavaVirtualMachine.ConstantPoolItems;
using System.Runtime.InteropServices;
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
        }
        public static int CreateJavaStringLiteral(string str)
        {
            if (StringPool.StringAddresses.TryGetValue(str, out int strAddr))
            {
                return strAddr;
            }

            byte[] data = Encoding.Unicode.GetBytes(str);
            int charArrAddr = Heap.CreateArray(2, data.Length / 2, ClassObjectManager.GetClassObjectAddr("char"));
            data.CopyTo(Heap.GetArray(charArrAddr).GetDataSpan());

            int newStrObjAddr = Heap.CreateObject(ClassFileManager.GetClassFileIndex("java/lang/String"));
            Heap.GetObject(newStrObjAddr).SetField("value", "[C", charArrAddr);

            StringPool.StringAddresses.Add(str, newStrObjAddr);
            return newStrObjAddr;
        }

        public static string ReadJavaString(int address)
        {
            HeapObject stringObj = Heap.GetObject(address);
            int charArrAddr = stringObj.GetField("value", "[C");
            HeapArray charArray = Heap.GetArray(charArrAddr);

            return Encoding.Unicode.GetString(charArray.GetDataSpan());
        }

        public static void ReturnValue(int retVal)
        {
            MethodInfo currMethod = Program.MethodFrameStack.Peek().MethodInfo;
            Program.StackTracePrinter.PrintMethodReturn(currMethod, retVal);
            Program.MethodFrameStack.Pop();
            if (Program.MethodFrameStack.Count >= 0)
            {
                MethodFrame parentFrame = Program.MethodFrameStack.Peek();
                Utility.Push(ref parentFrame.Stack, ref parentFrame.sp, retVal);
            }
        }

        public static void ReturnLargeValue(long retVal)
        {
            MethodInfo currMethod = Program.MethodFrameStack.Peek().MethodInfo;
            Program.StackTracePrinter.PrintMethodReturn(currMethod, retVal);
            Program.MethodFrameStack.Pop();
            if (Program.MethodFrameStack.Count >= 0)
            {
                MethodFrame parentFrame = Program.MethodFrameStack.Peek();
                Utility.Push(ref parentFrame.Stack, ref parentFrame.sp, retVal);
            }
        }

        public static void ReturnVoid()
        {
            MethodInfo currMethod = Program.MethodFrameStack.Peek().MethodInfo;
            Program.StackTracePrinter.PrintMethodReturn(currMethod);
            Program.MethodFrameStack.Pop();
        }

        public static void RunJavaFunction(MethodInfo methodInfo, params int[] arguments)
        {
            Program.StackTracePrinter.PrintMethodCall(methodInfo, arguments);
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
            int exceptionCFileIdx = ClassFileManager.GetClassFileIndex(type);
            ClassFile exceptionCFile = ClassFileManager.ClassFiles[exceptionCFileIdx];
            int objRef = Heap.CreateObject(exceptionCFileIdx);

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
                case "V":
                    return "void";
                default:
                    throw new ArgumentException("Unrecognized primitive name", nameof(name));
            }
        }

        public static bool IsPrimitiveType(string type)
        {
            return type == "boolean" ||
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
            return ReadJavaString(classObj.GetField("name", "Ljava/lang/String;"));
        }
        public static string ReadDescriptorArg(string descriptor, ref int i)
        {
            int startI = i;
            while (descriptor[i] == '[')
            {
                i++;
            }
            if (descriptor[i] == 'L')
            {
                for (i++; descriptor[i] != ';'; i++) ;
            }
            i++;
            return descriptor.Substring(startI, i - startI);
        }
        public static MethodInfo ResolveMethod(string className, string methodName, string methodDescriptor)
        {
            ClassFile cFile = ClassFileManager.GetClassFile(className);

            MethodInfo method = null;
            bool foundMethod = false;
            //Search for method in cFile's method dictionary. If it's not there, repeat search in cFile's super and so on
            while (!foundMethod)
            {
                foundMethod = cFile.MethodDictionary.TryGetValue((methodName, methodDescriptor), out method);
                if (!foundMethod && className == "java/lang/invoke/MethodHandle")
                {
                    //Signature polymorphic method
                    if (methodName == "invoke")
                    {
                        method = cFile.MethodDictionary[("invoke", "([Ljava/lang/Object;)Ljava/lang/Object;")];
                        foundMethod = true;
                    }
                    else if (methodName == "invokeExact")
                    {
                        method = cFile.MethodDictionary[("invokeExact", "([Ljava/lang/Object;)Ljava/lang/Object;")];
                        foundMethod = true;
                    }
                }
                cFile = cFile.SuperClass;
            }
            return method;
        }

        public static string MakeDescriptor(HeapObject returnType, HeapArray parameterTypes)
        {
            int[] paramTypesArr = MemoryMarshal.Cast<byte, int>(parameterTypes.GetDataSpan()).ToArray();

            string descriptor = "(";
            for (int i = 0; i < paramTypesArr.Length; i++)
            {
                throw new NotImplementedException();
                string typeName = ClassObjectName(paramTypesArr[i]);
            }

            string returnTypeName = ClassObjectName(returnType);

            if (IsPrimitiveType(returnTypeName))
            {
                throw new NotImplementedException();
            }

            int j;
            for (j = 0; returnTypeName[j] == '['; j++) ; //Skip through array brackets
            if (j != returnTypeName.Length) //Object
            {
                //Format as descriptor style
                returnTypeName = returnTypeName.Substring(0, j) + ")L" + returnTypeName.Substring(j, returnTypeName.Length - j).Replace(".", "/") + ";";
            }
            else
            {
                descriptor += ")";
            }
            descriptor += returnTypeName;

            return descriptor;
        }

        public static void CreateMethodTypeObj(string descriptor)
        {
            ClassFile methodTypeCFile = ClassFileManager.ClassFiles[ClassFileManager.GetClassFileIndex("java/lang/invoke/MethodType")];
            MethodInfo methodTypeMethod = methodTypeCFile.MethodDictionary[("methodType", "(Ljava/lang/Class;[Ljava/lang/Class;)Ljava/lang/invoke/MethodType;")];

            //Read method type
            List<int> paramsClassObjs = new List<int>();
            int i;
            for (i = 1; descriptor[i] != ')';)
            {
                string argumentType = ReadDescriptorArg(descriptor, ref i);
                int classObj = ClassObjectManager.GetClassObjectAddr(argumentType);
                paramsClassObjs.Add(classObj);
            }

            int paramsArrAddr = Heap.CreateArray(paramsClassObjs.ToArray(), ClassObjectManager.GetClassObjectAddr("java/lang/Class"));

            i++;
            string retType = ReadDescriptorArg(descriptor, ref i);
            int retClassObj = ClassObjectManager.GetClassObjectAddr(retType);

            RunJavaFunction(methodTypeMethod, retClassObj, paramsArrAddr);
        }

        public static int ResolveMethodHandle(CMethodHandleInfo methodHandle)
        {
            string className = ((CMethodRefInfo)methodHandle.Reference).ClassName;
            ClassFile cFile = ClassFileManager.ClassFiles[ClassFileManager.GetClassFileIndex(className)];

            var methodRef = (CMethodRefInfo)methodHandle.Reference;
            MethodInfo methodInfo = cFile.MethodDictionary[(methodRef.Name, methodRef.Descriptor)];

            CreateMethodTypeObj(((CMethodRefInfo)methodHandle.Reference).Descriptor);
            MethodFrame frame = Program.MethodFrameStack.Peek();
            int bootstrapMethodType = Utility.PopInt(frame.Stack, ref frame.sp);

            int methodHandleObjAddr = Heap.CreateObject(ClassFileManager.GetClassFileIndex("java/lang/invoke/MethodHandle"));
            HeapObject methodHandleObj = Heap.GetObject(methodHandleObjAddr);

            methodHandleObj.SetField("type", "Ljava/lang/invoke/MethodType;", bootstrapMethodType);

            if (methodInfo.HasFlag(MethodInfoFlag.VarArgs))
            {
                throw new NotImplementedException();
            }
            return methodHandleObjAddr;
        }

        public static (int methodHandle, int methodType, int[] staticArgs) ResolveCallSiteSpecifier(CInvokeDynamicInfo callSiteSpecifier)
        {
            CMethodHandleInfo bootstrapMethodHandle = callSiteSpecifier.BootstrapMethod.MethodHandle;
            int bootstrapMethodHandleObj = ResolveMethodHandle(bootstrapMethodHandle);

            CreateMethodTypeObj(((CMethodRefInfo)bootstrapMethodHandle.Reference).Descriptor);
            MethodFrame frame = Program.MethodFrameStack.Peek();
            int bootstrapMethodType = Utility.PopInt(frame.Stack, ref frame.sp);

            CPInfo[] staticArgsConstants = callSiteSpecifier.BootstrapMethod.Arguments;

            int numOfLargeArgs = 0;
            foreach (CPInfo constant in staticArgsConstants)
            {
                if (constant is CLongInfo || constant is CDoubleInfo)
                {
                    numOfLargeArgs++;
                }
            }

            int[] staticArgs = new int[staticArgsConstants.Length + numOfLargeArgs];
            for (int i = 0; i < staticArgsConstants.Length; i++)
            {
                CPInfo constant = callSiteSpecifier.BootstrapMethod.Arguments[i];
                switch (constant)
                {
                    case CClassInfo classInfo:
                        staticArgs[i] = ClassObjectManager.GetClassObjectAddr(classInfo);
                        throw new NotImplementedException();
                    case CMethodHandleInfo methodHandle:
                        staticArgs[i] = ResolveMethodHandle(methodHandle);
                        break;
                    case CMethodTypeInfo methodType:
                        CreateMethodTypeObj(methodType.Descriptor.String);
                        staticArgs[i] = Utility.PopInt(frame.Stack, ref frame.sp);
                        break;
                    case CStringInfo stringVal:
                        staticArgs[i] = CreateJavaStringLiteral(stringVal.String);
                        break;
                    case CIntegerInfo intVal:
                        staticArgs[i] = intVal.IntValue;
                        break;
                    case CLongInfo longVal:
                        {
                            (int low, int high) = longVal.LongValue.Split();
                            staticArgs[i] = low;
                            staticArgs[++i] = high;
                        }
                        break;
                    case CFloatInfo floatVal:
                        staticArgs[i] = (int)floatVal.IntValue;
                        break;
                    case CDoubleInfo doubleVal:
                        {
                            (int low, int high) = doubleVal.LongValue.Split();
                            staticArgs[i] = low;
                            staticArgs[++i] = high;
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            return (bootstrapMethodHandleObj, bootstrapMethodType, staticArgs);
        }
    }
}
