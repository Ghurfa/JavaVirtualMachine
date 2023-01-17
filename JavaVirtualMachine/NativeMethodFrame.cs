using JavaVirtualMachine.Attributes;
using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Runtime.InteropServices;

namespace JavaVirtualMachine
{
    class NativeMethodFrame : MethodFrame
    {
        public int[] Args;
        public NativeMethodFrame(MethodInfo methodInfo)
            : base(methodInfo)
        {
        }
        public override void Execute()
        {
            try
            {
                Program.MethodFrameStack.Push(this);
                string className = ClassFile.Name;
                (string funcName, string descriptor) nameAndDescriptor = (MethodInfo.Name, MethodInfo.Descriptor);
                HeapObject obj = default;
                if (!MethodInfo.HasFlag(MethodInfoFlag.Static))
                {
                    obj = Heap.GetObject(Args[0]);
                }

                if (className == "java/io/FileDescriptor" && nameAndDescriptor == ("initIDs", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/io/FileDescriptor" && nameAndDescriptor == ("set", "(I)J"))
                {
                    int d = Args[0];
                    if (d == 0) //in
                    {
                        JavaHelper.ReturnLargeValue(0L);
                    }
                    else if (d == 1) //out
                    {
                        JavaHelper.ReturnLargeValue(1L);
                    }
                    else if (d == 2) //err
                    {
                        JavaHelper.ReturnLargeValue(2L);
                    }
                    else
                    {
                        JavaHelper.ReturnLargeValue(-1L);
                    }
                    return;
                }
                else if (className == "java/io/FileInputStream" && nameAndDescriptor == ("available0", "()I"))
                {
                    int pathFieldAddr = obj.GetField("path", "Ljava/lang/String;");
                    if (pathFieldAddr == 0)
                    {
                        int fileDescriptorAddr = obj.GetField("fd", "Ljava/io/FileDescriptor;");
                        long handle = Heap.GetObject(fileDescriptorAddr).GetField("handle", "J");
                        if (handle != 0)
                        {
                            JavaHelper.ThrowJavaException("java/io/IOException");
                        }
                        else
                        {
                            int available = FileStreams.AvailableBytesFromConsole();
                            JavaHelper.ReturnValue(available);
                            return;
                        }
                    }
                    else
                    {
                        string path = JavaHelper.ReadJavaString(obj.GetField("path", "Ljava/lang/String;"));
                        int available = FileStreams.AvailableBytes(path);
                        JavaHelper.ReturnValue(available);
                        return;
                    }
                }
                else if (className == "java/io/FileInputStream" && nameAndDescriptor == ("close0", "()V"))
                {
                    string path = JavaHelper.ReadJavaString(obj.GetField("path", "Ljava/lang/String;"));
                    FileStreams.Close(path);
                    obj.SetField("closed", "Z", 1);
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/io/FileInputStream" && nameAndDescriptor == ("initIDs", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/io/FileInputStream" && nameAndDescriptor == ("open0", "(Ljava/lang/String;)V"))
                {
                    string fileName = JavaHelper.ReadJavaString(Args[1]);
                    try
                    {
                        FileStreams.OpenRead(fileName);
                        obj.SetField("path", "Ljava/lang/String;", Args[1]);
                    }
                    catch (FileNotFoundException)
                    {
                        JavaHelper.ThrowJavaException("java/io/FileNotFoundException");
                    }
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/io/FileInputStream" && nameAndDescriptor == ("readBytes", "([BII)I"))
                {
                    int pathFieldAddr = obj.GetField("path", "Ljava/lang/String;");

                    int byteArrAddr = Args[1];
                    int offset = Args[2];
                    int length = Args[3];

                    if (pathFieldAddr == 0)
                    {
                        int fileDescriptorAddr = obj.GetField("fd", "Ljava/io/FileDescriptor;");
                        long handle = Heap.GetObject(fileDescriptorAddr).GetFieldLong("handle", "J");
                        if (handle != 0)
                        {
                            JavaHelper.ThrowJavaException("java/io/IOException");
                        }
                        else
                        {
                            HeapArray javaByteArr = Heap.GetArray(byteArrAddr);
                            int bytesRead = FileStreams.ReadBytesFromConsole(javaByteArr.GetDataSpan().Slice(offset, length));

                            JavaHelper.ReturnValue(bytesRead);
                            return;
                        }
                    }
                    else
                    {
                        string path = JavaHelper.ReadJavaString(pathFieldAddr);
                        HeapArray javaByteArr = Heap.GetArray(byteArrAddr);

                        int bytesRead = FileStreams.ReadBytes(path, javaByteArr.GetDataSpan().Slice(offset, length));

                        JavaHelper.ReturnValue(bytesRead);
                        return;
                    }
                }
                else if (className == "java/io/FileOutputStream" && nameAndDescriptor == ("close0", "()V"))
                {
                    string path = JavaHelper.ReadJavaString(obj.GetField("path", "Ljava/lang/String;"));
                    FileStreams.Close(path);
                    obj.SetField("closed", "Z", 1);
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/io/FileOutputStream" && nameAndDescriptor == ("initIDs", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/io/FileOutputStream" && nameAndDescriptor == ("open0", "(Ljava/lang/String;Z)V"))
                {
                    string fileName = JavaHelper.ReadJavaString(Args[1]);
                    try
                    {
                        FileStreams.OpenWrite(fileName);
                        obj.SetField("path", "Ljava/lang/String;", Args[1]);
                    }
                    catch (FileNotFoundException)
                    {
                        JavaHelper.ThrowJavaException("java/io/FileNotFoundException");
                    }
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/io/FileOutputStream" && nameAndDescriptor == ("writeBytes", "([BIIZ)V"))
                {
                    int pathFieldAddr = obj.GetField("path", "Ljava/lang/String;");
                    int byteArrAddr = Args[1];
                    int offset = Args[2];
                    int length = Args[3];
                    bool append = Args[4] != 0;

                    if (pathFieldAddr == 0)
                    {
                        HeapObject fileDescriptor = Heap.GetObject(obj.GetField("fd", "Ljava/io/FileDescriptor;"));
                        long handle = fileDescriptor.GetFieldLong("handle", "J"); //address (defined in java/io/FileDescriptor set(int))
                        if (handle == 1)
                        {
                            HeapArray javaByteArr = Heap.GetArray(byteArrAddr);
                            FileStreams.WriteBytesToConsole(javaByteArr.GetDataSpan().Slice(offset, length));

                            JavaHelper.ReturnVoid();
                            return;
                        }
                        else if (handle == 2)
                        {
                            HeapArray javaByteArr = Heap.GetArray(byteArrAddr);
                            FileStreams.WriteBytesToError(javaByteArr.GetDataSpan().Slice(offset, length));

                            JavaHelper.ReturnVoid();
                            return;
                        }
                        else
                        {
                            JavaHelper.ThrowJavaException("java/io/IOException");
                        }
                    }
                    else
                    {
                        string path = JavaHelper.ReadJavaString(pathFieldAddr);
                        HeapArray javaByteArr = Heap.GetArray(byteArrAddr);

                        throw new NotImplementedException();
                        //FileStreams.WriteBytes(path, (byte[])javaByteArr.Array, offset, length, append);

                        JavaHelper.ReturnVoid();
                        return;
                    }

                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/io/WinNTFileSystem" && nameAndDescriptor == ("canonicalize0", "(Ljava/lang/String;)Ljava/lang/String;"))
                {
                    string input = JavaHelper.ReadJavaString(Args[1]);

                    string absoluteForm = Path.GetFullPath(input);
                    //not finished

                    JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(absoluteForm));
                    return;
                }
                else if (className == "java/io/WinNTFileSystem" && nameAndDescriptor == ("createFileExclusively", "(Ljava/lang/String;)Z"))
                {
                    string path = JavaHelper.ReadJavaString(Args[1]);
                    try
                    {
                        if (File.Exists(path))
                        {
                            JavaHelper.ReturnValue(0);
                            return;
                        }
                        else
                        {
                            File.Create(path);
                            JavaHelper.ReturnValue(1);
                            return;
                        }
                    }
                    catch (IOException)
                    {
                        JavaHelper.ThrowJavaException("java/io/IOException");
                    }
                }
                else if (className == "java/io/WinNTFileSystem" && nameAndDescriptor == ("getBooleanAttributes", "(Ljava/io/File;)I"))
                {
                    ClassFile fileCFile = ClassFileManager.GetClassFile("java/io/File");
                    MethodInfo getPathMethod = fileCFile.MethodDictionary[("getPath", "()Ljava/lang/String;")];
                    JavaHelper.RunJavaFunction(getPathMethod, Args[1]);
                    string path = JavaHelper.ReadJavaString(Utility.PopInt(Stack, ref sp));

                    try
                    {
                        FileAttributes attributes = File.GetAttributes(path);
                        int ret = (int)attributes;
                        JavaHelper.ReturnValue(ret);
                        return;
                    }
                    catch (FileNotFoundException)
                    {
                        JavaHelper.ThrowJavaException("java/io/FileNotFoundException");
                    }
                }
                else if (className == "java/io/WinNTFileSystem" && nameAndDescriptor == ("initIDs", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("desiredAssertionStatus0", "(Ljava/lang/Class;)Z"))
                {
                    JavaHelper.ReturnValue(0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("forName0", "(Ljava/lang/String;ZLjava/lang/ClassLoader;Ljava/lang/Class;)Ljava/lang/Class;"))
                {
                    //supposed to use classloader that is passed in
                    string classToLoadName = JavaHelper.ReadJavaString(Args[0]).Replace('.', '/');
                    int classObjAddr = ClassObjectManager.GetClassObjectAddr(classToLoadName);

                    ClassFileManager.GetClassFileIndex(classToLoadName);
                    if (Args[1] == 1)
                    {
                        ClassFileManager.InitializeClass(classToLoadName);
                    }
                    JavaHelper.ReturnValue(classObjAddr);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getClassLoader0", "()Ljava/lang/ClassLoader;"))
                {
                    JavaHelper.ReturnValue(0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getComponentType", "()Ljava/lang/Class;"))
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
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getDeclaredConstructors0", "(Z)[Ljava/lang/reflect/Constructor;"))
                {
                    ClassFile cFile = ClassFileManager.GetClassFile(JavaHelper.ClassObjectName(obj));

                    int constructorClassFileIdx = ClassFileManager.GetClassFileIndex("java/lang/reflect/Constructor");
                    ClassFile constructorClassFile = ClassFileManager.ClassFiles[constructorClassFileIdx];
                    MethodInfo constructorConstructor = constructorClassFile.MethodDictionary[("<init>", "(Ljava/lang/Class;[Ljava/lang/Class;[Ljava/lang/Class;IILjava/lang/String;[B[B)V")];

                    LinkedList<int> constructorObjLinkedList = new LinkedList<int>();
                    int i = 0;
                    int declaringClass = Args[0];
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

                            JavaHelper.RunJavaFunction(constructorConstructor, constructorObj,
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
                    return;

                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getDeclaredFields0", "(Z)[Ljava/lang/reflect/Field;"))
                {
                    bool publicOnly = Args[1] != 0;
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

                        JavaHelper.RunJavaFunction(initMethod, fieldAddr,
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
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getDeclaringClass0", "()Ljava/lang/Class;"))
                {
                    string name = JavaHelper.ClassObjectName(obj);
                    if (JavaHelper.IsPrimitiveType(name))
                    {
                        JavaHelper.ReturnValue(0);
                        return;
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
                                    return;
                                }
                            }
                            JavaHelper.ReturnValue(0);
                            return;
                        }
                    }
                    JavaHelper.ReturnValue(0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getEnclosingMethod0", "()[Ljava/lang/Object;"))
                {
                    string name = JavaHelper.ClassObjectName(obj);
                    if (JavaHelper.IsPrimitiveType(name))
                    {
                        JavaHelper.ReturnValue(0);
                        return;
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
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getModifiers", "()I"))
                {
                    ClassFile cFile = ClassFileManager.GetClassFile(JavaHelper.ClassObjectName(obj));

                    JavaHelper.ReturnValue(cFile.AccessFlags);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getPrimitiveClass", "(Ljava/lang/String;)Ljava/lang/Class;"))
                {
                    string name = JavaHelper.ReadJavaString(Args[0]);
                    int classObjAddr = classObjAddr = ClassObjectManager.GetClassObjectAddr(name);
                    JavaHelper.ReturnValue(classObjAddr);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getProtectionDomain0", "()Ljava/security/ProtectionDomain;"))
                {
                    JavaHelper.ReturnValue(0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getSuperclass", "()Ljava/lang/Class;"))
                {
                    string descriptor = JavaHelper.ReadJavaString(obj.GetField(2));
                    if (descriptor == "java.lang.Object" || descriptor.Length == 1 || obj.ClassFile.IsInterface())
                    {
                        JavaHelper.ReturnValue(0);
                        return;
                    }
                    ClassFile cFile = ClassFileManager.GetClassFile(descriptor);
                    ClassFile superClass = cFile.SuperClass;
                    JavaHelper.ReturnValue(ClassObjectManager.GetClassObjectAddr(superClass.Name));
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("isArray", "()Z"))
                {
                    string name = JavaHelper.ClassObjectName(obj);
                    bool isArray = name[0] == '[';
                    JavaHelper.ReturnValue(isArray ? 1 : 0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("isAssignableFrom", "(Ljava/lang/Class;)Z"))
                {
                    if (Args[1] == 0)
                    {
                        JavaHelper.ThrowJavaException("java/lang/NullPointerException");
                    }
                    int thisCFileIdx = ClassFileManager.GetClassFileIndex(Args[0]);
                    int otherCFileIdx = ClassFileManager.GetClassFileIndex(Args[1]);
                    ClassFile thisCFile = ClassFileManager.ClassFiles[thisCFileIdx];
                    ClassFile otherCFile = ClassFileManager.ClassFiles[otherCFileIdx];
                    bool assignable = JavaHelper.IsSubClassOf(otherCFile, thisCFile);
                    JavaHelper.ReturnValue(assignable ? 1 : 0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("isInstance", "(Ljava/lang/Object;)Z"))
                {
                    HeapObject objToCheck = Heap.GetObject(Args[1]);
                    JavaHelper.ReturnValue(objToCheck.IsInstance(Args[0]) ? 1 : 0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("isInterface", "()Z"))
                {
                    string name = JavaHelper.ReadJavaString(obj.GetField("name", "Ljava/lang/String;"));
                    ClassFile cFile = ClassFileManager.GetClassFile(name);
                    JavaHelper.ReturnValue(cFile.IsInterface() ? 1 : 0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("isPrimitive", "()Z"))
                {
                    string name = JavaHelper.ClassObjectName(obj);
                    JavaHelper.ReturnValue(JavaHelper.IsPrimitiveType(name) ? 1 : 0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("<clinit>", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/ClassLoader" && nameAndDescriptor == ("findBuiltinLib", "(Ljava/lang/String;)Ljava/lang/String;"))
                {
                    //temp
                    string name = JavaHelper.ReadJavaString(Args[0]);
                    ClassFile systemCFile = ClassFileManager.GetClassFile("java/lang/System");
                    MethodInfo getPropMethod = systemCFile.MethodDictionary[("getProperty", "(Ljava/lang/String;)Ljava/lang/String;")];
                    Stack = new int[1];
                    JavaHelper.RunJavaFunction(getPropMethod, JavaHelper.CreateJavaStringLiteral("java.library.path"));
                    int libPathAddr = Utility.PopInt(Stack, ref sp);
                    string libPath = JavaHelper.ReadJavaString(libPathAddr);
                    JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(libPath + name));
                    return;
                    //throw new NotImplementedException();
                }
                else if (className == "java/lang/ClassLoader" && nameAndDescriptor == ("findLoadedClass0", "(Ljava/lang/String;)Ljava/lang/Class;"))
                {
                    JavaHelper.ReturnValue(ClassObjectManager.GetClassObjectAddr(JavaHelper.ReadJavaString(Args[1])));
                    return;
                }
                else if (className == "java/lang/ClassLoader" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/ClassLoader$NativeLibrary" && nameAndDescriptor == ("load", "(Ljava/lang/String;Z)V"))
                {
                    ClassFile classLoaderCFile = ClassFileManager.GetClassFile("java/lang/ClassLoader");
                    int systemNativeLibrariesRef = (int)classLoaderCFile.StaticFieldsDictionary[("systemNativeLibraries", "Ljava/util/Vector;")];
                    HeapObject systemNativeLibraries = Heap.GetObject(systemNativeLibrariesRef);
                    int loadedLibraryNamesRef = (int)classLoaderCFile.StaticFieldsDictionary[("loadedLibraryNames", "Ljava/util/Vector;")];
                    HeapObject loadedLibraryNames = Heap.GetObject(loadedLibraryNamesRef);

                    ClassFile vectorCFile = ClassFileManager.GetClassFile("java/util/Vector");
                    MethodInfo addItemToVector = vectorCFile.MethodDictionary[("addElement", "(Ljava/lang/Object;)V")];

                    JavaHelper.RunJavaFunction(addItemToVector, systemNativeLibrariesRef, Args[0]);
                    JavaHelper.RunJavaFunction(addItemToVector, loadedLibraryNamesRef, Args[1]);

                    obj.SetField("loaded", "Z", 1);

                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Double" && nameAndDescriptor == ("doubleToRawLongBits", "(D)J"))
                {
                    //reversed?
                    //Double is already represented as long
                    JavaHelper.ReturnLargeValue((Args[1], Args[0]).ToLong());
                    return;
                }
                else if (className == "java/lang/Double" && nameAndDescriptor == ("longBitsToDouble", "(J)D"))
                {
                    //reversed?
                    //Double is stored as long
                    JavaHelper.ReturnLargeValue((Args[1], Args[0]).ToLong());
                    return;
                }
                else if (className == "java/lang/Float" && nameAndDescriptor == ("floatToRawIntBits", "(F)I"))
                {
                    //Float is already represented as int
                    JavaHelper.ReturnValue(Args[0]);
                    return;
                }
                else if (className == "java/lang/invoke/MethodHandleNatives" && nameAndDescriptor == ("getConstant", "(I)I"))
                {
                    if (Args[0] == 0) //MethodHandlePushLimit
                    {
                        JavaHelper.ReturnValue(0);
                        return;
                    }
                    else //1 = stack slot push size
                    {
                        JavaHelper.ReturnValue(0);
                        return;
                    }
                }
                else if (className == "java/lang/invoke/MethodHandleNatives" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/invoke/MethodHandleNatives" && nameAndDescriptor == ("resolve", "(Ljava/lang/invoke/MemberName;Ljava/lang/Class;)Ljava/lang/invoke/MemberName;"))
                {
                    throw new NotImplementedException();
                    HeapObject self = Heap.GetObject(Args[0]);
                    HeapObject caller = Heap.GetObject(Args[1]);

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


                    JavaHelper.ReturnValue(Args[0]);
                    return;
                }
                else if (className == "java/lang/Object" && nameAndDescriptor == ("clone", "()Ljava/lang/Object;"))
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
                    JavaHelper.ReturnValue(Heap.CloneObject(Args[0]));
                    return;
                }
                else if (className == "java/lang/Object" && nameAndDescriptor == ("getClass", "()Ljava/lang/Class;"))
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
                    return;
                }
                else if (className == "java/lang/Object" && nameAndDescriptor == ("hashCode", "()I"))
                {
                    //return new int[] {address of object
                    JavaHelper.ReturnValue(Args[0]);
                    return;
                }
                else if (className == "java/lang/Object" && nameAndDescriptor == ("notifyAll", "()V"))
                {
                    //don't support multiple threads
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Object" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    //unsure what natives to register
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/reflect/Array" && nameAndDescriptor == ("newArray", "(Ljava/lang/Class;I)Ljava/lang/Object;"))
                {
                    int typeClassObjAddr = Args[0];
                    int length = Args[1];
                    if (length < 0) JavaHelper.ThrowJavaException("java/lang/NegativeArraySizeException");
                    JavaHelper.ReturnValue(Heap.CreateArray(4, length, typeClassObjAddr));
                    return;
                }
                else if (className == "java/lang/Runtime" && nameAndDescriptor == ("availableProcessors", "()I"))
                {
                    JavaHelper.ReturnValue(1);
                    return;
                }
                else if (className == "java/lang/String" && nameAndDescriptor == ("intern", "()Ljava/lang/String;"))
                {
                    string str = JavaHelper.ReadJavaString(Args[0]);
                    if (StringPool.StringAddresses.TryGetValue(str, out int strAddr))
                    {
                        JavaHelper.ReturnValue(strAddr);
                    }
                    else
                    {
                        StringPool.StringAddresses.Add(str, Args[0]);
                        JavaHelper.ReturnValue(Args[0]);
                    }
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("arraycopy", "(Ljava/lang/Object;ILjava/lang/Object;II)V"))
                {
                    HeapArray srcArr = Heap.GetArray(Args[0]);
                    int srcStartInd = Args[1];
                    HeapArray destArr = Heap.GetArray(Args[2]);
                    int destStartInd = Args[3];
                    int count = Args[4];

                    int itemSize = srcArr.ItemSize;
                    int numBytes = count * srcArr.ItemSize;
                    Span<byte> srcSpan = srcArr.GetDataSpan().Slice(itemSize * srcStartInd, numBytes);
                    Span<byte> destSpan = destArr.GetDataSpan().Slice(itemSize * destStartInd);
                    srcSpan.CopyTo(destSpan);
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("currentTimeMillis", "()J"))
                {
                    DateTime dt1970 = new DateTime(1970, 1, 1);
                    TimeSpan timeSince = DateTime.Now - dt1970;
                    long totalMillis = (long)timeSince.TotalMilliseconds;
                    JavaHelper.ReturnLargeValue(totalMillis);
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("identityHashCode", "(Ljava/lang/Object;)I"))
                {
                    JavaHelper.ReturnValue(Args[0]);
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("initProperties", "(Ljava/util/Properties;)Ljava/util/Properties;"))
                {
                    HeapObject propertiesObject = Heap.GetObject(Args[0]);
                    Stack = new int[1];

                    //Complete list: https://docs.oracle.com/javase/8/docs/api/java/lang/System.html#getProperties--

                    MethodInfo setPropertyMethod = propertiesObject.ClassFile.MethodDictionary[("setProperty", "(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/Object;")];

                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("java.home"),
                                                                        JavaHelper.CreateJavaStringLiteral(Program.Configuration.javaHome)); Utility.PopInt(Stack, ref sp);
                    //JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("java.library.path"),
                    //JavaHelper.CreateJavaStringLiteral(Environment.GetEnvironmentVariable("JAVA_HOME") + "\\bin")); Utility.PopInt(Stack, ref sp);
                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("file.encoding"),
                                                                        JavaHelper.CreateJavaStringLiteral("UTF16le")); Utility.PopInt(Stack, ref sp);
                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("os.arch"),
                                                                        JavaHelper.CreateJavaStringLiteral("x64")); Utility.PopInt(Stack, ref sp);
                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("os.name"),
                                                                        JavaHelper.CreateJavaStringLiteral(Environment.OSVersion.Platform.ToString())); Utility.PopInt(Stack, ref sp);
                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("os.version"),
                                                                        JavaHelper.CreateJavaStringLiteral(Environment.OSVersion.Version.Major.ToString())); Utility.PopInt(Stack, ref sp);
                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("file.separator"),
                                                                        JavaHelper.CreateJavaStringLiteral(Path.DirectorySeparatorChar.ToString())); Utility.PopInt(Stack, ref sp);
                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("path.separator"),
                                                                        JavaHelper.CreateJavaStringLiteral(Path.PathSeparator.ToString())); Utility.PopInt(Stack, ref sp);
                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("line.separator"),
                                                                        JavaHelper.CreateJavaStringLiteral(Environment.NewLine)); Utility.PopInt(Stack, ref sp);
                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("user.name"),
                                                                        JavaHelper.CreateJavaStringLiteral(Environment.UserName)); Utility.PopInt(Stack, ref sp);
                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("user.home"),
                                                                        JavaHelper.CreateJavaStringLiteral(Environment.GetFolderPath(Environment.SpecialFolder.Personal))); Utility.PopInt(Stack, ref sp);
                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("java.library.path"),
                                                                        JavaHelper.CreateJavaStringLiteral(Program.Configuration.javaHome + @"\lib")); Utility.PopInt(Stack, ref sp);
                    JavaHelper.RunJavaFunction(setPropertyMethod, Args[0], JavaHelper.CreateJavaStringLiteral("sun.lang.ClassLoader.allowArraySyntax"),
                                                                        JavaHelper.CreateJavaStringLiteral("true")); Utility.PopInt(Stack, ref sp);


                    JavaHelper.ReturnValue(Args[0]);
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("mapLibraryName", "(Ljava/lang/String;)Ljava/lang/String;"))
                {
                    string libName = JavaHelper.ReadJavaString(Args[0]);
                    string mappedName = libName.Replace('.', '/');
                    JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(mappedName));
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("nanoTime", "()J"))
                {
                    long nanoTime = Program.Stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond * 345365667;
                    JavaHelper.ReturnLargeValue(nanoTime);
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    int printStreamAddr = Heap.CreateObject(ClassFileManager.GetClassFileIndex("java/io/PrintStream"));
                    ClassFileManager.GetClassFile("java/lang/System").StaticFieldsDictionary[("out", "Ljava/io/PrintStream;")] = printStreamAddr;
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("setErr0", "(Ljava/io/PrintStream;)V"))
                {
                    ClassFile systemClassFile = ClassFileManager.GetClassFile("java/lang/System");
                    systemClassFile.StaticFieldsDictionary[("err", "Ljava/io/PrintStream;")] = Args[0];
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("setIn0", "(Ljava/io/InputStream;)V"))
                {
                    ClassFile systemClassFile = ClassFileManager.GetClassFile("java/lang/System");
                    systemClassFile.StaticFieldsDictionary[("in", "Ljava/io/InputStream;")] = Args[0];
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("setOut0", "(Ljava/io/PrintStream;)V"))
                {
                    ClassFile systemClassFile = ClassFileManager.GetClassFile("java/lang/System");
                    systemClassFile.StaticFieldsDictionary[("out", "Ljava/io/PrintStream;")] = Args[0];
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("currentThread", "()Ljava/lang/Thread;"))
                {
                    JavaHelper.ReturnValue(ThreadManager.GetThreadAddr());
                    return;
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("isAlive", "()Z"))
                {
                    int threadStatus = obj.GetField("threadStatus", "I");
                    if (threadStatus == 0)
                    {
                        JavaHelper.ReturnValue(0);
                        return;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("isInterrupted", "(Z)Z"))
                {
                    int threadStatus = obj.GetField("threadStatus", "I");
                    if (threadStatus == 2)
                    {
                        JavaHelper.ReturnValue(1);
                        return;
                    }
                    else
                    {
                        JavaHelper.ReturnValue(0);
                        return;
                    }
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("setPriority0", "(I)V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("start0", "()V"))
                {
                    int targetAddr = obj.GetField("target", "Ljava/lang/Runnable;");
                    if (targetAddr == 0)
                    {
                        JavaHelper.ReturnVoid();
                        return;
                    }
                    throw new NotImplementedException();
                }
                else if (className == "java/lang/Throwable" && nameAndDescriptor == ("fillInStackTrace", "(I)Ljava/lang/Throwable;"))
                {
                    //http://hg.openjdk.java.net/jdk7/jdk7/jdk/file/9b8c96f96a0f/src/share/classes/java/lang/Throwable.java
                    JavaHelper.ReturnValue(Args[0]);
                    return;
                }
                else if (className == "java/lang/Throwable" && nameAndDescriptor == ("getStackTraceDepth", "()I"))
                {
                    JavaHelper.ReturnValue(Program.MethodFrameStack.Count);
                    return;
                }
                else if (className == "java/lang/Throwable" && nameAndDescriptor == ("getStackTraceElement", "(I)Ljava/lang/StackTraceElement;"))
                {
                    int index = Args[1];

                    int stackTraceElementCFileIdx = ClassFileManager.GetClassFileIndex("java/lang/StackTraceElement");
                    ClassFile stackTraceElementCFile = ClassFileManager.ClassFiles[stackTraceElementCFileIdx];
                    int objAddr = Heap.CreateObject(stackTraceElementCFileIdx);

                    MethodInfo constructor = stackTraceElementCFile.MethodDictionary[("<init>", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;I)V")];

                    MethodFrame frame = Program.MethodFrameStack.Peek(index);
                    int declaringClass = JavaHelper.CreateJavaStringLiteral(frame.ClassFile.Name);
                    int methodName = JavaHelper.CreateJavaStringLiteral(frame.MethodInfo.Name);
                    int fileName = 0;
                    int lineNumber = frame is NativeMethodFrame ? -2 : -1;

                    JavaHelper.RunJavaFunction(constructor, objAddr,
                                                            declaringClass,
                                                            methodName,
                                                            fileName,
                                                            lineNumber);
                    JavaHelper.ReturnValue(objAddr);
                    return;

                }
                else if (className == "java/net/DualStackPlainSocketImpl" && nameAndDescriptor == ("connect0", "(ILjava/net/InetAddress;I)I"))
                {

                }
                else if (className == "java/net/DualStackPlainSocketImpl" && nameAndDescriptor == ("initIDs", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/net/DualStackPlainSocketImpl" && nameAndDescriptor == ("socket0", "(ZZ)I"))
                {
                    bool stream = Args[0] != 0;
                    bool v6Only = Args[1] != 0;
                    //Socket socket = new Socket(stream ? SocketType.Stream : SocketType.Dgram, v6Only ? ProtocolType.IcmpV6 : ProtocolType.Tcp);
                    //temp
                    JavaHelper.ReturnValue(0);
                    return;
                }
                else if (className == "java/net/Inet4Address" && nameAndDescriptor == ("init", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/net/Inet6Address" && nameAndDescriptor == ("init", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/net/Inet6AddressImpl" && nameAndDescriptor == ("getLocalHostName", "()Ljava/lang/String;"))
                {
                    string name = Dns.GetHostName();
                    JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(name));
                    return;
                }
                else if (className == "java/net/Inet6AddressImpl" && nameAndDescriptor == ("lookupAllHostAddr", "(Ljava/lang/String;)[Ljava/net/InetAddress;"))
                {
                    string hostName = JavaHelper.ReadJavaString(Args[1]);
                    IPAddress[] addresses = Dns.GetHostAddresses(hostName);

                    ClassFile inet6AddressCFile = ClassFileManager.GetClassFile("java/net/Inet6Address");
                    MethodInfo constructor = inet6AddressCFile.MethodDictionary[("<init>", "()V")];

                    int[] innerArray = new int[addresses.Length];

                    for (int i = 0; i < addresses.Length; i++)
                    {
                        int newObjAddr = Heap.CreateObject(ClassFileManager.GetClassFileIndex("java/net/Inet6Address"));
                        innerArray[i] = newObjAddr;
                        JavaHelper.RunJavaFunction(constructor, newObjAddr);
                    }

                    int arrayAddr = Heap.CreateArray(innerArray, ClassObjectManager.GetClassObjectAddr("java/net/Inet6Address"));

                    JavaHelper.ReturnValue(arrayAddr);
                    return;
                }
                else if (className == "java/net/InetAddress" && nameAndDescriptor == ("init", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "java/net/InetAddressImplFactory" && nameAndDescriptor == ("isIPv6Supported", "()Z"))
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
                    return;
                }
                else if (className == "java/security/AccessController" && nameAndDescriptor == ("doPrivileged", "(Ljava/security/PrivilegedAction;)Ljava/lang/Object;"))
                {
                    //http://hg.openjdk.java.net/jdk6/jdk6/jdk/file/2d585507a41b/src/share/classes/java/security/AccessController.java
                    //Runs the "run" function of the action
                    //Enables privileges?
                    //Returns result of the "run"
                    Stack = new int[1];
                    HeapObject privilegedAction = Heap.GetObject(Args[0]);
                    MethodInfo method = privilegedAction.ClassFile.MethodDictionary[("run", "()Ljava/lang/Object;")];

                    JavaHelper.RunJavaFunction(method, Args);
                    //methodFrame.Execute returns to this NativeMethodFrame's stack
                    JavaHelper.ReturnValue(Utility.PopInt(Stack, ref sp));
                    return;
                }
                else if (className == "java/security/AccessController" && nameAndDescriptor == ("doPrivileged", "(Ljava/security/PrivilegedAction;Ljava/security/AccessControlContext;)Ljava/lang/Object;"))
                {
                    Stack = new int[1];
                    HeapObject privilegedAction = Heap.GetObject(Args[0]);
                    MethodInfo method = privilegedAction.ClassFile.MethodDictionary[("run", "()Ljava/lang/Object;")];

                    JavaHelper.RunJavaFunction(method, Args[0]);
                    JavaHelper.ReturnValue(Utility.PopInt(Stack, ref sp));
                    return;
                }
                else if (className == "java/security/AccessController" && nameAndDescriptor == ("doPrivileged", "(Ljava/security/PrivilegedExceptionAction;)Ljava/lang/Object;"))
                {
                    Stack = new int[1];
                    HeapObject privilegedExceptionAction = Heap.GetObject(Args[0]);
                    MethodInfo method = privilegedExceptionAction.ClassFile.MethodDictionary[("run", "()Ljava/lang/Object;")];
                    //DebugWriter.CallFuncDebugWrite(privilegedExceptionAction.ClassObject.Name, "run", Args);
                    JavaHelper.RunJavaFunction(method, Args);
                    //methodFrame.Execute returns to this NativeMethodFrame's stack
                    JavaHelper.ReturnValue(Utility.PopInt(Stack, ref sp));
                    return;
                }
                else if (className == "java/security/AccessController" && nameAndDescriptor == ("getStackAccessControlContext", "()Ljava/security/AccessControlContext;"))
                {
                    //HeapObject accessControlContext = new HeapObject(ClassFileManager.GetClassFile("java/security/AccessControlContext"));
                    //Heap.AddItem(accessControlContext);

                    JavaHelper.ReturnValue(0);
                    return;
                }
                else if (className == "java/util/concurrent/atomic/AtomicLong" && nameAndDescriptor == ("VMSupportsCS8", "()Z"))
                {
                    JavaHelper.ReturnValue(1);
                    return;
                }
                else if (className == "java/util/TimeZone" && nameAndDescriptor == ("getSystemGMTOffsetID", "()Ljava/lang/String;"))
                {
                    string offsetId = TimeZoneInfo.Local.BaseUtcOffset.ToString();
                    JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(offsetId));
                    return;
                }
                else if (className == "java/util/TimeZone" && nameAndDescriptor == ("getSystemTimeZoneID", "(Ljava/lang/String;)Ljava/lang/String;"))
                {
                    string timeZoneId = TimeZoneInfo.Local.Id;
                    JavaHelper.ReturnValue(JavaHelper.CreateJavaStringLiteral(timeZoneId));
                    return;
                }
                else if (className == "sun/io/Win32ErrorMode" && nameAndDescriptor == ("setErrorMode", "(J)J"))
                {
                    JavaHelper.ReturnLargeValue(0L);
                    return;
                }
                else if (className == "sun/misc/Signal" && nameAndDescriptor == ("findSignal", "(Ljava/lang/String;)I"))
                {
                    int signalNumber = -1;
                    switch (JavaHelper.ReadJavaString(Args[0]))
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

                        //JavaHelper.RunJavaFunction(putMethod, signalsTableRef.Address, signalNumber, Args[])

                    }*/
                    JavaHelper.ReturnValue(signalNumber);
                    return;
                }
                else if (className == "sun/misc/Signal" && nameAndDescriptor == ("handle0", "(IJ)J"))
                {
                    JavaHelper.ReturnLargeValue(0L);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("addressSize", "()I"))
                {
                    JavaHelper.ReturnValue(4);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("allocateMemory", "(J)J"))
                {
                    long size = (Args[1], Args[2]).ToLong();
                    long address = Heap.AllocateMemory(size);
                    JavaHelper.ReturnLargeValue(address);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("arrayBaseOffset", "(Ljava/lang/Class;)I"))
                {
                    //Returns offset of addr of first element from the addr of the array (bytes)
                    JavaHelper.ReturnValue(Heap.ArrayBaseOffset);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("arrayIndexScale", "(Ljava/lang/Class;)I"))
                {
                    //returns num of bytes
                    HeapObject classObject = Heap.GetObject(Args[1]);
                    string itemDescriptor = JavaHelper.ReadJavaString(classObject.GetField("name", "Ljava/lang/String;")).Substring(1);
                    if (itemDescriptor == "D" || itemDescriptor == "J") JavaHelper.ReturnValue(8);
                    else JavaHelper.ReturnValue(4);

                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("compareAndSwapObject", "(Ljava/lang/Object;JLjava/lang/Object;Ljava/lang/Object;)Z"))
                {
                    // Sets first object to x if it currently equals excpected
                    // Returns true if set, false if not set

                    int objAddr = Args[1];
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    int expected = Args[4];
                    int newVal = Args[5];

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
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("compareAndSwapInt", "(Ljava/lang/Object;JII)Z"))
                {
                    int objAddr = Args[1];
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    int expected = Args[4];
                    int newVal = Args[5];

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
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("compareAndSwapLong", "(Ljava/lang/Object;JJJ)Z"))
                {
                    int objAddr = Args[1];
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    long expected = Utility.ToLong((Args[4], Args[5]));
                    long newVal = Utility.ToLong((Args[6], Args[7]));

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
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("copyMemory", "(Ljava/lang/Object;JLjava/lang/Object;JJ)V"))
                {
                    /* todo:
                     * The transfers are in coherent (atomic) units of a size determined
                     * by the address and length parameters.  If the effective addresses and
                     * length are all even modulo 8, the transfer takes place in 'long' units.
                     * If the effective addresses and length are (resp.) even modulo 4 or 2,
                     * the transfer takes place in units of 'int' or 'short'.
                     */
                    int srcObjAddr = Args[1];
                    long srcOffset = Utility.ToLong((Args[2], Args[3]));
                    int destObjAddr = Args[4];
                    long destOffset = Utility.ToLong((Args[5], Args[6]));
                    long numOfBytes = Utility.ToLong((Args[7], Args[8]));

                    Span<byte> srcSpan = Heap.GetSpan(srcObjAddr + (int)srcOffset, (int)numOfBytes);
                    Span<byte> destSpan = Heap.GetSpan(destObjAddr + (int)destOffset, (int)numOfBytes);
                    srcSpan.CopyTo(destSpan);

                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("ensureClassInitialized", "(Ljava/lang/Class;)V"))
                {
                    HeapObject cFileObj = Heap.GetObject(Args[1]);
                    ClassFileManager.InitializeClass(JavaHelper.ReadJavaString(cFileObj.GetField(2)));
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("freeMemory", "(J)V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("getByte", "(J)B"))
                {
                    long address = (Args[1], Args[2]).ToLong();
                    byte value = Heap.GetByte((int)address);
                    JavaHelper.ReturnValue(value);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("getIntVolatile", "(Ljava/lang/Object;J)I"))
                {
                    int baseAddr = Args[1];
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    int val = Heap.GetInt(baseAddr + (int)offset);
                    JavaHelper.ReturnValue(val);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("getLongVolatile", "(Ljava/lang/Object;J)J"))
                {
                    int baseAddr = Args[1];
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    long val = Heap.GetLong(baseAddr + (int)offset);
                    JavaHelper.ReturnLargeValue(val);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("getObjectVolatile", "(Ljava/lang/Object;J)Ljava/lang/Object;"))
                {
                    //todo
                    //https://docs.oracle.com/javase/specs/jvms/se6/html/Threads.doc.html#22258
                    int baseAddr = Args[1];
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    int val = Heap.GetInt(baseAddr + (int)offset);
                    JavaHelper.ReturnValue(val);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("putObjectVolatile", "(Ljava/lang/Object;JLjava/lang/Object;)V"))
                {
                    int baseAddr = Args[1];
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    int objToStoreAddr = Args[4];
                    Heap.PutInt(baseAddr + (int)offset, objToStoreAddr);
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("objectFieldOffset", "(Ljava/lang/reflect/Field;)J"))
                {
                    HeapObject fieldObj = Heap.GetObject(Args[1]);
                    //HeapObject classObj = Heap.GetObject(((FieldReferenceValue)fieldObj.GetField("clazz", "Ljava/lang/Class;")).Address);
                    int slot = fieldObj.GetField("slot", "I");
                    int offset = Heap.ObjectFieldOffset + Heap.ObjectFieldSize * slot;
                    JavaHelper.ReturnLargeValue(offset);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("pageSize", "()I"))
                {
                    JavaHelper.ReturnValue(8);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("putLong", "(JJ)V"))
                {
                    long address = (Args[1], Args[2]).ToLong();
                    long value = (Args[3], Args[4]).ToLong();
                    Heap.PutLong((int)address, value);
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("setMemory", "(Ljava/lang/Object;JJB)V"))
                {
                    int objAddr = Args[1];
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    long bytes = Utility.ToLong((Args[4], Args[5]));
                    byte value = (byte)Args[6];

                    Heap.Fill(objAddr + (int)offset, bytes, value);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("staticFieldBase", "(Ljava/lang/reflect/Field;)Ljava/lang/Object;"))
                {
                    JavaHelper.ReturnValue(0);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("staticFieldOffset", "(Ljava/lang/reflect/Field;)J"))
                {
                    HeapObject fieldObject = Heap.GetObject(Args[1]);

                    int slot = fieldObject.GetField("slot", "I");

                    JavaHelper.ReturnLargeValue(Heap.ObjectFieldSize * slot);
                    return;
                }
                else if (className == "sun/misc/URLClassPath" && nameAndDescriptor == ("getLookupCacheURLs", "(Ljava/lang/ClassLoader;)[Ljava/net/URL;"))
                {
                    JavaHelper.ReturnValue(0);
                    return;
                }
                else if (className == "sun/misc/VM" && nameAndDescriptor == ("initialize", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "sun/nio/ch/FileChannelImpl" && nameAndDescriptor == ("initIDs", "()J"))
                {
                    JavaHelper.ReturnLargeValue(0L);
                    return;
                }
                else if (className == "sun/nio/ch/FileDispatcherImpl" && nameAndDescriptor == ("read0", "(Ljava/io/FileDescriptor;JI)I"))
                {
                    int fileDescriptorAddr = Args[0];
                    long address = (Args[1], Args[2]).ToLong(); //Memory address to write to
                    int length = Args[3];

                    if (fileDescriptorAddr == 0) JavaHelper.ThrowJavaException("java/io/IOException");
                    HeapObject fileDescriptor = Heap.GetObject(fileDescriptorAddr);

                    HeapObject parent = Heap.GetObject(fileDescriptor.GetField("parent", "Ljava/io/Closeable;"));
                    string path = JavaHelper.ReadJavaString(parent.GetField("path", "Ljava/lang/String;"));

                    if (FileStreams.AvailableBytes(path) == 0)
                    {
                        JavaHelper.ReturnValue(-1); //End of file
                        return;
                    }

                    byte[] data = new byte[length];
                    int ret = FileStreams.ReadBytes(path, data.AsSpan());
                    Heap.PutData((int)address, data);

                    JavaHelper.ReturnValue(ret);
                    return;
                }
                else if (className == "sun/nio/ch/IOUtil" && nameAndDescriptor == ("initIDs", "()V"))
                {
                    JavaHelper.ReturnVoid();
                    return;
                }
                else if (className == "sun/nio/ch/IOUtil" && nameAndDescriptor == ("iovMax", "()I"))
                {
                    JavaHelper.ReturnValue(0);
                    return;
                }
                else if (className == "sun/reflect/NativeConstructorAccessorImpl" && nameAndDescriptor == ("newInstance0", "(Ljava/lang/reflect/Constructor;[Ljava/lang/Object;)Ljava/lang/Object;"))
                {
                    int constructorAddr = Args[0];
                    int argsArrAddr = Args[1];

                    HeapObject constructorObj = Heap.GetObject(constructorAddr);

                    //Get args
                    ClassFile constructorClassFile = ClassFileManager.GetClassFile("java/lang/reflect/Constructor");
                    MethodInfo getDeclaringClassMethod = constructorClassFile.MethodDictionary[("getDeclaringClass", "()Ljava/lang/Class;")];
                    JavaHelper.RunJavaFunction(getDeclaringClassMethod, constructorAddr);
                    int declaringClassClassObjAddr = Utility.PopInt(Stack, ref sp);
                    HeapObject declaringClassClassObj = Heap.GetObject(declaringClassClassObjAddr);
                    
                    string declaringClassName = JavaHelper.ReadJavaString(declaringClassClassObj.GetField("name", "Ljava/lang/String;"));
                    int declaringClassCFileIdx = ClassFileManager.GetClassFileIndex(declaringClassName);
                    ClassFile declaringClass = ClassFileManager.ClassFiles[declaringClassCFileIdx];

                    //Get slot
                    MethodInfo getSlotMethod = constructorClassFile.MethodDictionary[("getSlot", "()I")];
                    JavaHelper.RunJavaFunction(getSlotMethod, constructorAddr);
                    int slot = Utility.PopInt(Stack, ref sp);

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
                    JavaHelper.RunJavaFunction(constructorMethod, arguments);
                    JavaHelper.ReturnValue(newObjectAddr);

                    return;
                }
                else if (className == "sun/reflect/Reflection" && nameAndDescriptor == ("getCallerClass", "()Ljava/lang/Class;"))
                {
                    MethodFrame frame = Program.MethodFrameStack.Peek(2);
                    ClassFile callerClass = frame.ClassFile;

                    int classObjAddr = ClassObjectManager.GetClassObjectAddr(callerClass.Name);

                    JavaHelper.ReturnValue(classObjAddr);
                    return;
                }
                else if (className == "sun/reflect/Reflection" && nameAndDescriptor == ("getClassAccessFlags", "(Ljava/lang/Class;)I"))
                {
                    HeapObject classObj = Heap.GetObject(Args[0]);
                    string cFileName = JavaHelper.ReadJavaString(classObj.GetField("name", "Ljava/lang/String;"));
                    ClassFile cFile = ClassFileManager.GetClassFile(cFileName);
                    JavaHelper.ReturnValue(cFile.AccessFlags);
                    return;
                }
                throw new MissingMethodException($"className == \"{className}\" && nameAndDescriptor == (\"{nameAndDescriptor.funcName}\", \"{nameAndDescriptor.descriptor}\")");
            }
            catch (JavaException ex)
            {
                Program.StackTracePrinter.PrintMethodThrewException(MethodInfo, ex);
                if (Program.MethodFrameStack.Count > 1)
                {
                    MethodFrame parentFrame = Program.MethodFrameStack.Peek(1);
                    parentFrame.Stack = new int[parentFrame.Stack.Length];
                    Utility.Push(ref parentFrame.Stack, ref parentFrame.sp, Utility.PopInt(Stack, ref sp)); //push address of exception onto parent stack
                }
                Program.MethodFrameStack.Pop();
                throw;
            }
        }
    }
}
