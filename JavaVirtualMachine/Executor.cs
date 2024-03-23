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
        public struct MethodFrame
        {
            public readonly MethodInfo Method;
            public readonly int BaseOffset;
            public readonly IEnumerator<MethodInfo>? NativeState;

            public int SP
            {
                get => Executor.Stack.Span[BaseOffset + Method.MaxLocals];
                set => Executor.Stack.Span[BaseOffset + Method.MaxLocals] = value;
            }

            public int IP
            {
                get => Executor.Stack.Span[BaseOffset + Method.MaxLocals + 1];
                set => Executor.Stack.Span[BaseOffset + Method.MaxLocals + 2] = value;
            }

            public Span<int> Stack => Executor.Stack.Span.Slice(BaseOffset + 2, Method.MaxStack);

            public MethodFrame(MethodInfo methodInfo, int baseOffset, IEnumerator<MethodInfo>? nativeState)
            {
                Method = methodInfo;
                BaseOffset = baseOffset;
                NativeState = nativeState;
                SP = 0;
                IP = 0;
            }
        }

        public static Memory<int> Stack = new Memory<int>(new int[short.MaxValue]);
        public static Stack<MethodFrame> MethodFrameStack = new();

        private static int sp = 0;
        private static int activeException = 0;

        public static void MainLoop()
        {
            while (MethodFrameStack.Count > 0)
            {
                // Execute the method at the top of the stack until it reaches a function call or returns

                MethodFrame currFrame = MethodFrameStack.Peek();
                MethodInfo? methodToPush = null;
                if (currFrame.Method.HasFlag(MethodInfoFlag.Native))
                {
                    if (currFrame.NativeState!.MoveNext())
                    {
                        methodToPush = currFrame.NativeState.Current;
                    }
                    else
                    {
                        MethodFrameStack.Pop();
                    }
                }
                else
                {
                    methodToPush = InterpretUntilCallOrRet();
                    if (methodToPush == null)
                    {
                        MethodFrameStack.Pop();
                    }
                }

                // Push a new method onto the stack, if needed

                if (methodToPush != null)
                {
                    int newFrameOffset = currFrame.BaseOffset + currFrame.Method.MaxLocals + 2 + currFrame.Method.MaxStack;
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
                    sp = newFrameOffset + newFrameSize;
                }
            }
        }

        public static MethodInfo ThrowJavaException(string type)
        {
            int exceptionCFileIdx = ClassFileManager.GetClassFileIndex(type);
            ClassFile exceptionCFile = ClassFileManager.ClassFiles[exceptionCFileIdx];
            int exceptionObjRef = Heap.CreateObject(exceptionCFileIdx);

            MethodFrame currFrame = MethodFrameStack.Peek();
            currFrame.Stack[0] = exceptionObjRef;
            Stack.Span[currFrame.BaseOffset + currFrame.Method.MaxLocals + 2 + 1] = exceptionObjRef;
            currFrame.SP = 1;

            MethodInfo initMethod = exceptionCFile.MethodDictionary[("<init>", "()V")];
            return initMethod;
        }

        public static IEnumerator<MethodInfo> ExecuteNative()
        {
            MethodFrame thisFrame = MethodFrameStack.Peek();
            MethodInfo methodInfo = thisFrame.Method;
            string className = methodInfo.ClassFile.Name;
            string thisFuncName = methodInfo.Name;
            string thisDescriptor = methodInfo.Descriptor;
            HeapObject obj = default;

            Span<int> args = Stack.Span.Slice(thisFrame.BaseOffset, methodInfo.MaxLocals);
            Span<int> stack = thisFrame.Stack;

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
                        int pathFieldAddr = obj.GetField("path", "Ljava/lang/String;");
                        if (pathFieldAddr == 0)
                        {
                            int fileDescriptorAddr = obj.GetField("fd", "Ljava/io/FileDescriptor;");
                            long handle = Heap.GetObject(fileDescriptorAddr).GetField("handle", "J");
                            if (handle != 0)
                            {
                                yield return ThrowJavaException("java/io/IOException");
                                activeException = thisFrame.Stack[0];
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
                            string path = JavaHelper.ReadJavaString(obj.GetField("path", "Ljava/lang/String;"));
                            int available = FileStreams.AvailableBytes(path);
                            JavaHelper.ReturnValue(available);
                        }
                        yield break;
                    }
                case ("java/io/FileInputStream", "close0", "()V"):
                    {
                        string path = JavaHelper.ReadJavaString(obj.GetField("path", "Ljava/lang/String;"));
                        FileStreams.Close(path);
                        obj.SetField("closed", "Z", 1);
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
                            obj.SetField("path", "Ljava/lang/String;", args[1]);
                        }
                        else
                        {
                            yield return ThrowJavaException("java/io/FileNotFoundException");
                            activeException = thisFrame.Stack[0];
                            yield break;
                        }
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/io/FileInputStream", "readBytes", "([BII)I"):
                    {

                        int pathFieldAddr = obj.GetField("path", "Ljava/lang/String;");

                        int byteArrAddr = args[1];
                        int offset = args[2];
                        int length = args[3];

                        if (pathFieldAddr == 0)
                        {
                            int fileDescriptorAddr = obj.GetField("fd", "Ljava/io/FileDescriptor;");
                            long handle = Heap.GetObject(fileDescriptorAddr).GetFieldLong("handle", "J");
                            if (handle != 0)
                            {
                                yield return ThrowJavaException("java/io/IOException");
                                activeException = thisFrame.Stack[0];
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
                        string path = JavaHelper.ReadJavaString(obj.GetField("path", "Ljava/lang/String;"));
                        FileStreams.Close(path);
                        obj.SetField("closed", "Z", 1);
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
                            obj.SetField("path", "Ljava/lang/String;", args[1]);
                        }
                        else
                        {
                            yield return ThrowJavaException("java/io/FileNotFoundException");
                            activeException = thisFrame.Stack[0];
                            yield break;
                        }
                        JavaHelper.ReturnVoid();
                        yield break;
                    }
                case ("java/io/FileOutputStream", "writeBytes", "([BIIZ)V"):
                    {
                        int pathFieldAddr = obj.GetField("path", "Ljava/lang/String;");
                        int byteArrAddr = args[1];
                        int offset = args[2];
                        int length = args[3];
                        bool append = args[4] != 0;

                        if (pathFieldAddr == 0)
                        {
                            HeapObject fileDescriptor = Heap.GetObject(obj.GetField("fd", "Ljava/io/FileDescriptor;"));
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
                                activeException = thisFrame.Stack[0];
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
                            foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/io/IOException"))
                            {
                                yield return e;
                            }
                        }
                        yield break;
                    }
                case ("java/io/WinNTFileSystem", "getBooleanAttributes", "(Ljava/io/File;)I"):
                    {
                        ClassFile fileCFile = ClassFileManager.GetClassFile("java/io/File");
                        MethodInfo getPathMethod = fileCFile.MethodDictionary[("getPath", "()Ljava/lang/String;")];
                        yield return JavaHelper.RunJavaFunction(getPathMethod, args[1]);
                        string path = JavaHelper.ReadJavaString(Utility.PopInt(stack, ref sp));

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
                            foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/io/FileNotFoundException"))
                            {
                                yield return e;
                            }
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
                        string name = JavaHelper.ClassObjectName(obj);
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
                        ClassFile cFile = ClassFileManager.GetClassFile(JavaHelper.ClassObjectName(obj));

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

                                yield return JavaHelper.RunJavaFunction(constructorConstructor, constructorObj,
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
                        ClassFile cFile = ClassFileManager.GetClassFile(JavaHelper.ClassObjectName(obj));
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

                            yield return JavaHelper.RunJavaFunction(initMethod, fieldAddr,
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
                        string name = JavaHelper.ClassObjectName(obj);
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
                        string name = JavaHelper.ClassObjectName(obj);
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
                        ClassFile cFile = ClassFileManager.GetClassFile(JavaHelper.ClassObjectName(obj));

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
                        string descriptor = JavaHelper.ReadJavaString(obj.GetField(2));
                        if (descriptor == "java.lang.Object" || descriptor.Length == 1 || obj.ClassFile.IsInterface())
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
                        string name = JavaHelper.ClassObjectName(obj);
                        bool isArray = name[0] == '[';
                        JavaHelper.ReturnValue(isArray ? 1 : 0);
                        yield break;
                    }
                case ("java/lang/Class", "isAssignableFrom", "(Ljava/lang/Class;)Z"):
                    {
                        if (args[1] == 0)
                        {
                            foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NullPointerException"))
                            {
                                yield return e;
                            }
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
                        string name = JavaHelper.ReadJavaString(obj.GetField("name", "Ljava/lang/String;"));
                        ClassFile cFile = ClassFileManager.GetClassFile(name);
                        JavaHelper.ReturnValue(cFile.IsInterface() ? 1 : 0);
                        yield break;
                    }
                case ("java/lang/Class", "isPrimitive", "()Z"):
                    {
                        string name = JavaHelper.ClassObjectName(obj);
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
                        stack = new int[1];
                        yield return JavaHelper.RunJavaFunction(getPropMethod, JavaHelper.CreateJavaStringLiteral("java.library.path"));
                        int libPathAddr = Utility.PopInt(stack, ref sp);
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

                        yield return JavaHelper.RunJavaFunction(addItemToVector, systemNativeLibrariesRef, args[0]);
                        yield return JavaHelper.RunJavaFunction(addItemToVector, loadedLibraryNamesRef, args[1]);

                        obj.SetField("loaded", "Z", 1);

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
                        if (obj is HeapArray arr)
                        {
                            int itemTypeAddr = arr.ItemTypeClassObjAddr;
                            int nameAddr = Heap.GetObject(itemTypeAddr).GetField("name", "Ljava/lang/String;");
                            string itemTypeName = JavaHelper.ReadJavaString(nameAddr);
                            string arrName = '[' + itemTypeName;
                            JavaHelper.ReturnValue(ClassObjectManager.GetClassObjectAddr(arrName));
                        }
                        else
                        {
                            JavaHelper.ReturnValue(ClassObjectManager.GetClassObjectAddr(obj.ClassFile.Name));
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
                            foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/lang/NegativeArraySizeException"))
                            {
                                yield return e;
                            }
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
                        stack = new int[1];

                        //Complete list: https://docs.oracle.com/javase/8/docs/api/java/lang/System.html#getProperties--

                        MethodInfo setPropertyMethod = propertiesObject.ClassFile.MethodDictionary[("setProperty", "(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/Object;")];

                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("java.home"),
                                                                            JavaHelper.CreateJavaStringLiteral(Program.Configuration.javaHome)); Utility.PopInt(stack, ref sp);
                        //JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("java.library.path"),
                        //JavaHelper.CreateJavaStringLiteral(Environment.GetEnvironmentVariable("JAVA_HOME") + "\\bin")); Utility.PopInt(Stack, ref sp);
                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("file.encoding"),
                                                                            JavaHelper.CreateJavaStringLiteral("UTF16le")); Utility.PopInt(stack, ref sp);
                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("os.arch"),
                                                                            JavaHelper.CreateJavaStringLiteral("x64")); Utility.PopInt(stack, ref sp);
                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("os.name"),
                                                                            JavaHelper.CreateJavaStringLiteral(Environment.OSVersion.Platform.ToString())); Utility.PopInt(stack, ref sp);
                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("os.version"),
                                                                            JavaHelper.CreateJavaStringLiteral(Environment.OSVersion.Version.Major.ToString())); Utility.PopInt(stack, ref sp);
                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("file.separator"),
                                                                            JavaHelper.CreateJavaStringLiteral(Path.DirectorySeparatorChar.ToString())); Utility.PopInt(stack, ref sp);
                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("path.separator"),
                                                                            JavaHelper.CreateJavaStringLiteral(Path.PathSeparator.ToString())); Utility.PopInt(stack, ref sp);
                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("line.separator"),
                                                                            JavaHelper.CreateJavaStringLiteral(Environment.NewLine)); Utility.PopInt(stack, ref sp);
                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("user.name"),
                                                                            JavaHelper.CreateJavaStringLiteral(Environment.UserName)); Utility.PopInt(stack, ref sp);
                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("user.home"),
                                                                            JavaHelper.CreateJavaStringLiteral(Environment.GetFolderPath(Environment.SpecialFolder.Personal))); Utility.PopInt(stack, ref sp);
                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("java.library.path"),
                                                                            JavaHelper.CreateJavaStringLiteral(Program.Configuration.javaHome + @"\lib")); Utility.PopInt(stack, ref sp);
                        yield return JavaHelper.RunJavaFunction(setPropertyMethod, args[0], JavaHelper.CreateJavaStringLiteral("sun.lang.ClassLoader.allowArraySyntax"),
                                                                            JavaHelper.CreateJavaStringLiteral("true")); Utility.PopInt(stack, ref sp);


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
                        int threadStatus = obj.GetField("threadStatus", "I");
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
                        int threadStatus = obj.GetField("threadStatus", "I");
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
                        int targetAddr = obj.GetField("target", "Ljava/lang/Runnable;");
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
                        JavaHelper.ReturnValue(Program.MethodFrameStack.Count);
                        yield break;
                    }
                case ("java/lang/Throwable", "getStackTraceElement", "(I)Ljava/lang/StackTraceElement;"):
                    {
                        int index = args[1];

                        int stackTraceElementCFileIdx = ClassFileManager.GetClassFileIndex("java/lang/StackTraceElement");
                        ClassFile stackTraceElementCFile = ClassFileManager.ClassFiles[stackTraceElementCFileIdx];
                        int objAddr = Heap.CreateObject(stackTraceElementCFileIdx);

                        MethodInfo ctor = stackTraceElementCFile.MethodDictionary[("<init>", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;I)V")];

                        MethodFrame frame = Program.MethodFrameStack.Peek(index);
                        int declaringClass = JavaHelper.CreateJavaStringLiteral(frame.ClassFile.Name);
                        int methodName = JavaHelper.CreateJavaStringLiteral(frame.MethodInfo.Name);
                        int fileName = 0;
                        int lineNumber = frame is NativeMethodFrame ? -2 : -1;

                        yield return JavaHelper.RunJavaFunction(ctor, objAddr,
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
                            yield return JavaHelper.RunJavaFunction(constructor, newObjAddr);
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
                        stack = new int[1];
                        HeapObject privilegedAction = Heap.GetObject(args[0]);
                        MethodInfo method = privilegedAction.ClassFile.MethodDictionary[("run", "()Ljava/lang/Object;")];

                        yield return JavaHelper.RunJavaFunction(method, args);
                        //methodFrame.Execute returns to this NativeMethodFrame's stack
                        JavaHelper.ReturnValue(Utility.PopInt(stack, ref sp));
                        yield break;
                    }
                case ("java/security/AccessController", "doPrivileged", "(Ljava/security/PrivilegedAction;Ljava/security/AccessControlContext;)Ljava/lang/Object;"):
                    {
                        stack = new int[1];
                        HeapObject privilegedAction = Heap.GetObject(args[0]);
                        MethodInfo method = privilegedAction.ClassFile.MethodDictionary[("run", "()Ljava/lang/Object;")];

                        yield return JavaHelper.RunJavaFunction(method, args[0]);
                        JavaHelper.ReturnValue(Utility.PopInt(stack, ref sp));
                        yield break;
                    }
                case ("java/security/AccessController", "doPrivileged", "(Ljava/security/PrivilegedExceptionAction;)Ljava/lang/Object;"):
                    {
                        stack = new int[1];
                        HeapObject privilegedExceptionAction = Heap.GetObject(args[0]);
                        MethodInfo method = privilegedExceptionAction.ClassFile.MethodDictionary[("run", "()Ljava/lang/Object;")];
                        //DebugWriter.CallFuncDebugWrite(privilegedExceptionAction.ClassObject.Name, "run", Args);
                        yield return JavaHelper.RunJavaFunction(method, args);
                        //methodFrame.Execute returns to this NativeMethodFrame's stack
                        JavaHelper.ReturnValue(Utility.PopInt(stack, ref sp));
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

                            //JavaHelper.RunJavaFunction(putMethod, signalsTableRef.Address, signalNumber, args[])

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
                            foreach (int e in JavaHelper.ThrowJavaExceptionYielding("java/io/IOException"))
                            {
                                yield return e;
                            }
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
                        yield return JavaHelper.RunJavaFunction(getDeclaringClassMethod, constructorAddr);
                        int declaringClassClassObjAddr = Utility.PopInt(stack, ref sp);
                        HeapObject declaringClassClassObj = Heap.GetObject(declaringClassClassObjAddr);

                        string declaringClassName = JavaHelper.ReadJavaString(declaringClassClassObj.GetField("name", "Ljava/lang/String;"));
                        int declaringClassCFileIdx = ClassFileManager.GetClassFileIndex(declaringClassName);
                        ClassFile declaringClass = ClassFileManager.ClassFiles[declaringClassCFileIdx];

                        //Get slot
                        MethodInfo getSlotMethod = constructorClassFile.MethodDictionary[("getSlot", "()I")];
                        yield return JavaHelper.RunJavaFunction(getSlotMethod, constructorAddr);
                        int slot = Utility.PopInt(stack, ref sp);

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
                        yield return JavaHelper.RunJavaFunction(constructorMethod, arguments);
                        JavaHelper.ReturnValue(newObjectAddr);

                        yield break;
                    }
                case ("sun/reflect/Reflection", "getCallerClass", "()Ljava/lang/Class;"):
                    {
                        MethodFrame frame = Program.MethodFrameStack.Peek(2);
                        ClassFile callerClass = frame.ClassFile;

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
            throw new NotImplementedException();
        }
    }
}
