using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JavaVirtualMachine
{
    public static class ClassFileManager
    {
        private static Dictionary<string, (string path, ClassFile classFile, bool isStaticLoaded)> classFiles;
        private static string javaClassesFilePath;
        public static void InitDictionary(string runtimePath, params string[] otherPaths)
        {
            classFiles = new Dictionary<string, (string, ClassFile, bool)>();
            foreach (string path in otherPaths)
            {
                string[] files = Directory.GetFiles(path, "*.class", SearchOption.AllDirectories);
                foreach (string filePath in files)
                {
                    string fileName = filePath.Substring(path.Length);
                    fileName = fileName.Substring(0, fileName.Length - 6); //Remove ".class"
                    classFiles.Add(fileName.Replace('\\', '/'), (path, null, false));
                }
            }
            ClassFileManager.javaClassesFilePath = runtimePath;
        }
        public static ClassFile GetClassFile(CClassInfo cClassInfo)
        {
            return GetClassFile(cClassInfo.Name);
        }
        public static ClassFile GetClassFile(int classObjAddr)
        {
            HeapObject classObj = Heap.GetObject(classObjAddr);
            FieldReferenceValue nameRef = (FieldReferenceValue)classObj.GetField(2);
            string name = JavaHelper.ReadJavaString(nameRef);
            return GetClassFile(name);
        }
        public static ClassFile GetClassFile(string key)
        {
            if (key.Last() == ';')
                key = key.Substring(1, key.Length - 2);
            key = key.Replace('.', '/');

            //todo: check permissions
            classFiles.TryGetValue(key, out (string path, ClassFile classFile, bool isStaticLoaded) classFileEntry);

            try
            {
                if (classFileEntry.path == null)
                {
                    //Java class file
                    string filePath = javaClassesFilePath + key + ".class";
                    classFiles.Add(key, (javaClassesFilePath, new ClassFile(filePath), false));
                    return classFiles[key].classFile;
                }
                else
                {
                    bool isNull = classFileEntry.classFile == null;
                    if (isNull)
                    {
                        classFiles[key] = (classFiles[key].path, new ClassFile(classFiles[key].path + key + ".class"), false);
                    }
                    return classFiles[key].classFile;
                }

            }
            catch (DirectoryNotFoundException)
            {
                ClassFile exceptionClassFile = GetClassFile("java/lang/ClassNotFoundException");
                HeapObject exception = new HeapObject(exceptionClassFile);
                MethodInfo initMethodInfo = exceptionClassFile.MethodDictionary[("<init>", "(Ljava/lang/String;)V")];
                int exceptionAddr = Heap.AddItem(exception);
                int messageAddr = JavaHelper.CreateJavaStringLiteral($"Could not find class {key}");
                JavaHelper.RunJavaFunction(initMethodInfo, exceptionAddr, messageAddr);
                if (Program.MethodFrameStack.Count > 0)
                {
                    MethodFrame parentFrame = Program.MethodFrameStack.Peek();
                    Utility.Push(ref parentFrame.Stack, ref parentFrame.sp, exceptionAddr); //push address of exception onto parent stack
                }
                throw new JavaException(exception.ClassFile, $"Could not find class {key}");
            }
        }
        public static void InitializeClass(string key)
        {
            if(key == "java/lang/ref/SoftReference")
            {

            }
            ClassFile cFile = GetClassFile(key);
            while (cFile != null)
            {
                if (!classFiles[cFile.Name].isStaticLoaded)
                {
                    classFiles[cFile.Name] = (classFiles[cFile.Name].path, cFile, true);
                    if (classFiles[cFile.Name].classFile.MethodDictionary.TryGetValue(("<clinit>", "()V"), out MethodInfo classInitMethod))
                    {
                        JavaHelper.RunJavaFunction(classInitMethod);
                    }
                }
                cFile = cFile.SuperClass;
                if(cFile != null && !classFiles[cFile.Name].isStaticLoaded && classFiles[cFile.Name].classFile.MethodDictionary.ContainsKey(("<clinit>", "()V")))
                {

                }
            }
        }
    }
}
