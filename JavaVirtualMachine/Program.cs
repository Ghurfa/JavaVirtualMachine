using JavaVirtualMachine.StackTracePrinters;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Pipes;

namespace JavaVirtualMachine
{
    internal struct Config
    {
        public string rtPath { get; set; }
        public string javaHome { get; set; }
        public string[] srcPaths { get; set; }
        public PrintStackTraceOptions printStackTrace { get; set; }
    }

    internal class Program
    {
        public static Stopwatch Stopwatch = new Stopwatch();
        public static Config Configuration { get; private set; }
        public static IStackTracePrinter StackTracePrinter = EmptyStackTracePrinter.Instance;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                //Get configuration
                string configPath = @"..\..\..\config.json";
                Configuration = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));

                switch(Configuration.printStackTrace)
                {
                    case PrintStackTraceOptions.Window:
                        //Start stack trace console by running current process with different args
                        string currProcessName = Environment.CommandLine;
                        currProcessName = currProcessName.Trim('"', ' ');
                        currProcessName = Path.ChangeExtension(currProcessName, ".exe");
                        if (currProcessName.Contains(Environment.CurrentDirectory))
                        {
                            currProcessName = currProcessName.Replace(Environment.CurrentDirectory, string.Empty);
                        }
                        currProcessName = currProcessName.Replace("\\", "");
                        currProcessName = currProcessName.Replace("\"", "");
                        ProcessStartInfo stcStartInfo = new ProcessStartInfo(currProcessName, "stacktrace " + Process.GetCurrentProcess().Id);
                        stcStartInfo.UseShellExecute = true;
                        Process.Start(stcStartInfo);

                        //Open pipe to communicate with printer process
                        NamedPipeServerStream serverStream = new("JVMStackTrace", PipeDirection.Out, 1);
                        ColoredStream coloredStream = new(serverStream);
                        StackTracePrinter = new ColorfulStackTracePrinter((color, str) => coloredStream.WriteString(color, str));
                        serverStream.WaitForConnection();
                        break;
                    case PrintStackTraceOptions.File:
                        string stOutputFile = @"..\..\..\stackTrace.txt";
                        if(File.Exists(stOutputFile))
                        {
                            File.Delete(stOutputFile);
                        }
                        FileStream fileStream = File.OpenWrite(stOutputFile);
                        StreamWriter fileWriter = new (fileStream);
                        StackTracePrinter = new ColorfulStackTracePrinter((color, str) => 
                        {
                            fileWriter.Write(str); fileWriter.Flush(); 
                        });
                        break;
                }

                JVM();
            }
            else if (args[0] == "stacktrace")
            {
                StackTraceConsole(parentID: int.Parse(args[1]));
            }
        }

        static void StackTraceConsole(int parentID)
        {
            Process parentProcess = Process.GetProcessById(parentID);
            using NamedPipeClientStream clientStream = new(".", "JVMStackTrace", PipeDirection.In, PipeOptions.None);
            clientStream.Connect();

            ColoredStream colorStream = new(clientStream);
            while (!parentProcess.HasExited)
            {
                (byte color, string str) = colorStream.ReadString();
                Console.ForegroundColor = (ConsoleColor)color;
                Console.Write(str);
            }
        }

        static void JVM()
        {
            Stopwatch.Start();

            //%JAVA_HOME%\bin\javap" - s -p -c -verbose Scanner.class > ..\..\..\Scanner.javap
            ClassFileManager.InitDictionary(runtimePath: Configuration.rtPath, otherPaths: Configuration.srcPaths);

            //Create main thread object
            int threadGroupAddr = Heap.CreateObject(ClassFileManager.GetClassFileIndex("java/lang/ThreadGroup"));
            
            ThreadManager.ThreadAddr = Heap.CreateObject(ClassFileManager.GetClassFileIndex("java/lang/Thread"));
            HeapObject threadObj = Heap.GetObject(ThreadManager.ThreadAddr);

            threadObj.SetField("group", "Ljava/lang/ThreadGroup;", threadGroupAddr);
            threadObj.SetField("priority", "I", 5);

            ClassFile systemCFile = ClassFileManager.GetClassFile("java/lang/System");
            MethodInfo initSystemClassMethod = systemCFile.MethodDictionary[("initializeSystemClass", "()V")];

            Executor.BeginExecution(initSystemClassMethod);
            if (Executor.ActiveException != 0)
            {
                throw new NotImplementedException();
            }

            int mainProgCFileIdx = ClassFileManager.GetClassFileIndex("Program");
            ClassFile mainProg = ClassFileManager.ClassFiles[mainProgCFileIdx];
            int mainProgObjAddr = Heap.CreateObject(mainProgCFileIdx);
            MethodInfo mainProgInit = mainProg.MethodDictionary[("<init>", "()V")];
            
            Executor.BeginExecution(mainProgInit, mainProgObjAddr);
            if (Executor.ActiveException != 0)
            {
                throw new NotImplementedException();
            }

            if (mainProg.MethodDictionary.TryGetValue(("<clinit>", "()V"), out MethodInfo clinitMethod))
            {
                Executor.BeginExecution(clinitMethod, mainProgObjAddr);
                if (Executor.ActiveException != 0)
                {
                    throw new NotImplementedException();
                }
            }

            foreach (var method in mainProg.MethodDictionary)
            {
                if (method.Key.name == "main")
                {
                    Executor.BeginExecution(method.Value, mainProgObjAddr, 0);
                    if (Executor.ActiveException != 0)
                    {
                        throw new NotImplementedException();
                        //DebugWriter.WriteDebugMessage($"Program ended with {ex.ClassFile.Name} ({ex.Message})");
                        //DebugWriter.PrintStack(ex.Stack);
                    }
                }
            }
        }
    }
}
