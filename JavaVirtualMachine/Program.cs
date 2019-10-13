using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace JavaVirtualMachine
{
    public class Program
    {
        public static Stack<MethodFrame> MethodFrameStack = new Stack<MethodFrame>();
        public static string BaseDirectory = @"C:\Users\Lorenzo.Lopez\LocalFolder\GradleProject\";
        public static Stopwatch Stopwatch = new Stopwatch();
        static void Main(string[] args)
        {
            Stopwatch.Start();

            //%JAVA_HOME%\bin\javap" - s -p -c -verbose Scanner.class > ..\..\..\Scanner.javap
            ClassFileManager.InitDictionary(@"\\GMRDC1\Folder Redirection\Lorenzo.Lopez\Desktop\rt\",
                                            BaseDirectory + @"build\classes\java\main\");

            //Create main thread object
            ClassFile threadGroupCFile = ClassFileManager.GetClassFile("java/lang/ThreadGroup");
            HeapObject threadGroupObj = new HeapObject(threadGroupCFile);
            int threadGroupAddr = Heap.AddItem(threadGroupObj);

            ClassFile threadCFile = ClassFileManager.GetClassFile("java/lang/Thread");
            HeapObject threadObj = new HeapObject(threadCFile);
            ThreadManager.ThreadAddr = Heap.AddItem(threadObj);

            //MethodInfo threadInit = threadCFile.MethodDictionary[("<init>", "(Ljava/lang/String;)V")];
            //Utility.RunJavaFunction(threadInit, ThreadManager.ThreadAddr, Utility.CreateJavaStringLiteral("main"));
            threadObj.SetField("group", "Ljava/lang/ThreadGroup;", threadGroupAddr);
            threadObj.SetField("priority", "I", 5);

            ClassFile systemCFile = ClassFileManager.GetClassFile("java/lang/System");
            MethodInfo initSystemClassMethod = systemCFile.MethodDictionary[("initializeSystemClass", "()V")];
            try
            {
                Utility.RunJavaFunction(initSystemClassMethod);
            }
            catch(JavaException ex)
            {
                if (ex.ClassFile.Name != "java/lang/IllegalStateException") throw ex;
            }

            ClassFile mainProg = ClassFileManager.GetClassFile("Program");
            int mainProgObjAddr = Heap.AddItem(new HeapObject(mainProg));
            MethodInfo mainProgInit = mainProg.MethodDictionary[("<init>", "()V")];
            try
            {
                Utility.RunJavaFunction(mainProgInit, mainProgObjAddr);
            }
            catch (JavaException ex)
            {
                DebugWriter.WriteDebugMessage($"Program ended with {ex.ClassFile.Name} ({ex.Message})");
                DebugWriter.PrintStack(ex.Stack);
            }
            
            foreach (var method in mainProg.MethodDictionary)
            {
                if (method.Key == ("<clinit>", "()V"))
                {
                    try
                    {
                        Utility.RunJavaFunction(method.Value, mainProgObjAddr);
                    }
                    catch (JavaException ex)
                    {
                        DebugWriter.WriteDebugMessage($"Program ended with {ex.ClassFile.Name} ({ex.Message})");
                        DebugWriter.PrintStack(ex.Stack);
                    }
                    break;
                }
            }

            foreach (var method in mainProg.MethodDictionary)
            {
                if (method.Key.name == "main")
                {
                    try
                    {
                        Utility.RunJavaFunction(method.Value, mainProgObjAddr);
                        Console.WriteLine("End of program");
                    }
                    catch (JavaException ex)
                    {
                        DebugWriter.WriteDebugMessage($"Program ended with {ex.ClassFile.Name} ({ex.Message})");
                        DebugWriter.PrintStack(ex.Stack);
                    }
                    break;
                }
            }
            Console.ReadLine();
        }
    }
}
