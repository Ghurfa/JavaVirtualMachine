﻿using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine
{
    public static class ClassFileManager
    {
        private static Dictionary<string, (string path, int classFileIndex, bool isStaticLoaded)> StrToClassFileInfo;
        private static List<ClassFile> _classFiles = new();
        public static IReadOnlyList<ClassFile> ClassFiles => _classFiles;

        private static string RuntimePath;

        public static void InitDictionary(string runtimePath, params string[] otherPaths)
        {
            StrToClassFileInfo = new Dictionary<string, (string, int, bool)>();
            foreach (string path in otherPaths)
            {
                string[] files = Directory.GetFiles(path, "*.class", SearchOption.AllDirectories);
                foreach (string filePath in files)
                {
                    string fileName = filePath.Substring(path.Length);
                    fileName = fileName.Substring(0, fileName.Length - 6); //Remove ".class"
                    StrToClassFileInfo.Add(fileName.Replace('\\', '/'), (path, -1, false));
                }
            }
            RuntimePath = runtimePath;
        }

        public static int GetClassFileIndex(CClassInfo cClassInfo)
        {
            return GetClassFileIndex(cClassInfo.Name);
        }

        public static int GetClassFileIndex(int classObjAddr)
        {
            HeapObject classObj = Heap.GetObject(classObjAddr);
            string name = JavaHelper.ReadJavaString(classObj.GetField(2));
            return GetClassFileIndex(name);
        }

        public static int GetClassFileIndex(string key)
        {
            if (key.Last() == ';')
                key = key.Substring(1, key.Length - 2);
            key = key.Replace('.', '/');

            //todo: check permissions

            try
            {
                if (StrToClassFileInfo.TryGetValue(key, out (string path, int classFileIndex, bool isStaticLoaded) classFileEntry))
                {
                    bool isLoaded = classFileEntry.classFileIndex != -1;
                    if (!isLoaded)
                    {
                        ClassFile classFile = new ClassFile(StrToClassFileInfo[key].path + key + ".class");
                        _classFiles.Add(classFile);

                        int index = _classFiles.Count - 1;
                        StrToClassFileInfo[key] = (RuntimePath, index, false);
                        return index;
                    }
                    else
                    {
                        return classFileEntry.classFileIndex;
                    }
                }
                else
                {
                    //Java class file
                    string filePath = RuntimePath + key + ".class";
                    ClassFile classFile = new ClassFile(filePath);
                    _classFiles.Add(classFile);

                    int index = _classFiles.Count - 1;
                    StrToClassFileInfo.Add(key, (RuntimePath, index, false));
                    return index;
                }
            }
            catch (DirectoryNotFoundException)
            {
                int exCFileIdx = GetClassFileIndex("java/lang/ClassNotFoundException");
                int exAddr = Heap.CreateObject(exCFileIdx);
                ClassFile exCFile = ClassFiles[exCFileIdx];

                MethodInfo initMethodInfo = exCFile.MethodDictionary[("<init>", "(Ljava/lang/String;)V")];
                int messageAddr = JavaHelper.CreateJavaStringLiteral($"Could not find class {key}");
                JavaHelper.RunJavaFunction(initMethodInfo, exAddr, messageAddr);
                if (Program.MethodFrameStack.Count > 0)
                {
                    MethodFrame parentFrame = Program.MethodFrameStack.Peek();
                    Utility.Push(ref parentFrame.Stack, ref parentFrame.sp, exAddr); //push address of exception onto parent stack
                }
                throw new JavaException(exCFile, $"Could not find class {key}");
            }
        }

        public static ClassFile GetClassFile(string name)
        {
            return ClassFiles[GetClassFileIndex(name)];
        }

        public static void InitializeClass(string key)
        {
            ClassFile cFile = GetClassFile(key);
            while (cFile != null)
            {
                if (!StrToClassFileInfo[cFile.Name].isStaticLoaded)
                {
                    StrToClassFileInfo[cFile.Name] = (StrToClassFileInfo[cFile.Name].path, StrToClassFileInfo[cFile.Name].classFileIndex, true);
                    
                    if (cFile.MethodDictionary.TryGetValue(("<clinit>", "()V"), out MethodInfo classInitMethod))
                    {
                        JavaHelper.RunJavaFunction(classInitMethod);
                    }
                }
                cFile = cFile.SuperClass;
            }
        }
    }
}
