using JavaVirtualMachine.ConstantPoolInfo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace JavaVirtualMachine
{
    internal struct Config
    {
        public string rtPath { get; set; }
        public string javaHome { get; set; }
        public string[] srcPaths { get; set; }
        public bool printStackTrace { get; set; }
    }

    public class Program
    {
        public static Stack<MethodFrame> MethodFrameStack = new Stack<MethodFrame>();
        public static Stopwatch Stopwatch = new Stopwatch();

        internal static Config Configuration { get; private set; }

        static void Main(string[] args)
        {
            string configPath = @"..\..\..\config.json";
            Configuration = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
            DebugWriter.WriteDebugMessages = Configuration.printStackTrace;

            Console.WindowWidth = 180;
            Stopwatch.Start();

            //%JAVA_HOME%\bin\javap" - s -p -c -verbose Scanner.class > ..\..\..\Scanner.javap
            ClassFileManager.InitDictionary(runtimePath: Configuration.rtPath, otherPaths: Configuration.srcPaths);

            //Create main thread object
            ClassFile threadGroupCFile = ClassFileManager.GetClassFile("java/lang/ThreadGroup");
            HeapObject threadGroupObj = new HeapObject(threadGroupCFile);
            int threadGroupAddr = Heap.AddItem(threadGroupObj);

            ClassFile threadCFile = ClassFileManager.GetClassFile("java/lang/Thread");
            HeapObject threadObj = new HeapObject(threadCFile);
            ThreadManager.ThreadAddr = Heap.AddItem(threadObj);

            threadObj.SetField("group", "Ljava/lang/ThreadGroup;", threadGroupAddr);
            threadObj.SetField("priority", "I", 5);

            ClassFile systemCFile = ClassFileManager.GetClassFile("java/lang/System");
            MethodInfo initSystemClassMethod = systemCFile.MethodDictionary[("initializeSystemClass", "()V")];
            try
            {
                JavaHelper.RunJavaFunction(initSystemClassMethod);
            }
            catch(JavaException ex)
            {
                if (ex.ClassFile.Name != "java/lang/IllegalStateException") throw;
            }

            ClassFile mainProg = ClassFileManager.GetClassFile("Program");
            int mainProgObjAddr = Heap.AddItem(new HeapObject(mainProg));
            MethodInfo mainProgInit = mainProg.MethodDictionary[("<init>", "()V")];
            try
            {
                JavaHelper.RunJavaFunction(mainProgInit, mainProgObjAddr);
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
                        JavaHelper.RunJavaFunction(method.Value, mainProgObjAddr);
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
                        JavaHelper.RunJavaFunction(method.Value, mainProgObjAddr, 0);
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
            Console.ReadKey();
        }
    }
}
