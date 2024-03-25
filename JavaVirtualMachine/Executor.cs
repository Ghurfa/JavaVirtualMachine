using JavaVirtualMachine.Attributes;
using JavaVirtualMachine.ConstantPoolItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JavaVirtualMachine
{
    internal static class Executor
    {
        private enum OpCodes
        {
            nop          = 0x0,  aconst_null   = 0x1,  iconst_m1   = 0x2,  iconst_0     = 0x3,  iconst_1   = 0x4,  iconst_2   = 0x5,  iconst_3    = 0x6,  iconst_4   = 0x7,
            iconst_5     = 0x8,  lconst_0      = 0x9,  lconst_1    = 0x0a, fconst_0     = 0x0b, fconst_1   = 0x0c, fconst_2   = 0x0d, dconst_0    = 0x0e, dconst_1   = 0x0f,
            bipush       = 0x10, sipush        = 0x11, ldc         = 0x12, ldc_w        = 0x13, ldc2_w     = 0x14, iload      = 0x15, lload       = 0x16, fload      = 0x17,
            dload        = 0x18, aload         = 0x19, iload_0     = 0x1a, iload_1      = 0x1b, iload_2    = 0x1c, iload_3    = 0x1d, lload_0     = 0x1e, lload_1    = 0x1f,
            lload_2      = 0x20, lload_3       = 0x21, fload_0     = 0x22, fload_1      = 0x23, fload_2    = 0x24, fload_3    = 0x25, dload_0     = 0x26, dload_1    = 0x27,
            dload_2      = 0x28, dload_3       = 0x29, aload_0     = 0x2a, aload_1      = 0x2b, aload_2    = 0x2c, aload_3    = 0x2d, iaload      = 0x2e, laload     = 0x2f,
            faload       = 0x30, daload        = 0x31, aaload      = 0x32, baload       = 0x33, caload     = 0x34, saload     = 0x35, istore      = 0x36, lstore     = 0x37,
            fstore       = 0x38, dstore        = 0x39, astore      = 0x3a, istore_0     = 0x3b, istore_1   = 0x3c, istore_2   = 0x3d, istore_3    = 0x3e, lstore_0   = 0x3f,
            lstore_1     = 0x40, lstore_2      = 0x41, lstore_3    = 0x42, fstore_0     = 0x43, fstore_1   = 0x44, fstore_2   = 0x45, fstore_3    = 0x46, dstore_0   = 0x47,
            dstore_1     = 0x48, dstore_2      = 0x49, dstore_3    = 0x4a, astore_0     = 0x4b, astore_1   = 0x4c, astore_2   = 0x4d, astore_3    = 0x4e, iastore    = 0x4f,
            lastore      = 0x50, fastore       = 0x51, dastore     = 0x52, aastore      = 0x53, bastore    = 0x54, castore    = 0x55, sastore     = 0x56, pop        = 0x57,
            pop2         = 0x58, dup           = 0x59, dup_x1      = 0x5a, dup_x2       = 0x5b, dup2       = 0x5c, dup2_x1    = 0x5d, dup2_x2     = 0x5e, swap       = 0x5f,
            iadd         = 0x60, ladd          = 0x61, fadd        = 0x62, dadd         = 0x63, isub       = 0x64, lsub       = 0x65, fsub        = 0x66, dsub       = 0x67,
            imul         = 0x68, lmul          = 0x69, fmul        = 0x6a, dmul         = 0x6b, idiv       = 0x6c, ldiv       = 0x6d, fdiv        = 0x6e, ddiv       = 0x6f,
            irem         = 0x70, lrem          = 0x71, frem        = 0x72, drem         = 0x73, ineg       = 0x74, lneg       = 0x75, fneg        = 0x76, dneg       = 0x77,
            ishl         = 0x78, lshl          = 0x79, ishr        = 0x7a, lshr         = 0x7b, iushr      = 0x7c, lushr      = 0x7d, iand        = 0x7e, land       = 0x7f,
            ior          = 0x80, lor           = 0x81, ixor        = 0x82, lxor         = 0x83, iinc       = 0x84, i2l        = 0x85, i2f         = 0x86, i2d        = 0x87,
            l2i          = 0x88, l2f           = 0x89, l2d         = 0x8a, f2i          = 0x8b, f2l        = 0x8c, f2d        = 0x8d, d2i         = 0x8e, d2l        = 0x8f,
            d2f          = 0x90, i2b           = 0x91, i2c         = 0x92, i2s          = 0x93, lcmp       = 0x94, fcmpl      = 0x95, fcmpg       = 0x96, dcmpl      = 0x97,
            dcmpg        = 0x98, ifeq          = 0x99, ifne        = 0x9a, iflt         = 0x9b, ifge       = 0x9c, ifgt       = 0x9d, ifle        = 0x9e, if_icmpeq  = 0x9f,
            if_icmpne    = 0xa0, if_icmplt     = 0xa1, if_icmpge   = 0xa2, if_icmpgt    = 0xa3, if_icmple  = 0xa4, if_acmpeq  = 0xa5, if_acmpne   = 0xa6, @goto      = 0xa7,
            jsr          = 0xa8, ret           = 0xa9, tableswitch = 0xaa, lookupswitch = 0xab, ireturn    = 0xac, lreturn    = 0xad, freturn     = 0xae, dreturn    = 0xaf,
            areturn      = 0xb0, @return       = 0xb1, getstatic   = 0xb2, putstatic    = 0xb3, getfield   = 0xb4, putfield   = 0xb5, invokevirtual = 0xb6, invokespecial = 0xb7,
            invokestatic = 0xb8, invokeinterface = 0xb9, invokedynamic = 0xba, @new     = 0xbb, newarray   = 0xbc, anewarray  = 0xbd, arraylength = 0xbe, athrow     = 0xbf,
            checkcast    = 0xc0, instanceof    = 0xc1, monitorenter = 0xc2, monitorexit  = 0xc3, wide      = 0xc4, multianewarray = 0xc5, ifnull  = 0xc6, ifnonnull  = 0xc7,
            goto_w       = 0xc8, jsr_w         = 0xc9, breakpoint  = 0xca, impdep1      = 0xfe, impdep2    = 0xff
        }

        private enum ArrayTypeCodes
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

        public static Memory<int> Stack { get; private set; } = new Memory<int>(new int[short.MaxValue]);
        public static Stack<MethodFrame> MethodFrameStack { get; private set; } = new();
        public static int ActiveException { get; private set; } = 0;

        private static (MethodFrame ThrowingFrame, int Exception)? pendingException;

        public static void BeginExecution(MethodInfo methodInfo, params int[] arguments)
        {
            RunJavaFunction(methodInfo, arguments);
            MethodFrame frame = new(methodInfo,
                                    0,
                                    methodInfo.HasFlag(MethodInfoFlag.Native) ? ExecuteNative() : null);
            MethodFrameStack.Push(frame);
            MainLoop();
        }

        public static void MainLoop()
        {
            while (MethodFrameStack.Count > 0)
            {
                // Execute the method at the top of the stack until it reaches a function call or returns

                MethodFrame currFrame = MethodFrameStack.Peek();
                MethodInfo? methodToPush = null;
                if (currFrame.Method.HasFlag(MethodInfoFlag.Native))
                {
                    if (ActiveException != 0 &&                 // No active exception (all native methods propogate exceptions for now)
                        currFrame.NativeState!.MoveNext())      // Native method has not yet returned
                    {
                        methodToPush = currFrame.NativeState.Current;
                    }
                }
                else
                {
                    methodToPush = InterpretUntilCallOrRet();
                }


                if (methodToPush == null)                                   // Method terminated: Pop it from the stack & propogate exception if needed
                {
                    MethodFrameStack.Pop();
                    if (MethodFrameStack.TryPeek(out MethodFrame parentFrame))
                    {
                        if (pendingException != null && pendingException.Value.ThrowingFrame.BaseOffset == parentFrame.BaseOffset)
                        {
                            ActiveException = pendingException.Value.Exception;
                            pendingException = null;
                        }

                        if (ActiveException != 0)
                        {
                            // Push exception onto parent stack

                            parentFrame.Stack[0] = ActiveException;
                            parentFrame.SP = 1;
                        }
                    }
                }
                else                                                        // Method called another function: Push new method frame onto the stack
                {
                    int newFrameOffset = currFrame.BaseOffset + currFrame.Method.MaxLocals + 2 + currFrame.SP;
                    int newFrameSize = methodToPush.MaxLocals + 2 + methodToPush.MaxStack;
                    if (newFrameOffset + newFrameSize >= Stack.Length)
                    {
                        // TODO: Throw java.lang.StackOverflowError

                        throw new NotImplementedException();
                    }

                    MethodFrame frame = new(methodToPush,
                                            newFrameOffset,
                                            methodToPush.HasFlag(MethodInfoFlag.Native) ? ExecuteNative() : null);
                    MethodFrameStack.Push(frame);
                }
            }
        }

        public static MethodInfo RunJavaFunction(MethodInfo methodInfo, params int[] arguments)
        {
            Program.StackTracePrinter.PrintMethodCall(methodInfo, arguments);
            int argumentsLocation = MethodFrameStack.TryPeek(out MethodFrame currFrame)
                                    ? currFrame.BaseOffset + currFrame.Method.MaxLocals + 2 + currFrame.SP
                                    : 0;
            arguments.CopyTo(Stack.Slice(argumentsLocation));
            return methodInfo;
        }

        public static MethodInfo ThrowJavaException(string type)
        {
            int exceptionCFileIdx = ClassFileManager.GetClassFileIndex(type);
            ClassFile exceptionCFile = ClassFileManager.ClassFiles[exceptionCFileIdx];
            MethodInfo initMethod = exceptionCFile.MethodDictionary[("<init>", "()V")];
            return ThrowJavaException(initMethod);
        }

        public static MethodInfo ThrowJavaException(MethodInfo initMethod, params int[] arguments)
        {
            if (pendingException != null)
            {
                // Attempting to throw an exception during the construction of another exception. Shouldn't be possible?
                throw new InvalidOperationException();
            }

            int exceptionObjRef = Heap.CreateObject(ClassFileManager.GetClassFileIndex(initMethod.ClassFile.Name));
            pendingException = (MethodFrameStack.Peek(), exceptionObjRef);

            int[] allArgs = new int[arguments.Length + 1];
            allArgs[0] = exceptionObjRef;
            arguments.CopyTo(allArgs, 1);
            return RunJavaFunction(initMethod, allArgs);
        }

        public static IEnumerator<MethodInfo> ExecuteNative()
        {
            MethodFrame thisFrame = MethodFrameStack.Peek();
            MethodInfo methodInfo = thisFrame.Method;
            string className = methodInfo.ClassFile.Name;
            string thisFuncName = methodInfo.Name;
            string thisDescriptor = methodInfo.Descriptor;
            
            int[] args = Stack.Span.Slice(thisFrame.BaseOffset, methodInfo.MaxLocals).ToArray();
            HeapObject? thisObj = methodInfo.HasFlag(MethodInfoFlag.Static) ? default : Heap.GetObject(args[0]);

            int PopInt()
            {
                return thisFrame.Stack[--thisFrame.SP];
            }

            switch (className, thisFuncName, thisDescriptor)
            {
                case ("java/io/FileDescriptor", "initIDs", "()V"):
                    JavaHelper.ReturnVoid();
                    yield break;
                case ("java/io/FileDescriptor", "set", "(I)J"):
                    {
                        int fd = args[0];
                        if (fd == 0) //in
                        {
                            JavaHelper.ReturnLargeValue(0L);
                        }
                        else if (fd == 1) //out
                        {
                            JavaHelper.ReturnLargeValue(1L);
                        }
                        else if (fd == 2) //err
                        {
                            JavaHelper.ReturnLargeValue(2L);
                        }
                        else
                        {
                            JavaHelper.ReturnLargeValue(-1L);
                        }
                        yield break;
                    }
                case ("java/io/FileInputStream", "available0", "()I"):
                    {
                        int pathFieldAddr = thisObj.GetField("path", "Ljava/lang/String;");
                        if (pathFieldAddr == 0)
                        {
                            int fileDescriptorAddr = thisObj.GetField("fd", "Ljava/io/FileDescriptor;");
                            long handle = Heap.GetObject(fileDescriptorAddr).GetField("handle", "J");
                            if (handle != 0)
                            {
                                yield return ThrowJavaException("java/io/IOException");
                                ActiveException = thisFrame.Stack[0];
                                yield break;
                            }
                            else
                            {
                                int available = FileStreams.AvailableBytesFromConsole();
                                JavaHelper.ReturnValue(available);
                            }
                        }
                        else
                        {
                            string path = JavaHelper.ReadJavaString(thisObj.GetField("path", "Ljava/lang/String;"));
                            int available = FileStreams.AvailableBytes(path);
                            JavaHelper.ReturnValue(available);
                        }
                        yield break;
                    }
                case ("java/io/FileInputStream", "close0", "()V"):
                    {
                        string path = JavaHelper.ReadJavaString(thisObj.GetField("path", "Ljava/lang/String;"));
                        FileStreams.Close(path);
                        thisObj.SetField("closed", "Z", 1);
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/io/FileInputStream", "initIDs", "()V"):
                    JavaHelper.ReturnVoid();
                    yield break;
                case ("java/io/FileInputStream", "open0", "(Ljava/lang/String;)V"):
                    {
                        string fileName = JavaHelper.ReadJavaString(args[1]);
                        if (FileStreams.OpenRead(fileName))
                        {
                            thisObj.SetField("path", "Ljava/lang/String;", args[1]);
                        }
                        else
                        {
                            yield return ThrowJavaException("java/io/FileNotFoundException");
                            ActiveException = thisFrame.Stack[0];
                            yield break;
                        }
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/io/FileInputStream", "readBytes", "([BII)I"):
                    {

                        int pathFieldAddr = thisObj.GetField("path", "Ljava/lang/String;");

                        int byteArrAddr = args[1];
                        int offset = args[2];
                        int length = args[3];

                        if (pathFieldAddr == 0)
                        {
                            int fileDescriptorAddr = thisObj.GetField("fd", "Ljava/io/FileDescriptor;");
                            long handle = Heap.GetObject(fileDescriptorAddr).GetFieldLong("handle", "J");
                            if (handle != 0)
                            {
                                yield return ThrowJavaException("java/io/IOException");
                                ActiveException = thisFrame.Stack[0];
                                yield break;
                            }
                            else
                            {
                                HeapArray javaByteArr = Heap.GetArray(byteArrAddr);
                                int bytesRead = FileStreams.ReadBytesFromConsole(javaByteArr.GetDataSpan().Slice(offset, length));

                                JavaHelper.ReturnValue(bytesRead);
                                yield break;
                            }
                        }
                        else
                        {
                            string path = JavaHelper.ReadJavaString(pathFieldAddr);
                            HeapArray javaByteArr = Heap.GetArray(byteArrAddr);

                            int bytesRead = FileStreams.ReadBytes(path, javaByteArr.GetDataSpan().Slice(offset, length));

                            JavaHelper.ReturnValue(bytesRead);
                            yield break;
                        }
                    }
                case ("java/io/FileOutputStream", "close0", "()V"):
                    {
                        string path = JavaHelper.ReadJavaString(thisObj.GetField("path", "Ljava/lang/String;"));
                        FileStreams.Close(path);
                        thisObj.SetField("closed", "Z", 1);
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/io/FileOutputStream", "initIDs", "()V"):
                    JavaHelper.ReturnVoid();
                    yield break;
                case ("java/io/FileOutputStream", "open0", "(Ljava/lang/String;Z)V"):
                    {
                        string fileName = JavaHelper.ReadJavaString(args[1]);
                        if (FileStreams.OpenWrite(fileName))
                        {
                            thisObj.SetField("path", "Ljava/lang/String;", args[1]);
                        }
                        else
                        {
                            yield return ThrowJavaException("java/io/FileNotFoundException");
                            ActiveException = thisFrame.Stack[0];
                            yield break;
                        }
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/io/FileOutputStream", "writeBytes", "([BIIZ)V"):
                    {
                        int pathFieldAddr = thisObj.GetField("path", "Ljava/lang/String;");
                        int byteArrAddr = args[1];
                        int offset = args[2];
                        int length = args[3];
                        bool append = args[4] != 0;

                        if (pathFieldAddr == 0)
                        {
                            HeapObject fileDescriptor = Heap.GetObject(thisObj.GetField("fd", "Ljava/io/FileDescriptor;"));
                            long handle = fileDescriptor.GetFieldLong("handle", "J"); //address (defined in java/io/FileDescriptor set(int))
                            if (handle == 1)
                            {
                                HeapArray javaByteArr = Heap.GetArray(byteArrAddr);
                                FileStreams.WriteBytesToConsole(javaByteArr.GetDataSpan().Slice(offset, length));

                                JavaHelper.ReturnVoid();
                                yield break;
                            }
                            else if (handle == 2)
                            {
                                HeapArray javaByteArr = Heap.GetArray(byteArrAddr);
                                FileStreams.WriteBytesToError(javaByteArr.GetDataSpan().Slice(offset, length));

                                JavaHelper.ReturnVoid();
                                yield break;
                            }
                            else
                            {
                                yield return ThrowJavaException("java/io/IOException");
                                ActiveException = thisFrame.Stack[0];
                                yield break;
                            }
                        }
                        else
                        {
                            string path = JavaHelper.ReadJavaString(pathFieldAddr);
                            HeapArray javaByteArr = Heap.GetArray(byteArrAddr);

                            throw new NotImplementedException();
                        }

                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/io/WinNTFileSystem", "canonicalize0", "(Ljava/lang/String;)Ljava/lang/String;"):
                    {
                        string input = JavaHelper.ReadJavaString(args[1]);

                        string absoluteForm = Path.GetFullPath(input);
                        //not finished

                        JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(absoluteForm));
                        yield break;
                    }
                case ("java/io/WinNTFileSystem", "createFileExclusively", "(Ljava/lang/String;)Z"):
                    {
                        string path = JavaHelper.ReadJavaString(args[1]);
                        bool exception = false;
                        try
                        {
                            if (File.Exists(path))
                            {
                                JavaHelper.ReturnValue(0);
                            }
                            else
                            {
                                File.Create(path);
                                JavaHelper.ReturnValue(1);
                            }
                        }
                        catch (IOException)
                        {
                            exception = true;
                            yield break;
                        }
                        if (exception)
                        {
                            yield return ThrowJavaException("java/io/IOException");
                            ActiveException = thisFrame.Stack[0];
                            yield break;
                        }
                        yield break;
                    }
                case ("java/io/WinNTFileSystem", "getBooleanAttributes", "(Ljava/io/File;)I"):
                    {
                        ClassFile fileCFile = ClassFileManager.GetClassFile("java/io/File");
                        MethodInfo getPathMethod = fileCFile.MethodDictionary[("getPath", "()Ljava/lang/String;")];
                        
                        yield return RunJavaFunction(getPathMethod, args[1]);

                        string path = JavaHelper.ReadJavaString(PopInt());

                        bool exception = false;
                        try
                        {
                            FileAttributes attributes = File.GetAttributes(path);
                            int ret = (int)attributes;
                            JavaHelper.ReturnValue(ret);

                        }
                        catch (FileNotFoundException)
                        {
                            exception = true;
                        }
                        if (exception)
                        {
                            yield return ThrowJavaException("java/io/FileNotFoundException");
                            ActiveException = thisFrame.Stack[0];
                            yield break;
                        }
                        yield break;
                    }
                case ("java/io/WinNTFileSystem", "initIDs", "()V"):
                    JavaHelper.ReturnVoid();
                    yield break;
                case ("java/lang/Class", "desiredAssertionStatus0", "(Ljava/lang/Class;)Z"):
                    JavaHelper.ReturnValue(0);
                    yield break;
                case ("java/lang/Class", "forName0", "(Ljava/lang/String;ZLjava/lang/ClassLoader;Ljava/lang/Class;)Ljava/lang/Class;"):
                    {
                        //supposed to use classloader that is passed in
                        string classToLoadName = JavaHelper.ReadJavaString(args[0]).Replace('.', '/');
                        int classObjAddr = ClassObjectManager.GetClassObjectAddr(classToLoadName);

                        ClassFileManager.GetClassFileIndex(classToLoadName);
                        if (args[1] == 1)
                        {
                            ClassFileManager.InitializeClass(classToLoadName);
                        }
                        JavaHelper.ReturnValue(classObjAddr);
                        yield break;
                    }
                case ("java/lang/Class", "getClassLoader0", "()Ljava/lang/ClassLoader;"):
                    JavaHelper.ReturnValue(0);
                    yield break;
                case ("java/lang/Class", "getComponentType", "()Ljava/lang/Class;"):
                    {
                        string name = JavaHelper.ClassObjectName(thisObj);
                        if (!name.StartsWith('['))
                        {
                            JavaHelper.ReturnValue(0);
                        }
                        else
                        {
                            int itemType = ClassObjectManager.GetClassObjectAddr(name.Substring(1));
                            JavaHelper.ReturnValue(itemType);
                        }
                        yield break;
                    }
                case ("java/lang/Class", "getDeclaredConstructors0", "(Z)[Ljava/lang/reflect/Constructor;"):
                    {
                        ClassFile cFile = ClassFileManager.GetClassFile(JavaHelper.ClassObjectName(thisObj));

                        int constructorClassFileIdx = ClassFileManager.GetClassFileIndex("java/lang/reflect/Constructor");
                        ClassFile constructorClassFile = ClassFileManager.ClassFiles[constructorClassFileIdx];
                        MethodInfo constructorConstructor = constructorClassFile.MethodDictionary[("<init>", "(Ljava/lang/Class;[Ljava/lang/Class;[Ljava/lang/Class;IILjava/lang/String;[B[B)V")];

                        LinkedList<int> constructorObjLinkedList = new LinkedList<int>();
                        int i = 0;
                        int declaringClass = args[0];
                        foreach (MethodInfo method in cFile.MethodDictionary.Values)
                        {
                            if (method.Name == "<init>")
                            {
                                string descriptor = method.Descriptor;
                                string descriptorArgs = descriptor.Split('(', ')')[1];
                                int[] parameterTypes;
                                if (descriptorArgs == "")
                                {
                                    parameterTypes = new int[] { };
                                }
                                else
                                {
                                    throw new NotImplementedException();
                                    string[] parameterDescriptors = descriptorArgs.Split(',');
                                    parameterTypes = new int[parameterDescriptors.Length];
                                    for (int j = 0; j < parameterTypes.Length; j++)
                                    {
                                        parameterTypes[j] = ClassObjectManager.GetClassObjectAddr(parameterDescriptors[j]);
                                    }
                                }
                                int parameterTypesArrayAddr = Heap.CreateArray(parameterTypes, ClassObjectManager.GetClassObjectAddr("java/lang/Class"));

                                int checkedExceptionsArrayAddr = 0;
                                if (method.ExceptionsAttribute != null)
                                {
                                    CClassInfo[] ExceptionsTable = method.ExceptionsAttribute.ExceptionsTable;
                                    int[] checkedExceptions = new int[ExceptionsTable.Length];
                                    for (int j = 0; j < checkedExceptions.Length; j++)
                                    {
                                        checkedExceptions[j] = ClassObjectManager.GetClassObjectAddr(ExceptionsTable[j].Name);
                                    }
                                    checkedExceptionsArrayAddr = Heap.CreateArray(checkedExceptions, ClassObjectManager.GetClassObjectAddr("Ljava/lang/Class"));
                                }

                                int modifiers = method.AccessFlags;

                                int slot = i;

                                string signature = descriptor;
                                int signatureObjAddr = JavaHelper.CreateJavaStringLiteral(signature);

                                int constructorObj = Heap.CreateObject(constructorClassFileIdx);

                                if (method.RawAnnotations == null)
                                {
                                    bool foundAnnotationsAttribute = false;
                                    foreach (AttributeInfo attribute in method.Attributes)
                                    {
                                        if (attribute.GetType() == typeof(RuntimeVisibleAnnotationsAttribute))
                                        {
                                            foundAnnotationsAttribute = true;
                                            RuntimeVisibleAnnotationsAttribute annotationsAttr = (RuntimeVisibleAnnotationsAttribute)attribute;
                                            method.RawAnnotations = new byte[annotationsAttr.NumAnnotations * 2 + 2];

                                            method.RawAnnotations[0] = (byte)((annotationsAttr.NumAnnotations >> 8) & 0xFFFF);
                                            method.RawAnnotations[1] = (byte)(annotationsAttr.NumAnnotations & 0xFFFF);

                                            for (int j = 0; j < annotationsAttr.NumAnnotations; j++)
                                            {
                                                Annotation annotation = annotationsAttr.Annotations[j];

                                                method.RawAnnotations[j * 2 + 2] = (byte)((annotation.TypeIndex >> 8) & 0xFFFF);
                                                method.RawAnnotations[j * 2 + 3] = (byte)(annotation.TypeIndex & 0xFFFF);
                                            }
                                            break;
                                        }
                                    }
                                    if (!foundAnnotationsAttribute)
                                    {
                                        method.RawAnnotations = new byte[] { };
                                    }
                                }

                                int annotationsArrayAddr = 0;
                                if (method.RawAnnotations.Length > 0)
                                {
                                    int[] intArr = method.RawAnnotations.Select(x => (int)x).ToArray();
                                    annotationsArrayAddr = Heap.CreateArray(intArr, ClassObjectManager.GetClassObjectAddr("B"));
                                }

                                if (method.RawParameterAnnotations == null)
                                {
                                    bool foundParameterAnnotationsAttribute = false;
                                    foreach (AttributeInfo attribute in method.Attributes)
                                    {
                                        if (attribute.GetType() == typeof(RuntimeVisibleParameterAnnotationsAttribute))
                                        {
                                            foundParameterAnnotationsAttribute = true;
                                            throw new NotImplementedException();
                                        }
                                    }
                                    if (!foundParameterAnnotationsAttribute)
                                    {
                                        method.RawParameterAnnotations = new byte[] { };
                                    }
                                }

                                int parameterAnnotationsArrayAddr = 0;
                                if (method.RawParameterAnnotations.Length > 0)
                                {
                                    int[] intArr = method.RawParameterAnnotations.Select(x => (int)x).ToArray();
                                    parameterAnnotationsArrayAddr = Heap.CreateArray(intArr, ClassObjectManager.GetClassObjectAddr("B"));
                                }

                                yield return RunJavaFunction(constructorConstructor, constructorObj,
                                    declaringClass,
                                    parameterTypesArrayAddr,
                                    checkedExceptionsArrayAddr,
                                    modifiers,
                                    slot,
                                    signatureObjAddr,
                                    annotationsArrayAddr,
                                    parameterAnnotationsArrayAddr);

                                constructorObjLinkedList.AddLast(constructorObj);
                            }
                            i++;
                        }
                        int[] constructorObjAddresses = new int[constructorObjLinkedList.Count];
                        constructorObjLinkedList.CopyTo(constructorObjAddresses, 0);

                        int constructorObjArrayAddr = Heap.CreateArray(constructorObjAddresses, ClassObjectManager.GetClassObjectAddr("Ljava/lang/reflect/Constructor;"));
                        JavaHelper.ReturnValue(constructorObjArrayAddr);
                        yield break;
                    }
                case ("java/lang/Class", "getDeclaredFields0", "(Z)[Ljava/lang/reflect/Field;"):
                    {
                        bool publicOnly = args[1] != 0;
                        ClassFile cFile = ClassFileManager.GetClassFile(JavaHelper.ClassObjectName(thisObj));
                        int length;
                        if (publicOnly)
                        {
                            length = 0;
                            foreach (FieldInfo fieldInfo in cFile.DeclaredFields)
                            {
                                if (fieldInfo.HasFlag(FieldInfoFlag.Public))
                                {
                                    length++;
                                }
                            }
                        }
                        else
                        {
                            length = cFile.DeclaredFields.Count;
                        }

                        int[] fields = new int[length];
                        int instanceSlot = 0;
                        int staticSlot = 0;
                        int addIdx = 0;

                        foreach (FieldInfo field in cFile.DeclaredFields)
                        {
                            if (publicOnly && !field.HasFlag(FieldInfoFlag.Public))
                            {
                                continue;
                            }

                            int fieldCFileIdx = ClassFileManager.GetClassFileIndex("java/lang/reflect/Field");
                            ClassFile fieldCFile = ClassFileManager.ClassFiles[fieldCFileIdx];
                            int fieldAddr = Heap.CreateObject(fieldCFileIdx);

                            MethodInfo initMethod = fieldCFile.MethodDictionary[("<init>", "(Ljava/lang/Class;Ljava/lang/String;Ljava/lang/Class;IILjava/lang/String;[B)V")];

                            string type;
                            if (field.Descriptor.Length == 1)
                            {
                                type = JavaHelper.PrimitiveFullName(field.Descriptor);
                            }
                            else type = field.Descriptor;

                            int slotArg;
                            if (field.HasFlag(FieldInfoFlag.Static))
                            {
                                slotArg = staticSlot++;
                            }
                            else
                            {
                                slotArg = instanceSlot++;
                            }

                            yield return RunJavaFunction(initMethod, fieldAddr,
                                                                ClassObjectManager.GetClassObjectAddr(cFile.Name),
                                                                JavaHelper.CreateJavaStringLiteral(field.Name),
                                                                ClassObjectManager.GetClassObjectAddr(type),
                                                                field.AccessFlags,
                                                                slotArg,
                                                                JavaHelper.CreateJavaStringLiteral(field.Descriptor),
                                                                0);

                            fields[addIdx++] = fieldAddr;
                        }

                        int fieldsArrAddr = Heap.CreateArray(fields, ClassObjectManager.GetClassObjectAddr("Ljava/lang/reflect/Field;"));
                        JavaHelper.ReturnValue(fieldsArrAddr);
                        yield break;
                    }
                case ("java/lang/Class", "getDeclaringClass0", "()Ljava/lang/Class;"):
                    {
                        string name = JavaHelper.ClassObjectName(thisObj);
                        if (JavaHelper.IsPrimitiveType(name))
                        {
                            JavaHelper.ReturnValue(0);
                            yield break;
                        }

                        ClassFile cFile = ClassFileManager.GetClassFile(name);
                        foreach (AttributeInfo attribute in cFile.Attributes)
                        {
                            if (attribute is InnerClassesAttribute icAttribute)
                            {
                                foreach (ClassTableEntry entry in icAttribute.Classes)
                                {
                                    if (icAttribute.Classes[0].InnerClassName == name)
                                    {
                                        CClassInfo outerClassInfo = icAttribute.Classes[0].OuterClassInfo;
                                        int outerClassObj = ClassObjectManager.GetClassObjectAddr(outerClassInfo);

                                        JavaHelper.ReturnValue(outerClassObj);
                                        yield break;
                                    }
                                }
                                JavaHelper.ReturnValue(0);
                                yield break;
                            }
                        }
                        JavaHelper.ReturnValue(0);
                        yield break;
                    }
                case ("java/lang/Class", "getEnclosingMethod0", "()[Ljava/lang/Object;"):
                    {
                        string name = JavaHelper.ClassObjectName(thisObj);
                        if (JavaHelper.IsPrimitiveType(name))
                        {
                            JavaHelper.ReturnValue(0);
                            yield break;
                        }

                        ClassFile cFile = ClassFileManager.GetClassFile(name);
                        foreach (AttributeInfo attribute in cFile.Attributes)
                        {
                            if (attribute is EnclosingMethodAttribute emAttribute)
                            {
                                throw new NotImplementedException();
                            }
                        }
                        JavaHelper.ReturnValue(0);
                        yield break;
                    }
                case ("java/lang/Class", "getModifiers", "()I"):
                    {
                        ClassFile cFile = ClassFileManager.GetClassFile(JavaHelper.ClassObjectName(thisObj));

                        JavaHelper.ReturnValue(cFile.AccessFlags);
                        yield break;
                    }
                case ("java/lang/Class", "getPrimitiveClass", "(Ljava/lang/String;)Ljava/lang/Class;"):
                    {
                        string name = JavaHelper.ReadJavaString(args[0]);
                        int classObjAddr = classObjAddr = ClassObjectManager.GetClassObjectAddr(name);
                        JavaHelper.ReturnValue(classObjAddr);
                        yield break;
                    }
                case ("java/lang/Class", "getProtectionDomain0", "()Ljava/security/ProtectionDomain;"):
                    {
                        JavaHelper.ReturnValue(0);
                        yield break;
                    }
                case ("java/lang/Class", "getSuperclass", "()Ljava/lang/Class;"):
                    {
                        string descriptor = JavaHelper.ReadJavaString(thisObj.GetField(2));
                        if (descriptor == "java.lang.Object" || descriptor.Length == 1 || thisObj.ClassFile.IsInterface())
                        {
                            JavaHelper.ReturnValue(0);
                            yield break;
                        }
                        ClassFile cFile = ClassFileManager.GetClassFile(descriptor);
                        ClassFile superClass = cFile.SuperClass;
                        JavaHelper.ReturnValue(ClassObjectManager.GetClassObjectAddr(superClass.Name));
                        yield break;
                    }
                case ("java/lang/Class", "isArray", "()Z"):
                    {
                        string name = JavaHelper.ClassObjectName(thisObj);
                        bool isArray = name[0] == '[';
                        JavaHelper.ReturnValue(isArray ? 1 : 0);
                        yield break;
                    }
                case ("java/lang/Class", "isAssignableFrom", "(Ljava/lang/Class;)Z"):
                    {
                        if (args[1] == 0)
                        {
                            yield return ThrowJavaException("java/lang/NullPointerException");
                            ActiveException = thisFrame.Stack[0];
                            yield break;
                        }
                        int thisCFileIdx = ClassFileManager.GetClassFileIndex(args[0]);
                        int otherCFileIdx = ClassFileManager.GetClassFileIndex(args[1]);
                        ClassFile thisCFile = ClassFileManager.ClassFiles[thisCFileIdx];
                        ClassFile otherCFile = ClassFileManager.ClassFiles[otherCFileIdx];
                        bool assignable = JavaHelper.IsSubClassOf(otherCFile, thisCFile);
                        JavaHelper.ReturnValue(assignable ? 1 : 0);
                        yield break;
                    }
                case ("java/lang/Class", "isInstance", "(Ljava/lang/Object;)Z"):
                    {
                        HeapObject objToCheck = Heap.GetObject(args[1]);
                        JavaHelper.ReturnValue(objToCheck.IsInstance(args[0]) ? 1 : 0);
                        yield break;
                    }
                case ("java/lang/Class", "isInterface", "()Z"):
                    {
                        string name = JavaHelper.ReadJavaString(thisObj.GetField("name", "Ljava/lang/String;"));
                        ClassFile cFile = ClassFileManager.GetClassFile(name);
                        JavaHelper.ReturnValue(cFile.IsInterface() ? 1 : 0);
                        yield break;
                    }
                case ("java/lang/Class", "isPrimitive", "()Z"):
                    {
                        string name = JavaHelper.ClassObjectName(thisObj);
                        JavaHelper.ReturnValue(JavaHelper.IsPrimitiveType(name) ? 1 : 0);
                        yield break;
                    }
                case ("java/lang/Class", "registerNatives", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/Class", "<clinit>", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/ClassLoader", "findBuiltinLib", "(Ljava/lang/String;)Ljava/lang/String;"):
                    {
                        //temp
                        string name = JavaHelper.ReadJavaString(args[0]);
                        ClassFile systemCFile = ClassFileManager.GetClassFile("java/lang/System");
                        MethodInfo getPropMethod = systemCFile.MethodDictionary[("getProperty", "(Ljava/lang/String;)Ljava/lang/String;")];
                        yield return RunJavaFunction(getPropMethod, JavaHelper.CreateJavaStringLiteral("java.library.path"));
                        int libPathAddr = PopInt();
                        string libPath = JavaHelper.ReadJavaString(libPathAddr);
                        JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(libPath + name));
                        yield break;
                    }
                case ("java/lang/ClassLoader", "findLoadedClass0", "(Ljava/lang/String;)Ljava/lang/Class;"):
                    {
                        JavaHelper.ReturnValue(ClassObjectManager.GetClassObjectAddr(JavaHelper.ReadJavaString(args[1])));
                        yield break;
                    }
                case ("java/lang/ClassLoader", "registerNatives", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/ClassLoader$NativeLibrary", "load", "(Ljava/lang/String;Z)V"):
                    {
                        ClassFile classLoaderCFile = ClassFileManager.GetClassFile("java/lang/ClassLoader");
                        int systemNativeLibrariesRef = (int)classLoaderCFile.StaticFieldsDictionary[("systemNativeLibraries", "Ljava/util/Vector;")];
                        HeapObject systemNativeLibraries = Heap.GetObject(systemNativeLibrariesRef);
                        int loadedLibraryNamesRef = (int)classLoaderCFile.StaticFieldsDictionary[("loadedLibraryNames", "Ljava/util/Vector;")];
                        HeapObject loadedLibraryNames = Heap.GetObject(loadedLibraryNamesRef);

                        ClassFile vectorCFile = ClassFileManager.GetClassFile("java/util/Vector");
                        MethodInfo addItemToVector = vectorCFile.MethodDictionary[("addElement", "(Ljava/lang/Object;)V")];

                        yield return RunJavaFunction(addItemToVector, systemNativeLibrariesRef, args[0]);
                        yield return RunJavaFunction(addItemToVector, loadedLibraryNamesRef, args[1]);

                        thisObj.SetField("loaded", "Z", 1);

                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/Double", "doubleToRawLongBits", "(D)J"):
                    {
                        //reversed?
                        //Double is already represented as long
                        JavaHelper.ReturnLargeValue((args[1], args[0]).ToLong());
                        yield break;
                    }
                case ("java/lang/Double", "longBitsToDouble", "(J)D"):
                    {
                        //reversed?
                        //Double is stored as long
                        JavaHelper.ReturnLargeValue((args[1], args[0]).ToLong());
                        yield break;
                    }
                case ("java/lang/Float", "floatToRawIntBits", "(F)I"):
                    {
                        //Float is already represented as int
                        JavaHelper.ReturnValue(args[0]);
                        yield break;
                    }
                case ("java/lang/invoke/MethodHandleNatives", "getConstant", "(I)I"):
                    {
                        if (args[0] == 0) //MethodHandlePushLimit
                        {
                            JavaHelper.ReturnValue(0);
                        }
                        else //1 = stack slot push size
                        {
                            JavaHelper.ReturnValue(0);
                        }
                        yield break;
                    }
                case ("java/lang/invoke/MethodHandleNatives", "registerNatives", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/invoke/MethodHandleNatives", "resolve", "(Ljava/lang/invoke/MemberName;Ljava/lang/Class;)Ljava/lang/invoke/MemberName;"):
                    {
                        throw new NotImplementedException();
                        HeapObject self = Heap.GetObject(args[0]);
                        HeapObject caller = Heap.GetObject(args[1]);

                        int classFieldAddr = self.GetField(0);
                        string methodClass = JavaHelper.ClassObjectName(classFieldAddr);
                        int nameFieldAddr = self.GetField(1);
                        string name = JavaHelper.ReadJavaString(nameFieldAddr);

                        HeapObject methodType = Heap.GetObject(self.GetField("type", "Ljava/lang/Object;"));
                        HeapObject returnType = Heap.GetObject(methodType.GetField("rtype", "Ljava/lang/Class;"));
                        HeapArray parameterTypes = Heap.GetArray(methodType.GetField("ptypes", "[Ljava/lang/Class;"));

                        string descriptor = JavaHelper.MakeDescriptor(returnType, parameterTypes);

                        int cFileIdx = ClassFileManager.GetClassFileIndex(methodClass);
                        MethodInfo method = ClassFileManager.ClassFiles[cFileIdx].MethodDictionary[(name, descriptor)];

                        int flags = self.GetField("flags", "I");
                        if (method.HasFlag(MethodInfoFlag.Static))
                        {

                        }


                        JavaHelper.ReturnValue(args[0]);
                        yield break;
                    }
                case ("java/lang/Object", "clone", "()Ljava/lang/Object;"):
                    {
                        /*
                         * What does this mean?
                         * 
                         * By convention, the returned object should be obtained by calling
                         * {@code super.clone}.  If a class and all of its superclasses (except
                         * {@code Object}) obey this convention, it will be the case that
                         * {@code x.clone().getClass() == x.getClass()}.
                         * 
                         */
                        JavaHelper.ReturnValue(Heap.CloneObject(args[0]));
                        yield break;
                    }
                case ("java/lang/Object", "getClass", "()Ljava/lang/Class;"):
                    {
                        if (thisObj is HeapArray arr)
                        {
                            int itemTypeAddr = arr.ItemTypeClassObjAddr;
                            int nameAddr = Heap.GetObject(itemTypeAddr).GetField("name", "Ljava/lang/String;");
                            string itemTypeName = JavaHelper.ReadJavaString(nameAddr);
                            string arrName = '[' + itemTypeName;
                            JavaHelper.ReturnValue(ClassObjectManager.GetClassObjectAddr(arrName));
                        }
                        else
                        {
                            JavaHelper.ReturnValue(ClassObjectManager.GetClassObjectAddr(thisObj.ClassFile.Name));
                        }
                        yield break;
                    }
                case ("java/lang/Object", "hashCode", "()I"):
                    {
                        //return new int[] {address of object
                        JavaHelper.ReturnValue(args[0]);
                        yield break;
                    }
                case ("java/lang/Object", "notifyAll", "()V"):
                    {
                        //don't support multiple threads
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/Object", "registerNatives", "()V"):
                    {
                        //unsure what natives to register
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/reflect/Array", "newArray", "(Ljava/lang/Class;I)Ljava/lang/Object;"):
                    {
                        int typeClassObjAddr = args[0];
                        int length = args[1];
                        if (length < 0)
                        {
                            yield return ThrowJavaException("java/lang/NegativeArraySizeException");
                            ActiveException = thisFrame.Stack[0];
                            yield break;
                        }
                        JavaHelper.ReturnValue(Heap.CreateArray(4, length, typeClassObjAddr));
                        yield break;
                    }
                case ("java/lang/Runtime", "availableProcessors", "()I"):
                    {
                        JavaHelper.ReturnValue(1);
                        yield break;
                    }
                case ("java/lang/String", "intern", "()Ljava/lang/String;"):
                    {
                        string str = JavaHelper.ReadJavaString(args[0]);
                        if (StringPool.StringAddresses.TryGetValue(str, out int strAddr))
                        {
                            JavaHelper.ReturnValue(strAddr);
                        }
                        else
                        {
                            StringPool.StringAddresses.Add(str, args[0]);
                            JavaHelper.ReturnValue(args[0]);
                        }
                        yield break;
                    }
                case ("java/lang/System", "arraycopy", "(Ljava/lang/Object;ILjava/lang/Object;II)V"):
                    {
                        HeapArray srcArr = Heap.GetArray(args[0]);
                        int srcStartInd = args[1];
                        HeapArray destArr = Heap.GetArray(args[2]);
                        int destStartInd = args[3];
                        int count = args[4];

                        int itemSize = srcArr.ItemSize;
                        int numBytes = count * srcArr.ItemSize;
                        Span<byte> srcSpan = srcArr.GetDataSpan().Slice(itemSize * srcStartInd, numBytes);
                        Span<byte> destSpan = destArr.GetDataSpan().Slice(itemSize * destStartInd);
                        srcSpan.CopyTo(destSpan);
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/System", "currentTimeMillis", "()J"):
                    {
                        DateTime dt1970 = new DateTime(1970, 1, 1);
                        TimeSpan timeSince = DateTime.Now - dt1970;
                        long totalMillis = (long)timeSince.TotalMilliseconds;
                        JavaHelper.ReturnLargeValue(totalMillis);
                        yield break;
                    }
                case ("java/lang/System", "identityHashCode", "(Ljava/lang/Object;)I"):
                    {
                        JavaHelper.ReturnValue(args[0]);
                        yield break;
                    }
                case ("java/lang/System", "initProperties", "(Ljava/util/Properties;)Ljava/util/Properties;"):
                    {
                        HeapObject propertiesObject = Heap.GetObject(args[0]);

                        //Complete list: https://docs.oracle.com/javase/8/docs/api/java/lang/System.html#getProperties--

                        MethodInfo setPropertyMethod = propertiesObject.ClassFile.MethodDictionary[("setProperty", "(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/Object;")];

                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("java.home"),
                                                                            JavaHelper.CreateJavaStringLiteral(Program.Configuration.javaHome)); PopInt();
                        //RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("java.library.path"),
                        //JavaHelper.CreateJavaStringLiteral(Environment.GetEnvironmentVariable("JAVA_HOME") + "\\bin")); PopInt();
                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("file.encoding"),
                                                                            JavaHelper.CreateJavaStringLiteral("UTF16le")); PopInt();
                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("os.arch"),
                                                                            JavaHelper.CreateJavaStringLiteral("x64")); PopInt();
                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("os.name"),
                                                                            JavaHelper.CreateJavaStringLiteral(Environment.OSVersion.Platform.ToString())); PopInt();
                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("os.version"),
                                                                            JavaHelper.CreateJavaStringLiteral(Environment.OSVersion.Version.Major.ToString())); PopInt();
                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("file.separator"),
                                                                            JavaHelper.CreateJavaStringLiteral(Path.DirectorySeparatorChar.ToString())); PopInt();
                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("path.separator"),
                                                                            JavaHelper.CreateJavaStringLiteral(Path.PathSeparator.ToString())); PopInt();
                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("line.separator"),
                                                                            JavaHelper.CreateJavaStringLiteral(Environment.NewLine)); PopInt();
                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("user.name"),
                                                                            JavaHelper.CreateJavaStringLiteral(Environment.UserName)); PopInt();
                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("user.home"),
                                                                            JavaHelper.CreateJavaStringLiteral(Environment.GetFolderPath(Environment.SpecialFolder.Personal))); PopInt();
                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("java.library.path"),
                                                                            JavaHelper.CreateJavaStringLiteral(Program.Configuration.javaHome + @"\lib")); PopInt();
                        yield return RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("sun.lang.ClassLoader.allowArraySyntax"),
                                                                            JavaHelper.CreateJavaStringLiteral("true")); PopInt();


                        JavaHelper.ReturnValue(args[0]);
                        yield break;
                    }
                case ("java/lang/System", "mapLibraryName", "(Ljava/lang/String;)Ljava/lang/String;"):
                    {
                        string libName = JavaHelper.ReadJavaString(args[0]);
                        string mappedName = libName.Replace('.', '/');
                        JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(mappedName));
                        yield break;
                    }
                case ("java/lang/System", "nanoTime", "()J"):
                    {
                        long nanoTime = Program.Stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond * 345365667;
                        JavaHelper.ReturnLargeValue(nanoTime);
                        yield break;
                    }
                case ("java/lang/System", "registerNatives", "()V"):
                    {
                        int printStreamAddr = Heap.CreateObject(ClassFileManager.GetClassFileIndex("java/io/PrintStream"));
                        ClassFileManager.GetClassFile("java/lang/System").StaticFieldsDictionary[("out", "Ljava/io/PrintStream;")] = printStreamAddr;
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/System", "setErr0", "(Ljava/io/PrintStream;)V"):
                    {
                        ClassFile systemClassFile = ClassFileManager.GetClassFile("java/lang/System");
                        systemClassFile.StaticFieldsDictionary[("err", "Ljava/io/PrintStream;")] = args[0];
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/System", "setIn0", "(Ljava/io/InputStream;)V"):
                    {
                        ClassFile systemClassFile = ClassFileManager.GetClassFile("java/lang/System");
                        systemClassFile.StaticFieldsDictionary[("in", "Ljava/io/InputStream;")] = args[0];
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/System", "setOut0", "(Ljava/io/PrintStream;)V"):
                    {
                        ClassFile systemClassFile = ClassFileManager.GetClassFile("java/lang/System");
                        systemClassFile.StaticFieldsDictionary[("out", "Ljava/io/PrintStream;")] = args[0];
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/Thread", "currentThread", "()Ljava/lang/Thread;"):
                    {
                        JavaHelper.ReturnValue(ThreadManager.GetThreadAddr());
                        yield break;
                    }
                case ("java/lang/Thread", "isAlive", "()Z"):
                    {
                        int threadStatus = thisObj.GetField("threadStatus", "I");
                        if (threadStatus == 0)
                        {
                            JavaHelper.ReturnValue(0);
                            yield break;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                case ("java/lang/Thread", "isInterrupted", "(Z)Z"):
                    {
                        int threadStatus = thisObj.GetField("threadStatus", "I");
                        if (threadStatus == 2)
                        {
                            JavaHelper.ReturnValue(1);
                            yield break;
                        }
                        else
                        {
                            JavaHelper.ReturnValue(0);
                            yield break;
                        }
                    }
                case ("java/lang/Thread", "registerNatives", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/Thread", "setPriority0", "(I)V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/lang/Thread", "start0", "()V"):
                    {
                        int targetAddr = thisObj.GetField("target", "Ljava/lang/Runnable;");
                        if (targetAddr == 0)
                        {
                            JavaHelper.ReturnVoid();
                            yield break;
                        }
                        throw new NotImplementedException();
                    }
                case ("java/lang/Throwable", "fillInStackTrace", "(I)Ljava/lang/Throwable;"):
                    {
                        //http://hg.openjdk.java.net/jdk7/jdk7/jdk/file/9b8c96f96a0f/src/share/classes/java/lang/Throwable.java
                        JavaHelper.ReturnValue(args[0]);
                        yield break;
                    }
                case ("java/lang/Throwable", "getStackTraceDepth", "()I"):
                    {
                        JavaHelper.ReturnValue(MethodFrameStack.Count);
                        yield break;
                    }
                case ("java/lang/Throwable", "getStackTraceElement", "(I)Ljava/lang/StackTraceElement;"):
                    {
                        int index = args[1];

                        int stackTraceElementCFileIdx = ClassFileManager.GetClassFileIndex("java/lang/StackTraceElement");
                        ClassFile stackTraceElementCFile = ClassFileManager.ClassFiles[stackTraceElementCFileIdx];
                        int objAddr = Heap.CreateObject(stackTraceElementCFileIdx);

                        MethodInfo ctor = stackTraceElementCFile.MethodDictionary[("<init>", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;I)V")];

                        MethodFrame frame = MethodFrameStack.ToArray()[index];
                        int declaringClass = JavaHelper.CreateJavaStringLiteral(frame.Method.ClassFile.Name);
                        int methodName = JavaHelper.CreateJavaStringLiteral(frame.Method.Name);
                        int fileName = 0;
                        int lineNumber = frame.Method.HasFlag(MethodInfoFlag.Native) ? -2 : -1;

                        yield return RunJavaFunction(ctor, objAddr,
                                                                                    declaringClass,
                                                                                    methodName,
                                                                                    fileName,
                                                                                    lineNumber);
                        JavaHelper.ReturnValue(objAddr);
                        yield break;

                    }
                case ("java/net/DualStackPlainSocketImpl", "connect0", "(ILjava/net/InetAddress;I)I"):
                    {
                        throw new NotImplementedException();
                    }
                case ("java/net/DualStackPlainSocketImpl", "initIDs", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/net/DualStackPlainSocketImpl", "socket0", "(ZZ)I"):
                    {
                        bool stream = args[0] != 0;
                        bool v6Only = args[1] != 0;
                        //Socket socket = new Socket(stream ? SocketType.Stream : SocketType.Dgram, v6Only ? ProtocolType.IcmpV6 : ProtocolType.Tcp);
                        //temp
                        JavaHelper.ReturnValue(0);
                        yield break;
                    }
                case ("java/net/Inet4Address", "init", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/net/Inet6Address", "init", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/net/Inet6AddressImpl", "getLocalHostName", "()Ljava/lang/String;"):
                    {
                        string name = Dns.GetHostName();
                        JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(name));
                        yield break;
                    }
                case ("java/net/Inet6AddressImpl", "lookupAllHostAddr", "(Ljava/lang/String;)[Ljava/net/InetAddress;"):
                    {
                        string hostName = JavaHelper.ReadJavaString(args[1]);
                        IPAddress[] addresses = Dns.GetHostAddresses(hostName);

                        ClassFile inet6AddressCFile = ClassFileManager.GetClassFile("java/net/Inet6Address");
                        MethodInfo constructor = inet6AddressCFile.MethodDictionary[("<init>", "()V")];

                        int[] innerArray = new int[addresses.Length];

                        for (int i = 0; i < addresses.Length; i++)
                        {
                            int newObjAddr = Heap.CreateObject(ClassFileManager.GetClassFileIndex("java/net/Inet6Address"));
                            innerArray[i] = newObjAddr;
                            yield return RunJavaFunction(constructor, newObjAddr);
                        }

                        int arrayAddr = Heap.CreateArray(innerArray, ClassObjectManager.GetClassObjectAddr("java/net/Inet6Address"));

                        JavaHelper.ReturnValue(arrayAddr);
                        yield break;
                    }
                case ("java/net/InetAddress", "init", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/net/InetAddressImplFactory", "isIPv6Supported", "()Z"):
                    {
                        //https://docs.microsoft.com/en-us/dotnet/api/system.net.networkinformation.networkinterface?view=netframework-4.7.2
                        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                        bool supportsIPv6 = false;
                        foreach (NetworkInterface adaptor in networkInterfaces)
                        {
                            if (adaptor.Supports(NetworkInterfaceComponent.IPv6))
                            {
                                supportsIPv6 = true;
                                break;
                            }
                        }
                        JavaHelper.ReturnValue(supportsIPv6 ? 1 : 0);
                        yield break;
                    }
                case ("java/security/AccessController", "doPrivileged", "(Ljava/security/PrivilegedAction;)Ljava/lang/Object;"):
                    {
                        //http://hg.openjdk.java.net/jdk6/jdk6/jdk/file/2d585507a41b/src/share/classes/java/security/AccessController.java
                        //Runs the "run" function of the action
                        //Enables privileges?
                        //Returns result of the "run"
                        HeapObject privilegedAction = Heap.GetObject(args[0]);
                        MethodInfo method = privilegedAction.ClassFile.MethodDictionary[("run", "()Ljava/lang/Object;")];

                        yield return RunJavaFunction(method, args.ToArray());
                        //methodFrame.Execute returns to this NativeMethodFrame's stack
                        JavaHelper.ReturnValue(PopInt());
                        yield break;
                    }
                case ("java/security/AccessController", "doPrivileged", "(Ljava/security/PrivilegedAction;Ljava/security/AccessControlContext;)Ljava/lang/Object;"):
                    {
                        HeapObject privilegedAction = Heap.GetObject(args[0]);
                        MethodInfo method = privilegedAction.ClassFile.MethodDictionary[("run", "()Ljava/lang/Object;")];

                        yield return RunJavaFunction(method, args[0]);
                        JavaHelper.ReturnValue(PopInt());
                        yield break;
                    }
                case ("java/security/AccessController", "doPrivileged", "(Ljava/security/PrivilegedExceptionAction;)Ljava/lang/Object;"):
                    {
                        HeapObject privilegedExceptionAction = Heap.GetObject(args[0]);
                        MethodInfo method = privilegedExceptionAction.ClassFile.MethodDictionary[("run", "()Ljava/lang/Object;")];
                        //DebugWriter.CallFuncDebugWrite(privilegedExceptionAction.ClassObject.Name, "run", Args);
                        yield return RunJavaFunction(method, args.ToArray());
                        //methodFrame.Execute returns to this NativeMethodFrame's stack
                        JavaHelper.ReturnValue(PopInt());
                        yield break;
                    }
                case ("java/security/AccessController", "getStackAccessControlContext", "()Ljava/security/AccessControlContext;"):
                    {
                        //HeapObject accessControlContext = new HeapObject(ClassFileManager.GetClassFile("java/security/AccessControlContext"));
                        //Heap.AddItem(accessControlContext);

                        JavaHelper.ReturnValue(0);
                        yield break;
                    }
                case ("java/util/concurrent/atomic/AtomicLong", "VMSupportsCS8", "()Z"):
                    {
                        JavaHelper.ReturnValue(1);
                        yield break;
                    }
                case ("java/util/TimeZone", "getSystemGMTOffsetID", "()Ljava/lang/String;"):
                    {
                        string offsetId = TimeZoneInfo.Local.BaseUtcOffset.ToString();
                        JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(offsetId));
                        yield break;
                    }
                case ("java/util/TimeZone", "getSystemTimeZoneID", "(Ljava/lang/String;)Ljava/lang/String;"):
                    {
                        string timeZoneId = TimeZoneInfo.Local.Id;
                        JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(timeZoneId));
                        yield break;
                    }
                case ("sun/io/Win32ErrorMode", "setErrorMode", "(J)J"):
                    {
                        JavaHelper.ReturnLargeValue(0L);
                        yield break;
                    }
                case ("sun/misc/Signal", "findSignal", "(Ljava/lang/String;)I"):
                    {
                        int signalNumber = -1;
                        switch (JavaHelper.ReadJavaString(args[0]))
                        {
                            case "INT":
                                signalNumber = 0;
                                break;
                            case "TERM":
                                signalNumber = 1;
                                break;
                            case "HUP":
                                signalNumber = 2;
                                break;
                        }
                        /*if(signalNumber != -1)
                        {
                            ClassFile signalClassFile = ClassFileManager.GetClassFile("sun/misc/Signal");
                            FieldReferenceValue signalsTableRef = (FieldReferenceValue)signalClassFile.StaticFieldsDictionary[("signals", "Ljava/util/Hashtable;")];
                            FieldReferenceValue handlersTableRef = (FieldReferenceValue)signalClassFile.StaticFieldsDictionary[("handlers", "Ljava/util/Hashtable;")];

                            ClassFile hashTableClassFile = ClassFileManager.GetClassFile("java/util/Hashtable");
                            MethodInfo putMethod = hashTableClassFile.MethodDictionary[("put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;")];

                            //RunJavaFunction(putMethod, signalsTableRef.Address, signalNumber, args[])

                        }*/
                        JavaHelper.ReturnValue(signalNumber);
                        yield break;
                    }
                case ("sun/misc/Signal", "handle0", "(IJ)J"):
                    {
                        JavaHelper.ReturnLargeValue(0L);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "addressSize", "()I"):
                    {
                        JavaHelper.ReturnValue(4);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "allocateMemory", "(J)J"):
                    {
                        long size = (args[1], args[2]).ToLong();
                        long address = Heap.AllocateMemory(size);
                        JavaHelper.ReturnLargeValue(address);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "arrayBaseOffset", "(Ljava/lang/Class;)I"):
                    {
                        //Returns offset of addr of first element from the addr of the array (bytes)
                        JavaHelper.ReturnValue(Heap.ArrayBaseOffset);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "arrayIndexScale", "(Ljava/lang/Class;)I"):
                    {
                        //returns num of bytes
                        HeapObject classObject = Heap.GetObject(args[1]);
                        string itemDescriptor = JavaHelper.ReadJavaString(classObject.GetField("name", "Ljava/lang/String;")).Substring(1);
                        if (itemDescriptor == "D" || itemDescriptor == "J")
                        {
                            JavaHelper.ReturnValue(8);
                        }
                        else JavaHelper.ReturnValue(4);

                        yield break;
                    }
                case ("sun/misc/Unsafe", "compareAndSwapObject", "(Ljava/lang/Object;JLjava/lang/Object;Ljava/lang/Object;)Z"):
                    {
                        // Sets first object to x if it currently equals excpected
                        // Returns true if set, false if not set

                        int objAddr = args[1];
                        long offset = Utility.ToLong((args[2], args[3]));
                        int expected = args[4];
                        int newVal = args[5];

                        int oldVal = Heap.GetInt(objAddr + (int)offset);
                        if (oldVal == expected)
                        {
                            Heap.PutInt(objAddr + (int)offset, newVal);
                            JavaHelper.ReturnValue(1);
                        }
                        else
                        {
                            JavaHelper.ReturnValue(0);
                        }
                        yield break;
                    }
                case ("sun/misc/Unsafe", "compareAndSwapInt", "(Ljava/lang/Object;JII)Z"):
                    {
                        int objAddr = args[1];
                        long offset = Utility.ToLong((args[2], args[3]));
                        int expected = args[4];
                        int newVal = args[5];

                        int oldVal = Heap.GetInt(objAddr + (int)offset);
                        if (oldVal == expected)
                        {
                            Heap.PutInt(objAddr + (int)offset, newVal);
                            JavaHelper.ReturnValue(1);
                        }
                        else
                        {
                            JavaHelper.ReturnValue(0);
                        }
                        yield break;
                    }
                case ("sun/misc/Unsafe", "compareAndSwapLong", "(Ljava/lang/Object;JJJ)Z"):
                    {
                        int objAddr = args[1];
                        long offset = Utility.ToLong((args[2], args[3]));
                        long expected = Utility.ToLong((args[4], args[5]));
                        long newVal = Utility.ToLong((args[6], args[7]));

                        long oldVal = Heap.GetLong(objAddr + (int)offset);
                        if (oldVal == expected)
                        {
                            Heap.PutLong(objAddr + (int)offset, newVal);
                            JavaHelper.ReturnValue(1);
                        }
                        else
                        {
                            JavaHelper.ReturnValue(0);
                        }
                        yield break;
                    }
                case ("sun/misc/Unsafe", "copyMemory", "(Ljava/lang/Object;JLjava/lang/Object;JJ)V"):
                    {
                        /* todo:
                         * The transfers are in coherent (atomic) units of a size determined
                         * by the address and length parameters.  If the effective addresses and
                         * length are all even modulo 8, the transfer takes place in 'long' units.
                         * If the effective addresses and length are (resp.) even modulo 4 or 2,
                         * the transfer takes place in units of 'int' or 'short'.
                         */
                        int srcObjAddr = args[1];
                        long srcOffset = Utility.ToLong((args[2], args[3]));
                        int destObjAddr = args[4];
                        long destOffset = Utility.ToLong((args[5], args[6]));
                        long numOfBytes = Utility.ToLong((args[7], args[8]));

                        Span<byte> srcSpan = Heap.GetSpan(srcObjAddr + (int)srcOffset, (int)numOfBytes);
                        Span<byte> destSpan = Heap.GetSpan(destObjAddr + (int)destOffset, (int)numOfBytes);
                        srcSpan.CopyTo(destSpan);

                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("sun/misc/Unsafe", "ensureClassInitialized", "(Ljava/lang/Class;)V"):
                    {
                        HeapObject cFileObj = Heap.GetObject(args[1]);
                        ClassFileManager.InitializeClass(JavaHelper.ReadJavaString(cFileObj.GetField(2)));
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("sun/misc/Unsafe", "freeMemory", "(J)V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("sun/misc/Unsafe", "getByte", "(J)B"):
                    {
                        long address = (args[1], args[2]).ToLong();
                        byte value = Heap.GetByte((int)address);
                        JavaHelper.ReturnValue(value);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "getIntVolatile", "(Ljava/lang/Object;J)I"):
                    {
                        int baseAddr = args[1];
                        long offset = Utility.ToLong((args[2], args[3]));
                        int val = Heap.GetInt(baseAddr + (int)offset);
                        JavaHelper.ReturnValue(val);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "getLongVolatile", "(Ljava/lang/Object;J)J"):
                    {
                        int baseAddr = args[1];
                        long offset = Utility.ToLong((args[2], args[3]));
                        long val = Heap.GetLong(baseAddr + (int)offset);
                        JavaHelper.ReturnLargeValue(val);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "getObjectVolatile", "(Ljava/lang/Object;J)Ljava/lang/Object;"):
                    {
                        //todo
                        //https://docs.oracle.com/javase/specs/jvms/se6/html/Threads.doc.html#22258
                        int baseAddr = args[1];
                        long offset = Utility.ToLong((args[2], args[3]));
                        int val = Heap.GetInt(baseAddr + (int)offset);
                        JavaHelper.ReturnValue(val);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "putObjectVolatile", "(Ljava/lang/Object;JLjava/lang/Object;)V"):
                    {
                        int baseAddr = args[1];
                        long offset = Utility.ToLong((args[2], args[3]));
                        int objToStoreAddr = args[4];
                        Heap.PutInt(baseAddr + (int)offset, objToStoreAddr);
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("sun/misc/Unsafe", "objectFieldOffset", "(Ljava/lang/reflect/Field;)J"):
                    {
                        HeapObject fieldObj = Heap.GetObject(args[1]);
                        //HeapObject classObj = Heap.GetObject(((FieldReferenceValue)fieldObj.GetField("clazz", "Ljava/lang/Class;")).Address);
                        int slot = fieldObj.GetField("slot", "I");
                        int offset = Heap.ObjectFieldOffset + Heap.ObjectFieldSize * slot;
                        JavaHelper.ReturnLargeValue(offset);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "pageSize", "()I"):
                    {
                        JavaHelper.ReturnValue(8);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "putLong", "(JJ)V"):
                    {
                        long address = (args[1], args[2]).ToLong();
                        long value = (args[3], args[4]).ToLong();
                        Heap.PutLong((int)address, value);
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("sun/misc/Unsafe", "registerNatives", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("sun/misc/Unsafe", "setMemory", "(Ljava/lang/Object;JJB)V"):
                    {
                        int objAddr = args[1];
                        long offset = Utility.ToLong((args[2], args[3]));
                        long length = Utility.ToLong((args[4], args[5]));
                        byte value = (byte)args[6];

                        Heap.GetSpan(objAddr + (int)offset, (int)length).Fill(value);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "staticFieldBase", "(Ljava/lang/reflect/Field;)Ljava/lang/Object;"):
                    {
                        JavaHelper.ReturnValue(0);
                        yield break;
                    }
                case ("sun/misc/Unsafe", "staticFieldOffset", "(Ljava/lang/reflect/Field;)J"):
                    {
                        HeapObject fieldObject = Heap.GetObject(args[1]);

                        int slot = fieldObject.GetField("slot", "I");

                        JavaHelper.ReturnLargeValue(Heap.ObjectFieldSize * slot);
                        yield break;
                    }
                case ("sun/misc/URLClassPath", "getLookupCacheURLs", "(Ljava/lang/ClassLoader;)[Ljava/net/URL;"):
                    {
                        JavaHelper.ReturnValue(0);
                        yield break;
                    }
                case ("sun/misc/VM", "initialize", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("sun/nio/ch/FileChannelImpl", "initIDs", "()J"):
                    {
                        JavaHelper.ReturnLargeValue(0L);
                        yield break;
                    }
                case ("sun/nio/ch/FileDispatcherImpl", "read0", "(Ljava/io/FileDescriptor;JI)I"):
                    {
                        int fileDescriptorAddr = args[0];
                        long address = (args[1], args[2]).ToLong(); //Memory address to write to
                        int length = args[3];

                        if (fileDescriptorAddr == 0)
                        {
                            yield return ThrowJavaException("java/io/IOException");
                            ActiveException = thisFrame.Stack[0];
                            yield break;
                        }
                        HeapObject fileDescriptor = Heap.GetObject(fileDescriptorAddr);

                        HeapObject parent = Heap.GetObject(fileDescriptor.GetField("parent", "Ljava/io/Closeable;"));
                        string path = JavaHelper.ReadJavaString(parent.GetField("path", "Ljava/lang/String;"));

                        if (FileStreams.AvailableBytes(path) == 0)
                        {
                            JavaHelper.ReturnValue(-1); //End of file
                            yield break;
                        }

                        int ret = FileStreams.ReadBytes(path, Heap.GetSpan((int)address, length));

                        JavaHelper.ReturnValue(ret);
                        yield break;
                    }
                case ("sun/nio/ch/IOUtil", "initIDs", "()V"):
                    {
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("sun/nio/ch/IOUtil", "iovMax", "()I"):
                    {
                        JavaHelper.ReturnValue(0);
                        yield break;
                    }
                case ("sun/reflect/NativeConstructorAccessorImpl", "newInstance0", "(Ljava/lang/reflect/Constructor;[Ljava/lang/Object;)Ljava/lang/Object;"):
                    {
                        int constructorAddr = args[0];
                        int argsArrAddr = args[1];

                        HeapObject constructorObj = Heap.GetObject(constructorAddr);

                        //Get args
                        ClassFile constructorClassFile = ClassFileManager.GetClassFile("java/lang/reflect/Constructor");
                        MethodInfo getDeclaringClassMethod = constructorClassFile.MethodDictionary[("getDeclaringClass", "()Ljava/lang/Class;")];
                        yield return RunJavaFunction(getDeclaringClassMethod, constructorAddr);
                        int declaringClassClassObjAddr = PopInt();
                        HeapObject declaringClassClassObj = Heap.GetObject(declaringClassClassObjAddr);

                        string declaringClassName = JavaHelper.ReadJavaString(declaringClassClassObj.GetField("name", "Ljava/lang/String;"));
                        int declaringClassCFileIdx = ClassFileManager.GetClassFileIndex(declaringClassName);
                        ClassFile declaringClass = ClassFileManager.ClassFiles[declaringClassCFileIdx];

                        //Get slot
                        MethodInfo getSlotMethod = constructorClassFile.MethodDictionary[("getSlot", "()I")];
                        yield return RunJavaFunction(getSlotMethod, constructorAddr);
                        int slot = PopInt();

                        //Find constructor
                        MethodInfo constructorMethod = null;
                        int i = 0;
                        foreach (MethodInfo method in declaringClass.MethodDictionary.Values)
                        {
                            if (i == slot)
                            {
                                constructorMethod = method;
                                break;
                            }
                            i++;
                        }

                        //Create object
                        int newObjectAddr = Heap.CreateObject(declaringClassCFileIdx);

                        //Copy arguments to arguments array
                        int[] arguments;
                        if (argsArrAddr == 0)
                        {
                            arguments = new int[] { newObjectAddr };
                        }
                        else
                        {
                            HeapArray argsArr = Heap.GetArray(argsArrAddr);
                            arguments = new int[argsArr.Length + 1];
                            arguments[0] = newObjectAddr;
                            Span<int> argsArrData = MemoryMarshal.Cast<byte, int>(argsArr.GetDataSpan());
                            argsArrData.CopyTo(arguments.AsSpan(1));
                        }

                        //Run constructor
                        yield return RunJavaFunction(constructorMethod, arguments);
                        JavaHelper.ReturnValue(newObjectAddr);

                        yield break;
                    }
                case ("sun/reflect/Reflection", "getCallerClass", "()Ljava/lang/Class;"):
                    {
                        MethodFrame frame = MethodFrameStack.ToArray()[2];
                        ClassFile callerClass = frame.Method.ClassFile;

                        int classObjAddr = ClassObjectManager.GetClassObjectAddr(callerClass.Name);

                        JavaHelper.ReturnValue(classObjAddr);
                        yield break;
                    }
                case ("sun/reflect/Reflection", "getClassAccessFlags", "(Ljava/lang/Class;)I"):
                    {
                        HeapObject classObj = Heap.GetObject(args[0]);
                        string cFileName = JavaHelper.ReadJavaString(classObj.GetField("name", "Ljava/lang/String;"));
                        ClassFile cFile = ClassFileManager.GetClassFile(cFileName);
                        JavaHelper.ReturnValue(cFile.AccessFlags);
                        yield break;
                    }
                default:
                    throw new MissingMethodException($"className == \"{className}\" && nameAndDescriptor == (\"{thisFuncName}\", \"{thisDescriptor}\")");
            }
        }

        public static MethodInfo? InterpretUntilCallOrRet()
        {
            MethodFrame thisFrame = MethodFrameStack.Peek();
            ReadOnlyMemory<byte> code = thisFrame.Method.CodeAttribute.Code;
            Span<int> locals = Stack.Slice(thisFrame.BaseOffset, thisFrame.Method.MaxLocals).Span;

            int[] args = Stack.Span.Slice(thisFrame.BaseOffset, thisFrame.Method.MaxLocals).ToArray();

            int PopInt() => thisFrame.Stack[--thisFrame.SP];
            void Push(int val) => thisFrame.Stack[thisFrame.SP++] = val;

            byte readByte() => code.Span[thisFrame.IP++];

            short readShort()
            {
                byte chomp = code.Span[thisFrame.IP++]; //Larger byte
                byte nibble = code.Span[thisFrame.IP++]; //Smaller byte
                return (short)((chomp << 8) | nibble);
            }

            int readInt()
            {
                byte byte0 = code.Span[thisFrame.IP++];
                byte byte1 = code.Span[thisFrame.IP++];
                byte byte2 = code.Span[thisFrame.IP++];
                byte byte3 = code.Span[thisFrame.IP++];
                return ((byte0 << 24) | (byte1 << 16) | (byte2 << 8) | byte3);
            }

            while (true)
            {
                if (ActiveException != 0)
                {
                    throw new NotImplementedException();
                }

                int exception = 0;

                //TODO: Fix system.out.println
                int oldIp = thisFrame.IP;
                byte opCode = code.Span[thisFrame.IP++];
                bool wide = opCode == (int)OpCodes.wide;
                if (wide)
                {
                    opCode = code.Span[thisFrame.IP++];
                }
                switch ((OpCodes)opCode)
                {
                    case OpCodes.nop:
                        break;
                    case OpCodes.aconst_null:
                        Push(0);
                        break;
                    case OpCodes.iconst_m1:
                    case OpCodes.iconst_0:
                    case OpCodes.iconst_1:
                    case OpCodes.iconst_2:
                    case OpCodes.iconst_3:
                    case OpCodes.iconst_4:
                    case OpCodes.iconst_5:
                        Push(opCode - 0x03);
                        break;
                    case OpCodes.lconst_0:
                        Push(0);
                        Push(0);
                        break;
                    case OpCodes.lconst_1:
                        Push(0);
                        Push(1);
                        break;
                    case OpCodes.fconst_0:
                        Push(0);
                        break;
                    case OpCodes.fconst_1:
                        Push(0x3f800000);
                        break;
                    case OpCodes.fconst_2:
                        Push(0x40000000);
                        break;
                    case OpCodes.dconst_0:
                        Push(0);
                        Push(0);
                        break;
                    case OpCodes.dconst_1:
                        Push(0x3ff00000);
                        Push(0);
                        break;
                    case OpCodes.bipush:
                        Push((sbyte)readByte());
                        break;
                    case OpCodes.sipush:
                        Push((short)((readByte() << 8) | readByte()));
                        break;
                    case OpCodes.ldc:
                        {
                            CPInfo value = thisFrame.Method.ClassFile.Constants[readByte()];

                            if (value.GetType() == typeof(CIntegerInfo))
                            {
                                Push(((CIntegerInfo)value).IntValue);
                            }
                            else if (value.GetType() == typeof(CFloatInfo))
                            {
                                Push((int)(((CFloatInfo)value).IntValue));
                            }
                            else if (value.GetType() == typeof(CStringInfo))
                            {
                                string @string = ((CStringInfo)value).String;
                                Push(JavaHelper.CreateJavaStringLiteral(@string));
                            }
                            else if (value.GetType() == typeof(CClassInfo))
                            {
                                Push(ClassObjectManager.GetClassObjectAddr(((CClassInfo)value).Name));
                            }
                            else throw new NotImplementedException("Not supported");
                        }
                        break;
                    case OpCodes.ldc_w:
                        {
                            short index = readShort();
                            CPInfo value = thisFrame.Method.ClassFile.Constants[index];

                            if (value.GetType() == typeof(CIntegerInfo))
                            {
                                Push(((CIntegerInfo)value).IntValue);
                            }
                            else if (value.GetType() == typeof(CFloatInfo))
                            {
                                Push(Stack[sp++] = (int)((CFloatInfo)value).IntValue);
                            }
                            else if (value.GetType() == typeof(CStringInfo))
                            {
                                string @string = ((CStringInfo)value).String;
                                Push(JavaHelper.CreateJavaStringLiteral(@string));
                            }
                            else if (value.GetType() == typeof(CClassInfo))
                            {
                                Push(ClassObjectManager.GetClassObjectAddr(((CClassInfo)value).Name));
                            }
                            else throw new NotImplementedException("Not supported");
                        }
                        break;
                    case OpCodes.ldc2_w:
                        {
                            short index = readShort();
                            CPInfo value = thisFrame.Method.ClassFile.Constants[index];
                            if (value.GetType() == typeof(CLongInfo))
                            {
                                Push(((CLongInfo)value).LongValue);
                            }
                            else if (value.GetType() == typeof(CDoubleInfo))
                            {
                                Push(Stack[sp++] = (int)((CDoubleInfo)value).LongValue);
                            }
                            else throw new NotImplementedException("Not supported");
                        }
                        break;
                    case OpCodes.iload:
                    case OpCodes.fload:
                    case OpCodes.aload:
                        if (wide)
                        {
                            Push(locals[readShort()]);
                        }
                        else
                        {
                            Push(locals[readByte()]);
                        }
                        break;
                    case OpCodes.lload:
                    case OpCodes.dload:
                        {
                            short index = wide ? readShort() : readByte();
                            int high = locals[index];
                            int low = locals[index + 1];
                            Push(high);
                            Push(low);
                        }
                        break;
                    case OpCodes.iload_0:
                    case OpCodes.iload_1:
                    case OpCodes.iload_2:
                    case OpCodes.iload_3:
                        Push(locals[opCode - 0x1A]);
                        break;
                    case OpCodes.lload_0:
                    case OpCodes.lload_1:
                    case OpCodes.lload_2:
                    case OpCodes.lload_3:
                        {
                            int index = opCode - 0x1e;
                            int high = locals[index];
                            int low = locals[index + 1];
                            Push(high);
                            Push(low);
                            break;
                        }
                    case OpCodes.fload_0:
                    case OpCodes.fload_1:
                    case OpCodes.fload_2:
                    case OpCodes.fload_3:
                        Push(locals[opCode - 0x22]);  //Floats already stored as int
                        break;
                    case OpCodes.dload_0:
                    case OpCodes.dload_1:
                    case OpCodes.dload_2:
                    case OpCodes.dload_3:
                        {
                            int index = opCode - 0x26;
                            int high = locals[index];
                            int low = locals[index + 1];
                            Push(high); //Doubles already stored as long
                            Push(low);
                            break;
                        }
                    case OpCodes.aload_0:
                    case OpCodes.aload_1:
                    case OpCodes.aload_2:
                    case OpCodes.aload_3:
                        Push(locals[opCode - 0x2A]);  //References already stored as int
                        break;
                    case OpCodes.iaload:
                    case OpCodes.faload:
                    case OpCodes.aaload:
                        {
                            int index = PopInt();
                            int arrayRef = PopInt();
                            if (arrayRef == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            HeapArray array = Heap.GetArray(arrayRef);
                            Push(array.GetItem(index));
                        }
                        break;
                    case OpCodes.baload:
                        {
                            int index = PopInt();
                            int arrayRef = PopInt();
                            if (arrayRef == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            HeapArray array = Heap.GetArray(arrayRef);
                            Push(array.GetItemByte(index));
                        }
                        break;
                    case OpCodes.caload:
                    case OpCodes.saload:
                        {
                            int index = PopInt();
                            int arrayRef = PopInt();
                            if (arrayRef == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            HeapArray array = Heap.GetArray(arrayRef);
                            Push(array.GetItemShort(index));
                        }
                        break;
                    case OpCodes.laload:
                    case OpCodes.daload:
                        {
                            int index = PopInt();
                            int arrayRef = PopInt();
                            if (arrayRef == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            HeapArray array = Heap.GetArray(arrayRef);
                            Push(array.GetItemLong(index));
                        }
                        break;
                    case OpCodes.istore:
                    case OpCodes.fstore:
                    case OpCodes.astore:
                        {
                            int index = wide ? readShort() : readByte();
                            int value = PopInt();
                            locals[index] = value;
                        }
                        break;
                    case OpCodes.lstore:
                    case OpCodes.dstore:
                        {
                            int index = wide ? readShort() : readByte();
                            (int high, int low) = Utility.PopLong(Stack, ref sp).Split();
                            locals[index] = high;
                            locals[index + 1] = low;
                        }
                        break;
                    case OpCodes.istore_0:
                    case OpCodes.istore_1:
                    case OpCodes.istore_2:
                    case OpCodes.istore_3:
                        locals[opCode - 0x3B] = Stack[--sp];
                        break;
                    case OpCodes.lstore_0:
                    case OpCodes.lstore_1:
                    case OpCodes.lstore_2:
                    case OpCodes.lstore_3:
                        {
                            (int high, int low) = Utility.PopLong(Stack, ref sp).Split();
                            locals[opCode - 0x3F] = high;
                            locals[opCode - 0x3E] = low;
                        }
                        break;
                    case OpCodes.fstore_0:
                    case OpCodes.fstore_1:
                    case OpCodes.fstore_2:
                    case OpCodes.fstore_3:
                        locals[opCode - 0x43] = PopInt();
                        break;
                    case OpCodes.dstore_0:
                    case OpCodes.dstore_1:
                    case OpCodes.dstore_2:
                    case OpCodes.dstore_3:
                        {
                            (int high, int low) = Utility.PopLong(Stack, ref sp).Split();
                            locals[opCode - 0x47] = high;
                            locals[opCode - 0x46] = low;
                        }
                        break;
                    case OpCodes.astore_0:
                    case OpCodes.astore_1:
                    case OpCodes.astore_2:
                    case OpCodes.astore_3:
                        locals[opCode - 0x4B] = PopInt();
                        break;
                    case OpCodes.iastore:
                    case OpCodes.fastore:
                    case OpCodes.aastore:
                        {
                            int value = PopInt();
                            int index = PopInt();
                            int arrayRef = PopInt();
                            if (arrayRef == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            HeapArray array = Heap.GetArray(arrayRef);
                            array.SetItem(index, value);
                        }
                        break;
                    case OpCodes.bastore:
                        {
                            int value = PopInt();
                            int index = PopInt();
                            int arrayRef = PopInt();
                            if (arrayRef == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            HeapArray array = Heap.GetArray(arrayRef);
                            array.SetItem(index, (byte)value);
                        }
                        break;
                    case OpCodes.castore:
                    case OpCodes.sastore:
                        {
                            int value = PopInt();
                            int index = PopInt();
                            int arrayRef = PopInt();
                            if (arrayRef == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            HeapArray array = Heap.GetArray(arrayRef);
                            array.SetItem(index, (short)value);
                        }
                        break;
                    case OpCodes.lastore:
                    case OpCodes.dastore:
                        {
                            long value = Utility.PopLong(Stack, ref sp);
                            int index = PopInt();
                            int arrayRef = PopInt();
                            if (arrayRef == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            HeapArray array = Heap.GetArray(arrayRef);
                            array.SetItem(index, value);
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
                        Push(Utility.PeekInt(Stack, sp));
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
                        Push(Utility.PeekInt(Stack, sp, 1));
                        Push(Utility.PeekInt(Stack, sp, 1));
                        break;
                    case OpCodes.dup2_x1:
                        Push(Utility.PeekInt(Stack, sp, 1));
                        Push(Utility.PeekInt(Stack, sp, 1));
                        Stack[sp - 3] = Stack[sp - 5];
                        Stack[sp - 4] = Stack[sp - 1];
                        Stack[sp - 5] = Stack[sp - 2];
                        break;
                    case OpCodes.dup2_x2:
                        Push(Utility.PeekInt(Stack, sp, 1));
                        Push(Utility.PeekInt(Stack, sp, 1));
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
                            int second = PopInt();
                            int first = PopInt();
                            Push(first + second);
                        }
                        break;
                    case OpCodes.ladd:
                        {
                            long second = Utility.PopLong(Stack, ref sp);
                            long first = Utility.PopLong(Stack, ref sp);
                            Push(first + second);
                        }
                        break;
                    case OpCodes.fadd:
                        {
                            float second = Utility.PopFloat(Stack, ref sp);
                            float first = Utility.PopFloat(Stack, ref sp);
                            Push(first + second);
                        }
                        break;
                    case OpCodes.dadd:
                        {
                            double second = Utility.PopDouble(Stack, ref sp);
                            double first = Utility.PopDouble(Stack, ref sp);
                            Push(first + second);
                        }
                        break;
                    case OpCodes.isub:
                        {
                            int second = PopInt();
                            int first = PopInt();
                            Push(first - second);
                        }
                        break;
                    case OpCodes.lsub:
                        {
                            long second = Utility.PopLong(Stack, ref sp);
                            long first = Utility.PopLong(Stack, ref sp);
                            Push(first - second);
                        }
                        break;
                    case OpCodes.fsub:
                        {
                            float second = Utility.PopFloat(Stack, ref sp);
                            float first = Utility.PopFloat(Stack, ref sp);
                            Push(first - second);
                        }
                        break;
                    case OpCodes.dsub:
                        {
                            double second = Utility.PopDouble(Stack, ref sp);
                            double first = Utility.PopDouble(Stack, ref sp);
                            Push(first - second);
                        }
                        break;
                    case OpCodes.imul:
                        {
                            int second = PopInt();
                            int first = PopInt();
                            Push(first * second);
                        }
                        break;
                    case OpCodes.lmul:
                        {
                            long second = Utility.PopLong(Stack, ref sp);
                            long first = Utility.PopLong(Stack, ref sp);
                            Push(first * second);
                        }
                        break;
                    case OpCodes.fmul:
                        {
                            float second = Utility.PopFloat(Stack, ref sp);
                            float first = Utility.PopFloat(Stack, ref sp);
                            Push(first * second);
                        }
                        break;
                    case OpCodes.dmul:
                        {
                            double second = Utility.PopDouble(Stack, ref sp);
                            double first = Utility.PopDouble(Stack, ref sp);
                            Push(first * second);
                        }
                        break;
                    case OpCodes.idiv:
                        {
                            int second = PopInt();
                            int first = PopInt();
                            if (second == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/ArithmeticException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            Push(first / second);
                        }
                        break;
                    case OpCodes.ldiv:
                        {
                            long second = Utility.PopLong(Stack, ref sp);
                            long first = Utility.PopLong(Stack, ref sp);
                            if (second == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/ArithmeticException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            Push(first / second);
                        }
                        break;
                    case OpCodes.fdiv:
                        {
                            float second = Utility.PopFloat(Stack, ref sp);
                            float first = Utility.PopFloat(Stack, ref sp);
                            if (second == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/ArithmeticException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            Push(first / second);
                        }
                        break;
                    case OpCodes.ddiv:
                        {
                            double second = Utility.PopDouble(Stack, ref sp);
                            double first = Utility.PopDouble(Stack, ref sp);
                            if (second == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/ArithmeticException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            Push(first / second);
                        }
                        break;
                    case OpCodes.irem:
                        {
                            int second = PopInt();
                            int first = PopInt();
                            if (second == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/ArithmeticException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            Push(first % second);
                        }
                        break;
                    case OpCodes.lrem:
                        {
                            long second = Utility.PopLong(Stack, ref sp);
                            long first = Utility.PopLong(Stack, ref sp);
                            if (second == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/ArithmeticException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            else
                            {
                                Push(first % second);
                            }
                        }
                        break;
                    case OpCodes.frem:
                        {
                            float second = Utility.PopFloat(Stack, ref sp);
                            float first = Utility.PopFloat(Stack, ref sp);
                            Push(first % second);
                        }
                        break;
                    case OpCodes.drem:
                        {
                            double second = Utility.PopDouble(Stack, ref sp);
                            double first = Utility.PopDouble(Stack, ref sp);
                            Push(first % second);
                        }
                        break;
                    case OpCodes.ineg: //arithmetic negation, not bitwise
                        Stack[sp - 1] = -Stack[sp - 1];
                        break;
                    case OpCodes.lneg:
                        Push(-Utility.PopLong(Stack, ref sp));
                        break;
                    case OpCodes.fneg:
                        Push(-Utility.PopFloat(Stack, ref sp));
                        break;
                    case OpCodes.dneg:
                        Push(-Utility.PopDouble(Stack, ref sp));
                        break;
                    #endregion
                    #region Bitwise Op-Codes
                    case OpCodes.ishl:
                        {
                            int shiftBy = PopInt();
                            int num = PopInt();
                            Push(num << shiftBy);
                        }
                        break;
                    case OpCodes.lshl:
                        {
                            int shiftBy = PopInt();
                            long num = Utility.PopLong(Stack, ref sp);
                            Push(num << shiftBy);
                        }
                        break;
                    case OpCodes.ishr:
                        {
                            int shiftBy = PopInt();
                            int num = PopInt();
                            Push(num >> shiftBy);
                        }
                        break;
                    case OpCodes.lshr:
                        {
                            int shiftBy = PopInt();
                            long num = Utility.PopLong(Stack, ref sp);
                            Push(num >> shiftBy);
                        }
                        break;
                    case OpCodes.iushr:
                        {
                            int shiftBy = PopInt();
                            int num = PopInt();
                            Push((int)((uint)num >> shiftBy));
                        }
                        break;
                    case OpCodes.lushr:
                        {
                            int shiftBy = PopInt();
                            long num = Utility.PopLong(Stack, ref sp);
                            Push((long)((ulong)num >> shiftBy));
                        }
                        break;
                    case OpCodes.iand:
                        {
                            int second = PopInt();
                            int first = PopInt();
                            Push(first & second);
                        }
                        break;
                    case OpCodes.land:
                        {
                            long second = Utility.PopLong(Stack, ref sp);
                            long first = Utility.PopLong(Stack, ref sp);
                            Push(first & second);
                        }
                        break;
                    case OpCodes.ior:
                        {
                            int second = PopInt();
                            int first = PopInt();
                            Push(first | second);
                        }
                        break;
                    case OpCodes.lor:
                        {
                            long second = Utility.PopLong(Stack, ref sp);
                            long first = Utility.PopLong(Stack, ref sp);
                            Push(first | second);
                        }
                        break;
                    case OpCodes.ixor:
                        {
                            int second = PopInt();
                            int first = PopInt();
                            Push(first ^ second);
                        }
                        break;
                    case OpCodes.lxor:
                        {
                            long second = Utility.PopLong(Stack, ref sp);
                            long first = Utility.PopLong(Stack, ref sp);
                            Push(first ^ second);
                        }
                        break;
                    case OpCodes.iinc:
                        {
                            short index = wide ? readShort() : readByte();
                            sbyte incrementBy = (sbyte)readByte();
                            locals[index] += incrementBy;
                        }
                        break;
                    #endregion
                    #region Stack Casting Op-Codes
                    case OpCodes.i2l:
                        Push((long)PopInt());
                        break;
                    case OpCodes.i2f:
                        Push((float)PopInt());
                        break;
                    case OpCodes.i2d:
                        Push((double)PopInt());
                        break;
                    case OpCodes.l2i:
                        Push((int)Utility.PopLong(Stack, ref sp));
                        break;
                    case OpCodes.l2f:
                        Push((float)Utility.PopLong(Stack, ref sp));
                        break;
                    case OpCodes.l2d:
                        Push((double)Utility.PopLong(Stack, ref sp));
                        break;
                    case OpCodes.f2i:
                        Push((int)Utility.PopFloat(Stack, ref sp));
                        break;
                    case OpCodes.f2l:
                        Push((long)Utility.PopFloat(Stack, ref sp));
                        break;
                    case OpCodes.f2d:
                        Push((double)Utility.PopFloat(Stack, ref sp));
                        break;
                    case OpCodes.d2i:
                        Push((int)Utility.PopDouble(Stack, ref sp));
                        break;
                    case OpCodes.d2l:
                        Push((long)Utility.PopDouble(Stack, ref sp));
                        break;
                    case OpCodes.d2f:
                        Push((float)Utility.PopDouble(Stack, ref sp));
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
                                Push(0);
                            }
                            else if (value1 > value2)
                            {
                                Push(1);
                            }
                            else
                            {
                                Push(-1);
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
                                    Push(-1);
                                }
                                else
                                {
                                    Push(1);
                                }
                            }
                            else
                            {
                                if (value1 > value2)
                                {
                                    Push(1);
                                }
                                else if (value1 == value2)
                                {
                                    Push(0);
                                }
                                else if (value1 < value2)
                                {
                                    Push(-1);
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
                                    Push(-1);
                                }
                                else
                                {
                                    Push(1);
                                }
                            }
                            else
                            {
                                if (value1 > value2)
                                {
                                    Push(1);
                                }
                                else if (value1 == value2)
                                {
                                    Push(0);
                                }
                                else if (value1 < value2)
                                {
                                    Push(-1);
                                }
                            }
                            break;
                        }
                    #endregion
                    #region Jump Op-Codes
                    case OpCodes.ifeq:
                        {
                            short offset = readShort();
                            int value = PopInt();
                            if (value == 0) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.ifne:
                        {
                            short offset = readShort();
                            int value = PopInt();
                            if (value != 0) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.iflt:
                        {
                            short offset = readShort();
                            int value = PopInt();
                            if (value < 0) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.ifge:
                        {
                            short offset = readShort();
                            int value = PopInt();
                            if (value >= 0) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.ifgt:
                        {
                            short offset = readShort();
                            int value = PopInt();
                            if (value > 0) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.ifle:
                        {
                            short offset = readShort();
                            int value = PopInt();
                            if (value <= 0) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.if_icmpeq:
                    case OpCodes.if_acmpeq:
                        {
                            short offset = readShort();
                            int value2 = PopInt();
                            int value1 = PopInt();
                            if (value1 == value2) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.if_icmpne:
                    case OpCodes.if_acmpne:
                        {
                            short offset = readShort();
                            int value2 = PopInt();
                            int value1 = PopInt();
                            if (value1 != value2) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.if_icmplt:
                        {
                            short offset = readShort();
                            int value2 = PopInt();
                            int value1 = PopInt();
                            if (value1 < value2) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.if_icmpge:
                        {
                            short offset = readShort();
                            int value2 = PopInt();
                            int value1 = PopInt();
                            if (value1 >= value2) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.if_icmpgt:
                        {
                            short offset = readShort();
                            int value2 = PopInt();
                            int value1 = PopInt();
                            if (value1 > value2) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.if_icmple:
                        {
                            short offset = readShort();
                            int value2 = PopInt();
                            int value1 = PopInt();
                            if (value1 <= value2) ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.@goto:
                        {
                            short offset = readShort();
                            ip = oldIp + offset;
                        }
                        break;
                    #endregion
                    case OpCodes.jsr:
                        {
                            short offset = readShort();
                            int retAddress = ip;
                            Push(retAddress);
                            ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.ret:
                        {
                            short index = wide ? readShort() : readByte();
                            int retAddress = locals[index];
                            ip = retAddress;
                        }
                        break;
                    case OpCodes.tableswitch:
                        {
                            int index = PopInt();
                            ip += (3 - ((ip - 1) % 4)); //Moves to next multiple of 4

                            int defaultOffset = readInt(); //What to do w/ this?

                            int low = readInt();
                            int high = readInt();

                            if (low > high) throw new InvalidDataException();

                            int numOfPairs = high - low + 1;

                            if (index < low || index > high)
                            {
                                ip = oldIp + defaultOffset;
                            }
                            else
                            {
                                ip += 4 * (index - low);
                                int offset = readInt();
                                ip = oldIp + offset;
                            }
                            break;
                        }
                    case OpCodes.lookupswitch:
                        {
                            int searchKey = PopInt();
                            ip += (3 - ((ip - 1) % 4)); //Moves to next multiple of 4

                            int defaultOffset = readInt();

                            int numOfPairs = readInt();

                            bool foundMatch = false;
                            for (int i = 0; i < numOfPairs; i++)
                            {
                                int key = readInt();
                                int offset = readInt();
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
                        JavaHelper.ReturnValue(PopInt());
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
                            short index = readShort();
                            CFieldRefInfo fieldRef = (CFieldRefInfo)ClassFile.Constants[index];
                            ClassFile cFile = ClassFileManager.GetClassFile(fieldRef.ClassName);
                            ClassFileManager.InitializeClass(fieldRef.ClassName);

                            //todo: superinterface? https://docs.oracle.com/javase/specs/jvms/se7/html/jvms-5.html#jvms-5.4.3.2

                            long fieldValue;
                            while (!cFile.StaticFieldsDictionary.TryGetValue((fieldRef.Name, fieldRef.Descriptor), out fieldValue))
                            {
                                cFile = cFile.SuperClass;
                            }

                            /*if (fieldRef.ClassName == "java/lang/ref/SoftReference" && fieldRef.Name == "clock" && fieldRef.Descriptor == "J")
                            {
                                Push(DateTime.Now.Ticks);
                            }*/
                            if (fieldRef.Descriptor == "J" || fieldRef.Descriptor == "D")
                            {
                                Push(fieldValue);
                            }
                            else
                            {
                                Push((int)fieldValue);
                            }
                        }
                        break;
                    case OpCodes.putstatic:
                        {
                            short index = readShort();
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
                                        int value = PopInt();
                                        cFile.StaticFieldsDictionary[(fieldRef.Name, fieldRef.Descriptor)] = value;
                                    }
                                    break;
                                case 'D':
                                case 'J':
                                    {
                                        long value = Utility.PopLong(Stack, ref sp);
                                        cFile.StaticFieldsDictionary[(fieldRef.Name, fieldRef.Descriptor)] = value;
                                    }
                                    break;
                                case 'L':
                                case '[':
                                    {
                                        int valueRef = PopInt();
                                        cFile.StaticFieldsDictionary[(fieldRef.Name, fieldRef.Descriptor)] = valueRef;
                                    }
                                    break;
                            }
                        }
                        break;
                    case OpCodes.getfield:
                        {
                            short index = readShort();
                            int objectRef = PopInt();

                            if (objectRef == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }

                            CFieldRefInfo fieldRef = (CFieldRefInfo)ClassFile.Constants[index];
                            HeapObject heapObj = Heap.GetObject(objectRef);
                            if (fieldRef.Descriptor == "J" || fieldRef.Descriptor == "D")
                            {
                                long fieldValue = heapObj.GetFieldLong(fieldRef.Name, fieldRef.Descriptor);
                                Push(fieldValue);
                            }
                            else
                            {
                                int fieldValue = heapObj.GetField(fieldRef.Name, fieldRef.Descriptor);
                                Push(fieldValue);
                            }
                        }
                        break;
                    case OpCodes.putfield:
                        {
                            short index = readShort();
                            CFieldRefInfo fieldRef = (CFieldRefInfo)ClassFile.Constants[index];
                            switch (fieldRef.Descriptor[0])
                            {
                                case 'Z':
                                case 'B':
                                case 'C':
                                case 'S':
                                case 'I':
                                case 'F':
                                case 'L':
                                case '[':
                                    {
                                        int value = PopInt();
                                        int objectRef = PopInt();
                                        HeapObject heapObj = Heap.GetObject(objectRef);
                                        heapObj.SetField(fieldRef.Name, fieldRef.Descriptor, value);
                                    }
                                    break;
                                case 'D':
                                case 'J':
                                    {
                                        long value = Utility.PopLong(Stack, ref sp);
                                        int objectRef = PopInt();
                                        HeapObject heapObj = Heap.GetObject(objectRef);
                                        heapObj.SetFieldLong(fieldRef.Name, fieldRef.Descriptor, value);
                                    }
                                    break;
                            }
                        }
                        break;
                    case OpCodes.invokevirtual:
                    case OpCodes.invokespecial:
                        {
                            MethodInfo method;
                            int[] arguments;
                            //Get method ref
                            short index = readShort();
                            CMethodRefInfo methodRef = (CMethodRefInfo)ClassFile.Constants[index];

                            //Get args
                            arguments = new int[methodRef.NumOfArgs() + 1];
                            for (int i = arguments.Length - 1; i >= 0; i--)
                            {
                                arguments[i] = PopInt();
                            }

                            if (arguments[0] == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }

                            ClassFile cFile;
                            if ((OpCodes)opCode == OpCodes.invokevirtual)
                            {
                                string objectRefClassFileName = Heap.GetObject(arguments[0]).ClassFile.Name;
                                cFile = ClassFileManager.GetClassFile(objectRefClassFileName);
                            }
                            else
                            {
                                CClassInfo cFileInfo = (CClassInfo)ClassFile.Constants[methodRef.ClassIndex];
                                cFile = ClassFileManager.GetClassFile(cFileInfo.Name);
                            }
                            method = JavaHelper.ResolveMethod(cFile.Name, methodRef.Name, methodRef.Descriptor);

                            if (method.HasFlag(MethodInfoFlag.Native))
                            {
                                Program.StackTracePrinter.PrintMethodCall(method, arguments);
                                NativeMethodFrame nativeMethodFrame = new NativeMethodFrame(method)
                                {
                                    Args = arguments
                                };
                                nativeMethodFrame.Execute();
                            }
                            else
                            {
                                Program.StackTracePrinter.PrintMethodCall(method, arguments);
                                MethodFrame methodFrame = new MethodFrame(method);
                                arguments.CopyTo(methodFrame.Locals, 0);
                                methodFrame.Execute();
                            }

                        }
                        break;
                    case OpCodes.invokestatic:
                        {
                            short index = readShort();

                            CMethodRefInfo methodRef = (CMethodRefInfo)ClassFile.Constants[index];

                            ClassFile cFile = ClassFileManager.GetClassFile(methodRef.ClassName);
                            ClassFileManager.InitializeClass(methodRef.ClassName);

                            int[] arguments = new int[methodRef.NumOfArgs()];


                            for (int i = arguments.Length - 1; i >= 0; i--)
                            {
                                arguments[i] = PopInt();
                            }

                            //Search for method in cFile's staticMethodDictionary. If it's not there, repeat search in cFile's super and so on
                            MethodInfo method;
                            while (!cFile.MethodDictionary.TryGetValue((methodRef.Name, methodRef.Descriptor), out method))
                            {
                                cFile = cFile.SuperClass;
                            }

                            if (!method.HasFlag(MethodInfoFlag.Native))
                            {
                                Program.StackTracePrinter.PrintMethodCall(method, arguments);
                                MethodFrame methodFrame = new MethodFrame(method);
                                arguments.CopyTo(methodFrame.Locals, 0);
                                methodFrame.Execute();
                            }
                            else
                            {
                                Program.StackTracePrinter.PrintMethodCall(method, arguments);
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
                            short index = readShort();
                            CInterfaceMethodRefInfo interfaceMethodRef = (CInterfaceMethodRefInfo)ClassFile.Constants[index];

                            byte count = readByte();
                            if (count == 0) throw new InvalidOperationException();

                            ip++; //Skip the zero

                            int[] arguments = new int[interfaceMethodRef.NumOfArgs() + 1];
                            for (int i = arguments.Length - 1; i >= 0; i--)
                            {
                                arguments[i] = PopInt();
                            }

                            if (arguments[0] == 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }

                            ClassFile cFile = Heap.GetObject(arguments[0]).ClassFile;

                            //Search for method in cFile's methodDictionary. If it's not there, repeat search in cFile's super and so on
                            MethodInfo method;
                            while (!cFile.MethodDictionary.TryGetValue((interfaceMethodRef.Name, interfaceMethodRef.Descriptor), out method))
                            {
                                cFile = cFile.SuperClass;
                                if (cFile == null)
                                {
                                    foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/AbstractMethodError"))
                                    {
                                        if (e == 0)
                                        {
                                            yield return e;
                                        }
                                        else
                                        {
                                            exception = e;
                                        }
                                    }
                                    break;
                                }
                            }

                            if (!method.HasFlag(MethodInfoFlag.Native))
                            {
                                Program.StackTracePrinter.PrintMethodCall(method, arguments, interfaceMethodRef);
                                MethodFrame methodFrame = new MethodFrame(method);
                                arguments.CopyTo(methodFrame.Locals, 0);
                                methodFrame.Execute();
                            }
                            else
                            {
                                Program.StackTracePrinter.PrintMethodCall(method, arguments, interfaceMethodRef);
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
                            short index = readShort();
                            CClassInfo classInfo = (CClassInfo)ClassFile.Constants[index];
                            ClassFileManager.InitializeClass(classInfo.Name);

                            int cFileIdx = ClassFileManager.GetClassFileIndex(classInfo.Name);

                            Push(Heap.CreateObject(cFileIdx));
                        }
                        break;
                    case OpCodes.newarray:
                        {
                            byte aType = readByte();
                            int count = PopInt();
                            if (count < 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NegativeArraySizeException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }
                            switch ((ArrayTypeCodes)aType)
                            {
                                case ArrayTypeCodes.T_BOOLEAN:
                                    Push(Heap.CreateArray(1, count, ClassObjectManager.GetClassObjectAddr("boolean")));
                                    break;
                                case ArrayTypeCodes.T_CHAR:
                                    Push(Heap.CreateArray(2, count, ClassObjectManager.GetClassObjectAddr("char")));
                                    break;
                                case ArrayTypeCodes.T_FLOAT:
                                    Push(Heap.CreateArray(4, count, ClassObjectManager.GetClassObjectAddr("float")));
                                    break;
                                case ArrayTypeCodes.T_DOUBLE:
                                    Push(Heap.CreateArray(8, count, ClassObjectManager.GetClassObjectAddr("double")));
                                    break;
                                case ArrayTypeCodes.T_BYTE:
                                    Push(Heap.CreateArray(1, count, ClassObjectManager.GetClassObjectAddr("byte")));
                                    break;
                                case ArrayTypeCodes.T_SHORT:
                                    Push(Heap.CreateArray(2, count, ClassObjectManager.GetClassObjectAddr("short")));
                                    break;
                                case ArrayTypeCodes.T_INT:
                                    Push(Heap.CreateArray(4, count, ClassObjectManager.GetClassObjectAddr("int")));
                                    break;
                                case ArrayTypeCodes.T_LONG:
                                    Push(Heap.CreateArray(8, count, ClassObjectManager.GetClassObjectAddr("long")));
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        break;
                    case OpCodes.anewarray:
                        {
                            short index = readShort();
                            CClassInfo type = (CClassInfo)ClassFile.Constants[index];

                            int count = PopInt();
                            if (count < 0)
                            {
                                foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NegativeArraySizeException"))
                                {
                                    if (e == 0)
                                    {
                                        yield return e;
                                    }
                                    else
                                    {
                                        exception = e;
                                    }
                                }
                                break;
                            }

                            Push(Heap.CreateArray(4, count, ClassObjectManager.GetClassObjectAddr(type.Name)));
                        }
                        break;
                    case OpCodes.arraylength:
                        {
                            int arrayRef = PopInt();
                            HeapArray array = Heap.GetArray(arrayRef);
                            Push(array.Length);
                        }
                        break;
                    case OpCodes.athrow:
                        {
                            int objRef = Utility.PeekInt(Stack, sp);
                            Stack = new int[MaxStack];
                            Stack[0] = objRef;
                            sp = 1;
                            HeapObject obj = Heap.GetObject(objRef);
                            int messageAddr = obj.GetField("detailMessage", "Ljava/lang/String;");
                            if (messageAddr == 0)
                            {
                                throw new JavaException(obj.ClassFile); //Handled in the same frame, outside of this switch
                            }
                            else
                            {
                                throw new JavaException(obj.ClassFile, $"{JavaHelper.ReadJavaString(messageAddr)}");
                            }
                        }
                    case OpCodes.checkcast:
                    case OpCodes.instanceof:
                        {
                            //https://docs.oracle.com/javase/specs/jvms/se7/html/jvms-6.html#jvms-6.5.checkcast
                            short index = readShort();
                            int objectRef;

                            if ((OpCodes)opCode == OpCodes.instanceof)
                            {
                                objectRef = PopInt();
                            }
                            else
                            {
                                objectRef = Utility.PeekInt(Stack, sp);
                            }

                            if (objectRef == 0)
                            {
                                if ((OpCodes)opCode == OpCodes.instanceof)
                                {
                                    Push(0);
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
                                    Push(result);
                                }
                                else
                                {
                                    if (!instanceOf)
                                    {
                                        foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/ClassCastException"))
                                        {
                                            if (e == 0)
                                            {
                                                yield return e;
                                            }
                                            else
                                            {
                                                exception = e;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    case OpCodes.monitorenter:
                        {
                            int objectRef = PopInt();
                            // todo
                        }
                        break;
                    case OpCodes.monitorexit:
                        {
                            int objectRef = PopInt();
                            // todo
                        }
                        break;
                    case OpCodes.ifnull:
                        {
                            int offset = readShort();
                            if (PopInt() == 0)
                            {
                                ip = oldIp + offset;
                            }
                        }
                        break;
                    case OpCodes.ifnonnull:
                        {
                            int offset = readShort();
                            if (PopInt() != 0)
                            {
                                ip = oldIp + offset;
                            }
                        }
                        break;
                    case OpCodes.goto_w:
                        {
                            int offset = readInt();
                            ip = oldIp + offset;
                        }
                        break;
                    case OpCodes.jsr_w:
                        {
                            int offset = readInt();
                            int retAddress = ip;
                            Push(retAddress);

                            ip = oldIp + offset;
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"Missing Op Code: 0x{opCode:X2} = {Enum.GetName(typeof(OpCodes), opCode)}");
                }

                if (exception != 0)
                {
                    bool handled = false;
                    ClassFile exceptionCFile = Heap.GetObject(exception).ClassFile;

                    for (int i = 0; i < ExceptionTable.Length; i++)
                    {
                        ExceptionHandlerInfo handler = ExceptionTable[i];
                        if ((handler.CatchType == 0 || exceptionCFile.IsSubClassOf(ClassFileManager.GetClassFile(handler.CatchClassType.Name))) &&
                            ip >= handler.StartPc && ip < handler.EndPc)
                        {
                            ip = handler.HandlerPc;
                            handled = true;
                            break;
                        }
                    }

                    if (handled)
                    {
                        exception = 0;
                    }
                    else
                    {
                        Program.StackTracePrinter.PrintMethodThrewException(MethodInfo, exception);
                        if (Program.MethodFrameStack.Count > 1)
                        {
                            MethodFrame parentFrame = Program.MethodFrameStack.Peek(1);
                            parentFrame.Stack = new int[parentFrame.Stack.Length];
                            parentFrame.sp = 1;
                            parentFrame.Stack[0] = PopInt();
                        }
                        Program.MethodFrameStack.Pop();
                        yield return exception;
                    }
                }
            }
        }
    }
}