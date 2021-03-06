﻿using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace JavaVirtualMachine
{
    public class Program
    {
        public static Stack<MethodFrame> MethodFrameStack = new Stack<MethodFrame>();
        public static string BaseDirectory = @"..\..\..\..\GradleProject\";
        public static string JavaHome = @"C:\Program Files\Java\jdk1.8.0_221\jre";
        public static Stopwatch Stopwatch = new Stopwatch();
        static void Main(string[] args)
        {
            Stopwatch.Start();
            Console.WindowWidth = 180;

            //%JAVA_HOME%\bin\javap" - s -p -c -verbose Scanner.class > ..\..\..\Scanner.javap
            ClassFileManager.InitDictionary(@"..\..\..\..\rt\",
                                            BaseDirectory + @"build\classes\java\main\");

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
