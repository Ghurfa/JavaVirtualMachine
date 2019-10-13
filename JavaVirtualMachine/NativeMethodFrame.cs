using JavaVirtualMachine.Attributes;
using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
                HeapObject obj = null;
                if (!MethodInfo.HasFlag(MethodInfoFlag.Static)) obj = Heap.GetObject(Args[0]);

                if (className == "java/io/FileDescriptor" && nameAndDescriptor == ("initIDs", "()V"))
                {
                    //???
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/io/FileDescriptor" && nameAndDescriptor == ("set", "(I)J"))
                {
                    int d = Args[0];
                    if (d == 0) //in
                    {
                        Utility.ReturnLargeValue(0L);
                    }
                    else if (d == 1) //out
                    {
                        //temp
                        Utility.ReturnLargeValue(1L);
                    }
                    else if (d == 2) //err
                    {
                        Utility.ReturnLargeValue(2L);
                    }
                    else
                    {
                        Utility.ReturnLargeValue(-1L);
                    }
                    //??
                    return;
                }
                else if (className == "java/io/FileInputStream" && nameAndDescriptor == ("available", "()I"))
                {
                    //system.in fails - todo: fix like java/io/fileinpustream readbytes

                    FieldReferenceValue pathField = (FieldReferenceValue)obj.GetField("path", "Ljava/lang/String;");
                    if (pathField.Address == 0)
                    {
                        int available = FileStreams.AvailableBytesFromConsole();
                        Utility.ReturnValue(available);
                        return;
                    }
                    else
                    {
                        string path = Utility.ReadJavaString((FieldReferenceValue)obj.GetField("path", "Ljava/lang/String;"));
                        int available = FileStreams.AvailableBytes(path);
                        Utility.ReturnValue(available);
                        return;
                    }
                }
                else if (className == "java/io/FileInputStream" && nameAndDescriptor == ("close0", "()V"))
                {
                    string path = Utility.ReadJavaString((FieldReferenceValue)obj.GetField("path", "Ljava/lang/String;"));
                    FileStreams.Close(path);
                    obj.SetField("closed", "Z", 1);
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/io/FileInputStream" && nameAndDescriptor == ("initIDs", "()V"))
                {
                    //???
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/io/FileInputStream" && nameAndDescriptor == ("open0", "(Ljava/lang/String;)V"))
                {
                    string fileName = Utility.ReadJavaString(Args[1]);
                    string[] filePathParts = fileName.Split('\\');
                    try
                    {
                        FileStreams.OpenRead(fileName); //make filestreams class?
                        obj.SetField("path", "Ljava/lang/String;", Args[1]); //not needed?
                    }
                    catch (FileNotFoundException)
                    {
                        Utility.ThrowJavaException("java/io/FileNotFoundException");
                    }
                    Utility.ReturnVoid();
                    //Utility.ThrowJavaException("java/io/FileNotFoundException");
                    return;
                }
                else if (className == "java/io/FileInputStream" && nameAndDescriptor == ("readBytes", "([BII)I"))
                {
                    FieldReferenceValue pathField = (FieldReferenceValue)obj.GetField("path", "Ljava/lang/String;");

                    int byteArrAddr = Args[1];
                    int offset = Args[2];
                    int length = Args[3];

                    if (pathField.Address == 0)
                    {
                        FieldReferenceValue fileDescriptor = (FieldReferenceValue)obj.GetField("fd", "Ljava/io/FileDescriptor;");
                        long handle = ((FieldLargeNumber)Heap.GetObject(fileDescriptor.Address).GetField("handle", "J")).Value;
                        if (handle != 0)
                        {
                            Utility.ThrowJavaException("java/io/IOException");
                        }
                        else
                        {
                            HeapArray javaByteArr = (HeapArray)Heap.GetItem(byteArrAddr);
                            int bytesRead = FileStreams.ReadBytesFromConsole((byte[])javaByteArr.Array, offset, length);

                            Utility.ReturnValue(bytesRead);
                            return;
                        }
                    }
                    else
                    {
                        string path = Utility.ReadJavaString(pathField);
                        HeapArray javaByteArr = (HeapArray)Heap.GetItem(byteArrAddr);

                        int bytesRead = FileStreams.ReadBytes(path, (byte[])javaByteArr.Array, offset, length);

                        Utility.ReturnValue(bytesRead);
                        return;
                    }
                }
                else if (className == "java/io/FileOutputStream" && nameAndDescriptor == ("initIDs", "()V"))
                {
                    //???
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/io/FileOutputStream" && nameAndDescriptor == ("writeBytes", "([BIIZ)V"))
                {
                    HeapArray bytes = (HeapArray)Heap.GetItem(Args[1]);
                    int offset = Args[2];
                    int length = Args[3];
                    bool append = Args[4] != 0;

                    HeapObject fileDescriptor = Heap.GetObject(((FieldReferenceValue)obj.GetField("fd", "Ljava/io/FileDescriptor;")).Address);
                    long handle = ((FieldLargeNumber)fileDescriptor.GetField("handle", "J")).Value; //address (defined in java/io/FileDescriptor set(int))

                    if (handle == 1)
                    {
                        string stringToPrint = Encoding.UTF8.GetString((byte[])bytes.Array, offset, length);
                        Console.Write(stringToPrint);
                    }

                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/io/InputStreamReader" && nameAndDescriptor == ("<init>", "(Ljava/io/InputStream;)V"))
                {
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/io/PrintStream" && nameAndDescriptor == ("newLine", "()V"))
                {
                    Console.WriteLine();
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/io/PrintStream" && nameAndDescriptor == ("println", "(Ljava/lang/String;)V"))
                {
                    HeapObject heapString = Heap.GetObject(Args[1]);
                    FieldReferenceValue fieldArr = (FieldReferenceValue)heapString.GetField("value", "[C");
                    char[] charArr = (char[])((HeapArray)Heap.GetItem(fieldArr.Address)).Array;
                    Console.ResetColor();
                    Console.WriteLine(charArr);
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/io/WinNTFileSystem" && nameAndDescriptor == ("getBooleanAttributes", "(Ljava/io/File;)I"))
                {
                    ClassFile FileCFile = ClassFileManager.GetClassFile("java/io/File");
                    MethodInfo getPathMethod = FileCFile.MethodDictionary[("getPath", "()Ljava/lang/String;")];
                    Utility.RunJavaFunction(getPathMethod, Args[1]);
                    string path = Utility.ReadJavaString(Utility.PopInt(Stack, ref sp));

                    try
                    {
                        FileAttributes attributes = File.GetAttributes(path);
                        int ret = (int)attributes;
                        Utility.ReturnValue(ret);
                        return;
                    }
                    catch (FileNotFoundException)
                    {
                        Utility.ThrowJavaException("java/io/FileNotFoundException");
                    }
                }
                else if (className == "java/io/WinNTFileSystem" && nameAndDescriptor == ("initIDs", "()V"))
                {
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("desiredAssertionStatus0", "(Ljava/lang/Class;)Z"))
                {
                    //What do?
                    Utility.ReturnValue(0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getComponentType", "()Ljava/lang/Class;"))
                {
                    string name = Utility.ReadJavaString(((FieldReferenceValue)obj.GetField("name", "Ljava/lang/String;")).Address);
                    if (!name.StartsWith('['))
                    {
                        Utility.ReturnValue(0);
                    }
                    else
                    {
                        int itemType = ClassObjectManager.GetClassObjectAddr(name.Substring(1));
                        Utility.ReturnValue(itemType);
                    }
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getEnclosingMethod0", "()[Ljava/lang/Object;"))
                {
                    string name = Utility.ReadJavaString(((FieldReferenceValue)obj.GetField("name", "Ljava/lang/String;")).Address);
                    ClassFile cFile = ClassFileManager.GetClassFile(name);
                    foreach (AttributeInfo attribute in cFile.Attributes)
                    {
                        if (attribute is EnclosingMethodAttribute emAttribute)
                        {
                            throw new NotImplementedException();
                        }
                    }
                    Utility.ReturnValue(0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("isArray", "()Z"))
                {
                    string name = Utility.ReadJavaString(((FieldReferenceValue)obj.GetField("name", "Ljava/lang/String;")).Address);
                    bool isArray = name[0] == '[';
                    Utility.ReturnValue(isArray ? 1 : 0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("isPrimitive", "()Z"))
                {
                    string name = Utility.ReadJavaString(((FieldReferenceValue)obj.GetField("name", "Ljava/lang/String;")).Address);
                    bool isPrimitive = name == "boolean" || name == "char" || name == "byte" || name == "short" || name == "int" || name == "long" || name == "float" || name == "double" || name == "void";
                    Utility.ReturnValue(isPrimitive ? 1 : 0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("forName0", "(Ljava/lang/String;ZLjava/lang/ClassLoader;Ljava/lang/Class;)Ljava/lang/Class;"))
                {
                    //supposed to use classloader that is passed in
                    string classToLoadName = Utility.ReadJavaString(Args[0]).Replace('.', '/');
                    int classObjAddr = ClassObjectManager.GetClassObjectAddr(classToLoadName);

                    ClassFileManager.GetClassFile(classToLoadName);
                    if (Args[1] == 1)
                    {
                        ClassFileManager.InitializeClass(classToLoadName);
                    }
                    Utility.ReturnValue(classObjAddr);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getClassLoader0", "()Ljava/lang/ClassLoader;"))
                {
                    Utility.ReturnValue(0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getDeclaredConstructors0", "(Z)[Ljava/lang/reflect/Constructor;"))
                {
                    ClassFile cFile = ClassFileManager.GetClassFile(Utility.ReadJavaString((FieldReferenceValue)obj.GetField("name", "Ljava/lang/String;")));

                    ClassFile constructorClassFile = ClassFileManager.GetClassFile("java/lang/reflect/Constructor");
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
                                string[] parameterDescriptors = descriptorArgs.Split(',');
                                parameterTypes = new int[parameterDescriptors.Length];
                                for (int j = 0; j < parameterTypes.Length; j++)
                                {
                                    parameterTypes[j] = ClassObjectManager.GetClassObjectAddr(parameterDescriptors[j]);
                                }
                            }
                            int parameterTypesArrayAddr = Heap.AddItem(new HeapArray(parameterTypes, ClassObjectManager.GetClassObjectAddr("java/lang/Class")));

                            int checkedExceptionsArrayAddr = 0;
                            if (method.ExceptionsAttribute != null)
                            {
                                CClassInfo[] ExceptionsTable = method.ExceptionsAttribute.ExceptionsTable;
                                int[] checkedExceptions = new int[ExceptionsTable.Length];
                                for (int j = 0; j < checkedExceptions.Length; j++)
                                {
                                    checkedExceptions[j] = ClassObjectManager.GetClassObjectAddr(ExceptionsTable[j].Name);
                                }
                                checkedExceptionsArrayAddr = Heap.AddItem(new HeapArray(checkedExceptions, ClassObjectManager.GetClassObjectAddr("Ljava/lang/Class")));
                            }

                            int modifiers = method.AccessFlags;

                            int slot = i;

                            string signature = descriptor;
                            int signatureObjAddr = Utility.CreateJavaStringLiteral(signature);

                            int constructorObj = Heap.AddItem(new HeapObject(constructorClassFile));

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
                                HeapArray rawAnnotationsArray = new HeapArray(method.RawAnnotations, ClassObjectManager.GetClassObjectAddr("B"));
                                annotationsArrayAddr = Heap.AddItem(rawAnnotationsArray);
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
                                HeapArray rawParameterAnnotationsArray = new HeapArray(method.RawParameterAnnotations, ClassObjectManager.GetClassObjectAddr("B"));
                                parameterAnnotationsArrayAddr = Heap.AddItem(rawParameterAnnotationsArray);
                            }

                            Utility.RunJavaFunction(constructorConstructor, constructorObj,
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

                    HeapArray constructorObjArrayObj = new HeapArray(constructorObjAddresses, ClassObjectManager.GetClassObjectAddr("Ljava/lang/reflect/Constructor;"));
                    Utility.ReturnValue(Heap.AddItem(constructorObjArrayObj));
                    return;

                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getDeclaredFields0", "(Z)[Ljava/lang/reflect/Field;"))
                {
                    bool publicOnly = Args[1] != 0;
                    ClassFile cFile = ClassFileManager.GetClassFile(Utility.ReadJavaString((FieldReferenceValue)obj.GetField("name", "Ljava/lang/String;")));
                    int length;
                    if (publicOnly)
                    {
                        length = 0;
                        foreach (FieldInfo fieldInfo in cFile.FieldsInfo)
                        {
                            if (fieldInfo.HasFlag(FieldInfoFlag.Public))
                            {
                                length++;
                            }
                        }
                    }
                    else
                    {
                        length = cFile.FieldsInfo.Length;
                    }
                    HeapArray fields = new HeapArray(new int[length], ClassObjectManager.GetClassObjectAddr("Ljava/lang/reflect/Field;"));
                    int fieldsArrAddr = Heap.AddItem(fields);
                    int index = 0;
                    int instanceSlot = 0;
                    int staticSlot = 0;
                    for (int i = 0; i < cFile.FieldsInfo.Length; i++)
                    {
                        if (publicOnly && !cFile.FieldsInfo[i].HasFlag(FieldInfoFlag.Public))
                        {
                            continue;
                        }
                        FieldInfo fieldInfo = cFile.FieldsInfo[i];
                        HeapObject field = new HeapObject(ClassFileManager.GetClassFile("java/lang/reflect/Field"));
                        int fieldAddr = Heap.AddItem(field);

                        MethodInfo initMethod = field.ClassFile.MethodDictionary[("<init>", "(Ljava/lang/Class;Ljava/lang/String;Ljava/lang/Class;IILjava/lang/String;[B)V")];

                        string type;
                        if (fieldInfo.Descriptor.Length == 1)
                        {
                            type = Utility.PrimitiveFullName(cFile.FieldsInfo[i].Descriptor);
                        }
                        else type = fieldInfo.Descriptor;

                        int slotArg;
                        if (cFile.FieldsInfo[i].HasFlag(FieldInfoFlag.Static))
                        {
                            slotArg = staticSlot++;
                        }
                        else
                        {
                            slotArg = instanceSlot++;
                        }

                        Utility.RunJavaFunction(initMethod, fieldAddr,
                                                            ClassObjectManager.GetClassObjectAddr(cFile.Name),
                                                            new FieldReferenceValue(Utility.CreateJavaStringLiteral(fieldInfo.Name)).Address,
                                                            ClassObjectManager.GetClassObjectAddr(type),
                                                            cFile.FieldsInfo[i].AccessFlags,
                                                            slotArg,
                                                            new FieldReferenceValue(Utility.CreateJavaStringLiteral(fieldInfo.Descriptor)).Address,
                                                            0);

                        ((int[])fields.Array)[index] = fieldAddr;
                        fields.SetItem(index++, fieldAddr);
                    }
                    Utility.ReturnValue(fieldsArrAddr);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getDeclaringClass0", "()Ljava/lang/Class;"))
                {
                    string name = Utility.ReadJavaString(((FieldReferenceValue)obj.GetField("name", "Ljava/lang/String;")).Address);
                    ClassFile cFile = ClassFileManager.GetClassFile(name);
                    foreach (AttributeInfo attribute in cFile.Attributes)
                    {
                        if (attribute is InnerClassesAttribute icAttribute)
                        {
                            if (icAttribute.Classes.Length != 1)
                            {
                                throw new NotImplementedException();
                            }
                            CClassInfo outerClassInfo = icAttribute.Classes[0].OuterClassInfo;
                            int outerClassObj = ClassObjectManager.GetClassObjectAddr(outerClassInfo);

                            Utility.ReturnValue(outerClassObj);
                            return;
                        }
                    }
                    Utility.ReturnValue(0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getModifiers", "()I"))
                {
                    ClassFile cFile = ClassFileManager.GetClassFile(Utility.ReadJavaString((FieldReferenceValue)obj.GetField("name", "Ljava/lang/String;")));

                    Utility.ReturnValue(cFile.AccessFlags);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getPrimitiveClass", "(Ljava/lang/String;)Ljava/lang/Class;"))
                {
                    string name = Utility.ReadJavaString(Args[0]);
                    int classObjAddr = classObjAddr = ClassObjectManager.GetClassObjectAddr(name);
                    /*switch (name)
                    {
                        case "byte":
                            classObjAddr = ClassObjectManager.GetClassObjectAddr("B");
                            break;
                        case "char":
                            classObjAddr = ClassObjectManager.GetClassObjectAddr("C");
                            break;
                        case "double":
                            classObjAddr = ClassObjectManager.GetClassObjectAddr("D");
                            break;
                        case "float":
                            classObjAddr = ClassObjectManager.GetClassObjectAddr("F");
                            break;
                        case "int":
                            classObjAddr = ClassObjectManager.GetClassObjectAddr("I");
                            break;
                        case "long":
                            classObjAddr = ClassObjectManager.GetClassObjectAddr("J");
                            break;
                        case "short":
                            classObjAddr = ClassObjectManager.GetClassObjectAddr("S");
                            break;
                        case "boolean":
                            classObjAddr = ClassObjectManager.GetClassObjectAddr("Z");
                            break;
                        default:
                            throw new ArgumentException();
                    }*/
                    Utility.ReturnValue(classObjAddr);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("getSuperclass", "()Ljava/lang/Class;"))
                {
                    string descriptor = Utility.ReadJavaString((FieldReferenceValue)obj.GetField(2));
                    if (descriptor == "java.lang.Object" || descriptor.Length == 1 || obj.ClassFile.IsInterface())
                    {
                        Utility.ReturnValue(0);
                        return;
                    }
                    ClassFile cFile = ClassFileManager.GetClassFile(descriptor);
                    ClassFile superClass = cFile.SuperClass;
                    Utility.ReturnValue(ClassObjectManager.GetClassObjectAddr(superClass.Name));
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("isAssignableFrom", "(Ljava/lang/Class;)Z"))
                {
                    if (Args[1] == 0)
                    {
                        Utility.ThrowJavaException("java/lang/NullPointerException");
                    }
                    ClassFile thisCFile = ClassFileManager.GetClassFile(Args[0]);
                    ClassFile otherCFile = ClassFileManager.GetClassFile(Args[1]);
                    bool assignable = Utility.IsSubClassOf(otherCFile, thisCFile);
                    Utility.ReturnValue(assignable ? 1 : 0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("isInstance", "(Ljava/lang/Object;)Z"))
                {
                    HeapObject objToCheck = Heap.GetObject(Args[1]);
                    Utility.ReturnValue(objToCheck.IsInstance(Args[0]) ? 1 : 0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("isInterface", "()Z"))
                {
                    FieldReferenceValue name = (FieldReferenceValue)obj.GetField("name", "Ljava/lang/String;");
                    ClassFile cFile = ClassFileManager.GetClassFile(Utility.ReadJavaString(name));
                    Utility.ReturnValue(cFile.IsInterface() ? 1 : 0);
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Class" && nameAndDescriptor == ("<clinit>", "()V"))
                {
                    //???
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/ClassLoader" && nameAndDescriptor == ("findBuiltinLib", "(Ljava/lang/String;)Ljava/lang/String;"))
                {
                    //temp
                    string name = Utility.ReadJavaString(Args[0]);
                    ClassFile systemCFile = ClassFileManager.GetClassFile("java/lang/System");
                    MethodInfo getPropMethod = systemCFile.MethodDictionary[("getProperty", "(Ljava/lang/String;)Ljava/lang/String;")];
                    Stack = new int[1];
                    Utility.RunJavaFunction(getPropMethod, Utility.CreateJavaStringLiteral("java.library.path"));
                    int libPathAddr = Utility.PopInt(Stack, ref sp);
                    string libPath = Utility.ReadJavaString(libPathAddr);
                    Utility.ReturnValue(Utility.CreateJavaStringLiteral(libPath + name));
                    return;
                    //throw new NotImplementedException();
                }
                else if (className == "java/lang/ClassLoader" && nameAndDescriptor == ("findLoadedClass0", "(Ljava/lang/String;)Ljava/lang/Class;"))
                {
                    Utility.ReturnValue(ClassObjectManager.GetClassObjectAddr(Utility.ReadJavaString(Args[1])));
                    return;
                }
                else if (className == "java/lang/ClassLoader" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/ClassLoader$NativeLibrary" && nameAndDescriptor == ("load", "(Ljava/lang/String;Z)V"))
                {
                    ClassFile classLoaderCFile = ClassFileManager.GetClassFile("java/lang/ClassLoader");
                    int systemNativeLibrariesRef = ((FieldReferenceValue)classLoaderCFile.StaticFieldsDictionary[("systemNativeLibraries", "Ljava/util/Vector;")]).Address;
                    HeapObject systemNativeLibraries = Heap.GetObject(systemNativeLibrariesRef);
                    int loadedLibraryNamesRef = ((FieldReferenceValue)classLoaderCFile.StaticFieldsDictionary[("loadedLibraryNames", "Ljava/util/Vector;")]).Address;
                    HeapObject loadedLibraryNames = Heap.GetObject(loadedLibraryNamesRef);

                    ClassFile vectorCFile = ClassFileManager.GetClassFile("java/util/Vector");
                    MethodInfo addItemToVector = vectorCFile.MethodDictionary[("addElement", "(Ljava/lang/Object;)V")];

                    Utility.RunJavaFunction(addItemToVector, systemNativeLibrariesRef, Args[0]);
                    Utility.RunJavaFunction(addItemToVector, loadedLibraryNamesRef, Args[1]);

                    obj.SetField("loaded", "Z", new FieldNumber(1));

                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Double" && nameAndDescriptor == ("doubleToRawLongBits", "(D)J"))
                {
                    //reversed?
                    //Double is already represented as long
                    Utility.ReturnLargeValue((Args[1], Args[0]).ToLong());
                    return;
                }
                else if (className == "java/lang/Double" && nameAndDescriptor == ("longBitsToDouble", "(J)D"))
                {
                    //reversed?
                    //Double is stored as long
                    Utility.ReturnLargeValue((Args[1], Args[0]).ToLong());
                    return;
                }
                else if (className == "java/lang/Float" && nameAndDescriptor == ("floatToRawIntBits", "(F)I"))
                {
                    //Float is already represented as int
                    Utility.ReturnValue(Args[0]);
                    return;
                }
                else if (className == "java/lang/Object" && nameAndDescriptor == ("<init>", "()V"))
                {
                    Utility.ReturnVoid();
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

                    if (obj is HeapArray originalArr)
                    {
                        Utility.ReturnValue(Heap.AddItem(originalArr.Clone()));
                    }
                    else
                    {
                        Utility.ReturnValue(Heap.AddItem(obj.Clone()));
                    }
                    return;
                }
                else if (className == "java/lang/Object" && nameAndDescriptor == ("getClass", "()Ljava/lang/Class;"))
                {
                    if (obj is HeapArray arr)
                    {
                        int itemTypeAddr = arr.ItemTypeClassObjAddr;
                        FieldReferenceValue nameField = (FieldReferenceValue)(Heap.GetObject(itemTypeAddr)).GetField("name", "Ljava/lang/String;");
                        string itemTypeName = Utility.ReadJavaString(nameField);
                        string arrName = '[' + itemTypeName;
                        Utility.ReturnValue(ClassObjectManager.GetClassObjectAddr(arrName));
                    }
                    else
                    {
                        Utility.ReturnValue(ClassObjectManager.GetClassObjectAddr(obj.ClassFile.Name));
                    }
                    return;
                }
                else if (className == "java/lang/Object" && nameAndDescriptor == ("hashCode", "()I"))
                {
                    //return new int[] {address of object
                    Utility.ReturnValue(Args[0]);
                    return;
                }
                else if (className == "java/lang/Object" && nameAndDescriptor == ("notifyAll", "()V"))
                {
                    //don't support multiple threads
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Object" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    //unsure what natives to register
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/reflect/Array" && nameAndDescriptor == ("newArray", "(Ljava/lang/Class;I)Ljava/lang/Object;"))
                {
                    int typeClassObjAddr = Args[0];
                    int length = Args[1];
                    if (length < 0) Utility.ThrowJavaException("java/lang/NegativeArraySizeException");
                    HeapArray newArr = new HeapArray(new int[length], typeClassObjAddr);
                    Utility.ReturnValue(Heap.AddItem(newArr));
                    return;
                }
                else if (className == "java/lang/Runtime" && nameAndDescriptor == ("availableProcessors", "()I"))
                {
                    Utility.ReturnValue(1);
                    return;
                }
                else if (className == "java/lang/String" && nameAndDescriptor == ("intern", "()Ljava/lang/String;"))
                {
                    Utility.ReturnValue(StringPool.Intern(Args[0]));
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("arraycopy", "(Ljava/lang/Object;ILjava/lang/Object;II)V"))
                {
                    HeapArray srcArr = (HeapArray)Heap.GetItem(Args[0]);
                    int srcStartInd = Args[1];
                    HeapArray destArr = (HeapArray)Heap.GetItem(Args[2]);
                    int destStartInd = Args[3];
                    int count = Args[4];
                    Array.Copy(srcArr.Array, srcStartInd, destArr.Array, destStartInd, count);
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("currentTimeMillis", "()J"))
                {
                    DateTime dt1970 = new DateTime(1970, 1, 1);
                    TimeSpan timeSince = DateTime.Now - dt1970;
                    long totalMillis = (long)timeSince.TotalMilliseconds;
                    Utility.ReturnLargeValue(totalMillis);
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("identityHashCode", "(Ljava/lang/Object;)I"))
                {
                    Utility.ReturnValue(Args[0]);
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("initProperties", "(Ljava/util/Properties;)Ljava/util/Properties;"))
                {
                    HeapObject propertiesObject = Heap.GetObject(Args[0]);
                    Stack = new int[1];

                    //Complete list: https://docs.oracle.com/javase/8/docs/api/java/lang/System.html#getProperties--

                    MethodInfo setPropertyMethod = propertiesObject.ClassFile.MethodDictionary[("setProperty", "(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/Object;")];

                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("java.home"),
                                                                        Utility.CreateJavaStringLiteral(@"C:\Program Files\Java\jdk1.8.0_221\jre")); Utility.PopInt(Stack, ref sp);
                    //Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("java.library.path"),
                    //Utility.CreateJavaStringLiteral(Environment.GetEnvironmentVariable("JAVA_HOME") + "\\bin")); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("file.encoding"),
                                                                        Utility.CreateJavaStringLiteral("UTF16le")); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("os.arch"),
                                                                        Utility.CreateJavaStringLiteral("x64")); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("os.name"),
                                                                        Utility.CreateJavaStringLiteral(Environment.OSVersion.Platform.ToString())); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("os.version"),
                                                                        Utility.CreateJavaStringLiteral(Environment.OSVersion.VersionString)); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("file.separator"),
                                                                        Utility.CreateJavaStringLiteral(Path.DirectorySeparatorChar.ToString())); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("path.separator"),
                                                                        Utility.CreateJavaStringLiteral(Path.PathSeparator.ToString())); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("line.separator"),
                                                                        Utility.CreateJavaStringLiteral(Environment.NewLine)); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("user.name"),
                                                                        Utility.CreateJavaStringLiteral(Environment.UserName)); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("user.home"),
                                                                        Utility.CreateJavaStringLiteral(Environment.GetFolderPath(Environment.SpecialFolder.Personal))); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("user.dir"),
                                                                        Utility.CreateJavaStringLiteral(Program.BaseDirectory)); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("java.library.path"),
                                                                        Utility.CreateJavaStringLiteral(@"C:\Program Files\Java\jdk-12.0.1\lib")); Utility.PopInt(Stack, ref sp);
                    Utility.RunJavaFunction(setPropertyMethod, Args[0], Utility.CreateJavaStringLiteral("sun.lang.ClassLoader.allowArraySyntax"),
                                                                        Utility.CreateJavaStringLiteral("true")); Utility.PopInt(Stack, ref sp);


                    Utility.ReturnValue(Args[0]);
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("mapLibraryName", "(Ljava/lang/String;)Ljava/lang/String;"))
                {
                    string libName = Utility.ReadJavaString(Args[0]);
                    string mappedName = libName.Replace('.', '/');
                    Utility.ReturnValue(Utility.CreateJavaStringLiteral(mappedName));
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("nanoTime", "()J"))
                {
                    long nanoTime = Program.Stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond * 345365667;
                    Utility.ReturnLargeValue(nanoTime);
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    HeapObject printStream = new HeapObject(ClassFileManager.GetClassFile("java/io/PrintStream"));
                    ClassFileManager.GetClassFile("java/lang/System").StaticFieldsDictionary[("out", "Ljava/io/PrintStream;")] = new FieldReferenceValue(Heap.AddItem(printStream));
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("setErr0", "(Ljava/io/PrintStream;)V"))
                {
                    ClassFile systemClassFile = ClassFileManager.GetClassFile("java/lang/System");
                    systemClassFile.StaticFieldsDictionary[("err", "Ljava/io/PrintStream;")] = new FieldReferenceValue(Args[0]);
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("setIn0", "(Ljava/io/InputStream;)V"))
                {
                    ClassFile systemClassFile = ClassFileManager.GetClassFile("java/lang/System");
                    systemClassFile.StaticFieldsDictionary[("in", "Ljava/io/InputStream;")] = new FieldReferenceValue(Args[0]);
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/System" && nameAndDescriptor == ("setOut0", "(Ljava/io/PrintStream;)V"))
                {
                    ClassFile systemClassFile = ClassFileManager.GetClassFile("java/lang/System");
                    systemClassFile.StaticFieldsDictionary[("out", "Ljava/io/PrintStream;")] = new FieldReferenceValue(Args[0]);
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("currentThread", "()Ljava/lang/Thread;"))
                {
                    Utility.ReturnValue(ThreadManager.GetThreadAddr());
                    return;
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("isAlive", "()Z"))
                {
                    FieldNumber threadStatus = (FieldNumber)obj.GetField("threadStatus", "I");
                    if (threadStatus.Value == 0)
                    {
                        Utility.ReturnValue(0);
                        return;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("isInterrupted", "(Z)Z"))
                {
                    FieldNumber threadStatus = (FieldNumber)obj.GetField("threadStatus", "I");
                    if (threadStatus.Value == 2)
                    {
                        Utility.ReturnValue(1);
                        return;
                    }
                    else
                    {
                        Utility.ReturnValue(0);
                        return;
                    }
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("setPriority0", "(I)V"))
                {
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "java/lang/Thread" && nameAndDescriptor == ("start0", "()V"))
                {
                    FieldReferenceValue target = (FieldReferenceValue)obj.GetField("target", "Ljava/lang/Runnable;");
                    if (target.Address == 0)
                    {
                        Utility.ReturnVoid();
                        return;
                    }
                    throw new NotImplementedException();
                }
                else if (className == "java/lang/Throwable" && nameAndDescriptor == ("fillInStackTrace", "(I)Ljava/lang/Throwable;"))
                {
                    //http://hg.openjdk.java.net/jdk7/jdk7/jdk/file/9b8c96f96a0f/src/share/classes/java/lang/Throwable.java
                    Utility.ReturnValue(Args[0]);
                    return;
                }
                else if (className == "java/lang/Throwable" && nameAndDescriptor == ("getStackTraceDepth", "()I"))
                {
                    Utility.ReturnValue(Program.MethodFrameStack.Count);
                    return;
                }
                else if (className == "java/lang/Throwable" && nameAndDescriptor == ("getStackTraceElement", "(I)Ljava/lang/StackTraceElement;"))
                {
                    int index = Args[1];

                    ClassFile stackTraceElementCFile = ClassFileManager.GetClassFile("java/lang/StackTraceElement");
                    HeapObject stackTraceElement = new HeapObject(stackTraceElementCFile);
                    int objAddr = Heap.AddItem(stackTraceElement);

                    MethodInfo constructor = stackTraceElementCFile.MethodDictionary[("<init>", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;I)V")];

                    MethodFrame frame = Program.MethodFrameStack.Peek(index);
                    int declaringClass = Utility.CreateJavaStringLiteral(frame.ClassFile.Name);
                    int methodName = Utility.CreateJavaStringLiteral(frame.MethodInfo.Name);
                    int fileName = 0;
                    int lineNumber = frame is NativeMethodFrame ? -2 : -1;

                    Utility.RunJavaFunction(constructor, objAddr,
                                                            declaringClass,
                                                            methodName,
                                                            fileName,
                                                            lineNumber);
                    Utility.ReturnValue(objAddr);
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

                    Utility.RunJavaFunction(method, Args);
                    //methodFrame.Execute returns to this NativeMethodFrame's stack
                    Utility.ReturnValue(Utility.PopInt(Stack, ref sp));
                    return;
                }
                else if (className == "java/security/AccessController" && nameAndDescriptor == ("doPrivileged", "(Ljava/security/PrivilegedAction;Ljava/security/AccessControlContext;)Ljava/lang/Object;"))
                {
                    Stack = new int[1];
                    HeapObject privilegedAction = Heap.GetObject(Args[0]);
                    MethodInfo method = privilegedAction.ClassFile.MethodDictionary[("run", "()Ljava/lang/Object;")];

                    Utility.RunJavaFunction(method, Args[0]);
                    Utility.ReturnValue(Utility.PopInt(Stack, ref sp));
                    return;
                }
                else if (className == "java/security/AccessController" && nameAndDescriptor == ("doPrivileged", "(Ljava/security/PrivilegedExceptionAction;)Ljava/lang/Object;"))
                {
                    Stack = new int[1];
                    HeapObject privilegedExceptionAction = Heap.GetObject(Args[0]);
                    MethodInfo method = privilegedExceptionAction.ClassFile.MethodDictionary[("run", "()Ljava/lang/Object;")];
                    //DebugWriter.CallFuncDebugWrite(privilegedExceptionAction.ClassObject.Name, "run", Args);
                    Utility.RunJavaFunction(method, Args);
                    //methodFrame.Execute returns to this NativeMethodFrame's stack
                    Utility.ReturnValue(Utility.PopInt(Stack, ref sp));
                    return;
                }
                else if (className == "java/security/AccessController" && nameAndDescriptor == ("getStackAccessControlContext", "()Ljava/security/AccessControlContext;"))
                {
                    //HeapObject accessControlContext = new HeapObject(ClassFileManager.GetClassFile("java/security/AccessControlContext"));
                    //Heap.AddItem(accessControlContext);

                    Utility.ReturnValue(0);
                    return;
                }
                else if (className == "java/util/concurrent/atomic/AtomicLong" && nameAndDescriptor == ("VMSupportsCS8", "()Z"))
                {
                    Utility.ReturnValue(1);
                    return;
                }
                else if (className == "java/util/TimeZone" && nameAndDescriptor == ("getSystemGMTOffsetID", "()Ljava/lang/String;"))
                {
                    string offsetId = TimeZoneInfo.Local.BaseUtcOffset.ToString();
                    Utility.ReturnValue(Utility.CreateJavaStringLiteral(offsetId));
                    return;
                }
                else if (className == "java/util/TimeZone" && nameAndDescriptor == ("getSystemTimeZoneID", "(Ljava/lang/String;)Ljava/lang/String;"))
                {
                    string timeZoneId = TimeZoneInfo.Local.Id;
                    Utility.ReturnValue(Utility.CreateJavaStringLiteral(timeZoneId));
                    return;
                }
                else if (className == "sun/io/Win32ErrorMode" && nameAndDescriptor == ("setErrorMode", "(J)J"))
                {
                    Utility.ReturnLargeValue(0L);
                    return;
                }
                else if (className == "sun/misc/Signal" && nameAndDescriptor == ("findSignal", "(Ljava/lang/String;)I"))
                {
                    int signalNumber = -1;
                    switch (Utility.ReadJavaString(Args[0]))
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

                        //Utility.RunJavaFunction(putMethod, signalsTableRef.Address, signalNumber, Args[])

                    }*/
                    Utility.ReturnValue(signalNumber);
                    return;
                }
                else if (className == "sun/misc/Signal" && nameAndDescriptor == ("handle0", "(IJ)J"))
                {
                    Utility.ReturnLargeValue(0L);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("addressSize", "()I"))
                {
                    Utility.ReturnValue(4);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("allocateMemory", "(J)J"))
                {
                    long size = (Args[1], Args[2]).ToLong();
                    long address = Heap.AllocateMemory(size);
                    Utility.ReturnLargeValue(address);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("arrayBaseOffset", "(Ljava/lang/Class;)I"))
                {
                    //Returns offset of addr of first element from the addr of the array (bytes)
                    Utility.ReturnValue(HeapArray.ArrayBaseOffset);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("arrayIndexScale", "(Ljava/lang/Class;)I"))
                {
                    //returns num of bytes
                    HeapObject classObject = Heap.GetObject(Args[1]);
                    string itemDescriptor = Utility.ReadJavaString((FieldReferenceValue)classObject.GetField("name", "Ljava/lang/String;")).Substring(1);
                    if (itemDescriptor == "D" || itemDescriptor == "J") Utility.ReturnValue(8);
                    else Utility.ReturnValue(4);

                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("compareAndSwapObject", "(Ljava/lang/Object;JLjava/lang/Object;Ljava/lang/Object;)Z"))
                {
                    // Sets first object to x if it currently equals excpected
                    // Returns true if set, false if not set

                    HeapObject o = Heap.GetObject(Args[1]);
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    int expected = Args[4];
                    int x = Args[5];
                    if (o is HeapArray arr)
                    {
                        if (arr.GetItemDataByOffset(offset) == expected)
                        {
                            ((int[])arr.Array)[(offset - HeapArray.ArrayBaseOffset) / arr.ItemSize] = x;
                            arr.SetItemByOffset(offset, x);
                            Utility.ReturnValue(1);
                            return;
                        }
                        Utility.ReturnValue(0);
                        return;
                    }
                    else
                    {
                        FieldReferenceValue field = (FieldReferenceValue)o.GetFieldByOffset(offset);
                        if (field.Address == expected)
                        {
                            o.SetFieldByOffset(offset, new FieldReferenceValue(x));
                            Utility.ReturnValue(1);
                            return;
                        }
                        Utility.ReturnValue(0);
                        return;
                    }

                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("compareAndSwapInt", "(Ljava/lang/Object;JII)Z"))
                {
                    HeapObject o = Heap.GetObject(Args[1]);
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    int expected = Args[4];
                    int x = Args[5];
                    if (o is HeapArray arr)
                    {
                        if (arr.GetItemDataByOffset(offset) == expected)
                        {
                            ((int[])arr.Array)[(offset - HeapArray.ArrayBaseOffset) / arr.ItemSize] = x;
                            arr.SetItemByOffset(offset, x);
                            Utility.ReturnValue(1);
                            return;
                        }
                        Utility.ReturnValue(0);
                        return;
                    }
                    else
                    {
                        FieldNumber field = (FieldNumber)o.GetFieldByOffset(offset);
                        if (field.Value == expected)
                        {
                            o.SetFieldByOffset(offset, new FieldNumber(x));
                            Utility.ReturnValue(1);
                            return;
                        }
                        Utility.ReturnValue(0);
                        return;
                    }
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("compareAndSwapLong", "(Ljava/lang/Object;JJJ)Z"))
                {
                    HeapObject o = Heap.GetObject(Args[1]);
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    long expected = Utility.ToLong((Args[4], Args[5]));
                    long x = Utility.ToLong((Args[6], Args[7]));
                    if (o is HeapArray arr)
                    {
                        if (arr.GetItemData((int)offset) == expected)
                        {
                            ((long[])arr.Array)[(offset - HeapArray.ArrayBaseOffset) / arr.ItemSize] = x;
                            arr.SetItemByOffset(offset, x);
                            Utility.ReturnValue(1);
                            return;
                        }
                        Utility.ReturnValue(0);
                        return;
                    }
                    else
                    {
                        FieldLargeNumber field = (FieldLargeNumber)o.GetFieldByOffset(offset);
                        if (field.Value == expected)
                        {
                            o.SetFieldByOffset(offset, new FieldLargeNumber(x));
                            Utility.ReturnValue(1);
                            return;
                        }
                        Utility.ReturnValue(0);
                        return;
                    }
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
                    HeapObject srcObj = Heap.GetObject(Args[1]);
                    long srcOffset = Utility.ToLong((Args[2], Args[3]));
                    HeapObject destObj = Heap.GetObject(Args[4]);
                    long destOffset = Utility.ToLong((Args[5], Args[6]));
                    long numOfBytes = Utility.ToLong((Args[7], Args[8]));

                    byte[] srcData;
                    if (srcObj == null)
                    {
                        srcData = Heap.GetMemorySlice((int)srcOffset, (int)numOfBytes).ToArray();
                    }
                    else if (srcObj is HeapArray arr)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    if (destObj == null)
                    {
                        Heap.PutData(destOffset, srcData);
                    }
                    else if (destObj is HeapArray arr)
                    {
                        arr.SetDataByOffset(srcData, (int)destOffset, (int)numOfBytes);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("ensureClassInitialized", "(Ljava/lang/Class;)V"))
                {
                    HeapObject cFileObj = Heap.GetObject(Args[1]);
                    ClassFileManager.InitializeClass(Utility.ReadJavaString((FieldReferenceValue)cFileObj.GetField(2)));
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("freeMemory", "(J)V"))
                {
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("getByte", "(J)B"))
                {
                    long address = (Args[1], Args[2]).ToLong();
                    byte value = Heap.GetMemorySlice((int)address, 1).ToArray()[0];
                    Utility.ReturnValue(value);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("getIntVolatile", "(Ljava/lang/Object;J)I"))
                {
                    //todo: volatile
                    HeapObject o = Heap.GetObject(Args[1]);
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    if (o == null)
                    {
                        Utility.ReturnValue(Heap.GetInt((int)offset));
                    }
                    else if (o is HeapArray arr)
                    {
                        int[] array = (int[])arr.Array;
                        Utility.ReturnValue(array[(offset - HeapArray.ArrayBaseOffset) / arr.ItemSize]); //offset is offset in memory, including array info before data
                    }
                    else
                    {
                        Utility.ReturnValue(((FieldNumber)o.GetFieldByOffset(offset)).Value);
                    }
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("getLongVolatile", "(Ljava/lang/Object;J)J"))
                {
                    HeapObject o = Heap.GetObject(Args[1]);
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    if (o == null)
                    {
                        Utility.ReturnLargeValue(Heap.GetLong((int)offset));
                    }
                    else if (o is HeapArray arr)
                    {
                        long[] array = (long[])arr.Array;
                        Utility.ReturnLargeValue(array[(offset - HeapArray.ArrayBaseOffset) / arr.ItemSize]); //offset is offset in memory, including array info before data
                    }
                    else
                    {
                        Utility.ReturnLargeValue(((FieldLargeNumber)o.GetFieldByOffset(offset)).Value);
                    }
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("getObjectVolatile", "(Ljava/lang/Object;J)Ljava/lang/Object;"))
                {
                    //todo
                    //https://docs.oracle.com/javase/specs/jvms/se6/html/Threads.doc.html#22258
                    int objAddr = Args[1];
                    long offset = (Args[2], Args[3]).ToLong();
                    HeapObject o = Heap.GetObject(objAddr);

                    if (o == null)
                    {
                        Utility.ReturnValue((int)offset); //return heap object at address of offset
                    }
                    else if (o is HeapArray arr)
                    {
                        Utility.ReturnValue(arr.GetItemDataByOffset(offset)); //offset is offset in memory, including array info before data
                    }
                    else
                    {
                        Utility.ReturnValue(((FieldReferenceValue)o.GetFieldByOffset(offset)).Address);
                    }
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("putObjectVolatile", "(Ljava/lang/Object;JLjava/lang/Object;)V"))
                {
                    int objAddr = Args[1];
                    long offset = (Args[2], Args[3]).ToLong();
                    int objToStoreAddr = Args[4];

                    HeapObject objToStoreIn = Heap.GetObject(objAddr);

                    if (objToStoreIn == null)
                    {
                        Heap.PutLong(offset, objToStoreAddr);
                    }
                    else if (objToStoreIn is HeapArray arr)
                    {
                        arr.SetItemByOffset(offset, objToStoreAddr);
                    }
                    else
                    {
                        objToStoreIn.SetFieldByOffset(offset, new FieldReferenceValue(objToStoreAddr));
                    }
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("objectFieldOffset", "(Ljava/lang/reflect/Field;)J"))
                {
                    HeapObject fieldObj = Heap.GetObject(Args[1]);
                    //HeapObject classObj = Heap.GetObject(((FieldReferenceValue)fieldObj.GetField("clazz", "Ljava/lang/Class;")).Address);
                    int slot = ((FieldNumber)fieldObj.GetField("slot", "I")).Value;
                    Utility.ReturnLargeValue((HeapObject.FieldOffset + HeapObject.FieldSize * slot));
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("pageSize", "()I"))
                {
                    Utility.ReturnValue(8);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("putLong", "(JJ)V"))
                {
                    long address = (Args[1], Args[2]).ToLong();
                    long value = (Args[3], Args[4]).ToLong();
                    Memory<byte> slice = Heap.GetMemorySlice((int)address, 8);
                    value.AsByteArray().CopyTo(slice);
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("registerNatives", "()V"))
                {
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("setMemory", "(Ljava/lang/Object;JJB)V"))
                {
                    HeapObject o = Heap.GetObject(Args[1]);
                    long offset = Utility.ToLong((Args[2], Args[3]));
                    long bytes = Utility.ToLong((Args[4], Args[5]));
                    byte value = (byte)Args[6];
                    if (o == null)
                    {
                        Heap.Fill(offset, bytes, value);
                        Utility.ReturnVoid();
                    }
                    else if (o is HeapArray arr)
                    {
                        throw new NotImplementedException();
                        long[] array = (long[])arr.Array;
                        long index = (offset - HeapArray.ArrayBaseOffset) / arr.ItemSize;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("staticFieldBase", "(Ljava/lang/reflect/Field;)Ljava/lang/Object;"))
                {
                    Utility.ReturnValue(0);
                    return;
                }
                else if (className == "sun/misc/Unsafe" && nameAndDescriptor == ("staticFieldOffset", "(Ljava/lang/reflect/Field;)J"))
                {
                    HeapObject fieldObject = Heap.GetObject(Args[1]);

                    FieldValue slotFieldVal = fieldObject.GetField("slot", "I");
                    int slot = ((FieldNumber)slotFieldVal).Value;

                    Utility.ReturnLargeValue(HeapObject.FieldSize * slot);
                    return;
                }
                else if (className == "sun/misc/URLClassPath" && nameAndDescriptor == ("getLookupCacheURLs", "(Ljava/lang/ClassLoader;)[Ljava/net/URL;"))
                {
                    Utility.ReturnValue(0);
                    return;
                }
                else if (className == "sun/misc/VM" && nameAndDescriptor == ("initialize", "()V"))
                {
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "sun/nio/ch/FileChannelImpl" && nameAndDescriptor == ("initIDs", "()J"))
                {
                    Utility.ReturnLargeValue(0L);
                    return;
                }
                else if (className == "sun/nio/ch/FileDispatcherImpl" && nameAndDescriptor == ("read0", "(Ljava/io/FileDescriptor;JI)I"))
                {
                    HeapObject fileDescriptor = Heap.GetObject(Args[0]);
                    if (fileDescriptor == null) Utility.ThrowJavaException("java/io/IOException");
                    long address = (Args[1], Args[2]).ToLong(); //Memory address to write to
                    int length = Args[3];
                    HeapObject parent = Heap.GetObject(((FieldReferenceValue)fileDescriptor.GetField("parent", "Ljava/io/Closeable;")).Address);
                    string path = Utility.ReadJavaString((FieldReferenceValue)parent.GetField("path", "Ljava/lang/String;"));

                    if (FileStreams.AvailableBytes(path) == 0)
                    {
                        Utility.ReturnValue(-1); //End of file
                        return;
                    }

                    byte[] data = new byte[length];
                    int ret = FileStreams.ReadBytes(path, data, 0, length);
                    Heap.PutData(address, data);

                    Utility.ReturnValue(ret);
                    return;
                }
                else if (className == "sun/nio/ch/IOUtil" && nameAndDescriptor == ("initIDs", "()V"))
                {
                    Utility.ReturnVoid();
                    return;
                }
                else if (className == "sun/nio/ch/IOUtil" && nameAndDescriptor == ("iovMax", "()I"))
                {
                    Utility.ReturnValue(0);
                    return;
                }
                else if (className == "sun/reflect/NativeConstructorAccessorImpl" && nameAndDescriptor == ("newInstance0", "(Ljava/lang/reflect/Constructor;[Ljava/lang/Object;)Ljava/lang/Object;"))
                {
                    int constructorAddr = Args[0];
                    int argsArrAddr = Args[1];

                    HeapObject constructorObj = Heap.GetObject(constructorAddr);

                    ClassFile constructorClassFile = ClassFileManager.GetClassFile("java/lang/reflect/Constructor");
                    MethodInfo getDeclaringClassMethod = constructorClassFile.MethodDictionary[("getDeclaringClass", "()Ljava/lang/Class;")];
                    Utility.RunJavaFunction(getDeclaringClassMethod, constructorAddr);
                    int declaringClassClassObjAddr = Utility.PopInt(Stack, ref sp);
                    HeapObject declaringClassClassObj = Heap.GetObject(declaringClassClassObjAddr);
                    FieldReferenceValue declaringClassName = (FieldReferenceValue)declaringClassClassObj.GetField("name", "Ljava/lang/String;");
                    ClassFile declaringClass = ClassFileManager.GetClassFile(Utility.ReadJavaString(declaringClassName));

                    MethodInfo getSlotMethod = constructorClassFile.MethodDictionary[("getSlot", "()I")];
                    Utility.RunJavaFunction(getSlotMethod, constructorAddr);
                    int slot = Utility.PopInt(Stack, ref sp);

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

                    HeapObject newObject = new HeapObject(declaringClass);
                    int newObjectAddr = Heap.AddItem(newObject);

                    int[] arguments;
                    if (argsArrAddr == 0)
                    {
                        arguments = new int[] { newObjectAddr };
                    }
                    else
                    {
                        HeapArray argsArr = (HeapArray)Heap.GetItem(argsArrAddr);
                        int[] args = (int[])argsArr.Array;
                        arguments = new int[args.Length + 1];
                        arguments[0] = newObjectAddr;
                        args.CopyTo(arguments, 1);
                    }

                    Utility.RunJavaFunction(constructorMethod, arguments);
                    Utility.ReturnValue(newObjectAddr);

                    return;
                }
                else if (className == "sun/reflect/Reflection" && nameAndDescriptor == ("getCallerClass", "()Ljava/lang/Class;"))
                {
                    MethodFrame frame = Program.MethodFrameStack.Peek(2);
                    ClassFile callerClass = frame.ClassFile;

                    int classObjAddr = ClassObjectManager.GetClassObjectAddr(callerClass.Name);

                    Utility.ReturnValue(classObjAddr);
                    return;
                }
                else if (className == "sun/reflect/Reflection" && nameAndDescriptor == ("getClassAccessFlags", "(Ljava/lang/Class;)I"))
                {
                    HeapObject classObj = Heap.GetObject(Args[0]);
                    string cFileName = Utility.ReadJavaString(((FieldReferenceValue)classObj.GetField("name", "Ljava/lang/String;")).Address);
                    ClassFile cFile = ClassFileManager.GetClassFile(cFileName);
                    Utility.ReturnValue(cFile.AccessFlags);
                    return;
                }
                throw new MissingMethodException($"className == \"{className}\" && nameAndDescriptor == (\"{nameAndDescriptor.funcName}\", \"{nameAndDescriptor.descriptor}\")");
            }
            catch (JavaException ex)
            {
                //No exception handling (yet)
                DebugWriter.ExceptionThrownDebugWrite(ex);
                if (Program.MethodFrameStack.Count > 1)
                {
                    MethodFrame parentFrame = Program.MethodFrameStack.Peek(1);
                    parentFrame.Stack = new int[parentFrame.Stack.Length];
                    Utility.Push(ref parentFrame.Stack, ref parentFrame.sp, Utility.PopInt(Stack, ref sp)); //push address of exception onto parent stack
                }
                Program.MethodFrameStack.Pop();
                throw ex;
            }
        }
    }
}
