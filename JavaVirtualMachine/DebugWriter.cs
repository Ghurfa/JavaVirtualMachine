using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    static class DebugWriter
    {
        public static int Depth = 0;
        public static bool WriteDebugMessages = true;
        const int Spacing = 2;
        const ConsoleColor DebugColor = ConsoleColor.DarkGray;
        const ConsoleColor NativeMethodColor = ConsoleColor.Green;
        const ConsoleColor ExceptionThrownColor = ConsoleColor.Yellow;
        public static void WriteDebugMessage(string message)
        {
            Console.ForegroundColor = DebugColor;
            Console.WriteLine(message);
        }
        public static void CallFuncDebugWrite(MethodInfo methodInfo, int[] args)
        {
            if (WriteDebugMessages)
            {
                if (methodInfo.HasFlag(MethodInfoFlag.Native))
                {
                    Console.ForegroundColor = NativeMethodColor;
                }
                else
                {
                    Console.ForegroundColor = DebugColor;
                }
                Console.Write($"{new string(' ', Depth * Spacing)}{methodInfo.ClassFile.Name}.{methodInfo.Name}");
                WriteArgs(args);
                Console.WriteLine();
            }
            Depth++;
        }
        public static void CallFuncDebugWrite(CInterfaceMethodRefInfo interfaceMethodInfo, int[] args, string classOfFuncName)
        {
            if (WriteDebugMessages)
            {
                Console.ForegroundColor = DebugColor;
                Console.Write($"{new string(' ', Depth * Spacing)}{classOfFuncName}.{interfaceMethodInfo.Name}");
                WriteArgs(args);
                Console.Write($"   (interface {interfaceMethodInfo.ClassName})");
                Console.WriteLine();
            }
            Depth++;
        }
        public static void ReturnedValueDebugWrite(int returnValue)
        {
            Depth--;
            if (WriteDebugMessages)
            {
                Console.ForegroundColor = DebugColor;
                Console.WriteLine($"{new string(' ', Depth * Spacing)}Returned {returnValue}");
            }
        }
        public static void ReturnedValueDebugWrite(long returnValue)
        {
            Depth--;
            if (WriteDebugMessages)
            {
                Console.ForegroundColor = DebugColor;
                Console.WriteLine($"{new string(' ', Depth * Spacing)}Returned {returnValue} (large num)");
            }
        }
        public static void ReturnedVoidDebugWrite()
        {
            Depth--;
            if (WriteDebugMessages)
            {
                Console.ForegroundColor = DebugColor;
                Console.WriteLine($"{new string(' ', Depth * Spacing)}Returned void");
            }
        }
        public static void ExceptionThrownDebugWrite(JavaException exception)
        {
            Depth--;
            if (WriteDebugMessages)
            {
                Console.ForegroundColor = ExceptionThrownColor;
                Console.WriteLine($"{new string(' ', Depth * Spacing)}Threw {exception.ClassFile.Name} ({exception.Message})");
            }
        }
        private static void WriteArgs(int[] args)
        {
            if (WriteDebugMessages)
            {
                Console.Write("(");
                for (int i = 0; i < args.Length; i++)
                {
                    Console.Write(args[i]);
                    if (i < args.Length - 1)
                    {
                        Console.Write(", ");
                    }
                }
                Console.Write(')');
            }
        }
        public static void PrintStack()
        {
            MethodFrame[] stack = new MethodFrame[Program.MethodFrameStack.Count];
            PrintStack(stack);
        }
        public static void PrintStack(MethodFrame[] stack)
        {
            Console.ForegroundColor = DebugColor;
            Console.WriteLine("\nStack:");
            Program.MethodFrameStack.CopyTo(stack, 0);
            for (int i = stack.Length - 1; i >= 0; i--)
            {
                Console.CursorLeft = (stack.Length - 1 - i) * Spacing;
                MethodFrame frame = stack[i];
                Console.WriteLine($"{frame.ClassFile.Name}.{frame.MethodInfo.Name}{frame.MethodInfo.Descriptor}");
            }
        }
    }
}
