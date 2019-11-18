using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    public static class DebugWriter
    {
        public static int Depth = 0;
        public static bool WriteDebugMessages = true;
        const int Spacing = 2;
        const ConsoleColor DebugDefaultColor = ConsoleColor.DarkGray;
        const ConsoleColor NativeMethodColor = ConsoleColor.Green;
        const ConsoleColor ExceptionThrownColor = ConsoleColor.Yellow;
        public static void WriteDebugMessage(string message)
        {
            Console.ForegroundColor = DebugDefaultColor;
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
                    Console.ForegroundColor = DebugDefaultColor;
                }
                Console.Write($"{new string(' ', Depth * Spacing)}{methodInfo.ClassFile.Name}.{methodInfo.Name}");
                WriteArgs(methodInfo.Descriptor, methodInfo.HasFlag(MethodInfoFlag.Static), args);
                Console.WriteLine();
            }
            Depth++;
        }
        public static void CallFuncDebugWrite(CInterfaceMethodRefInfo interfaceMethodInfo, int[] args, string classOfFuncName)
        {
            if (WriteDebugMessages)
            {
                Console.ForegroundColor = DebugDefaultColor;
                Console.Write($"{new string(' ', Depth * Spacing)}{classOfFuncName}.{interfaceMethodInfo.Name}");
                throw new NotImplementedException();
                WriteArgs(interfaceMethodInfo.Descriptor, true, args);
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
                Console.ForegroundColor = DebugDefaultColor;
                Console.WriteLine($"{new string(' ', Depth * Spacing)}Returned {returnValue}");
            }
        }
        public static void ReturnedValueDebugWrite(long returnValue)
        {
            Depth--;
            if (WriteDebugMessages)
            {
                Console.ForegroundColor = DebugDefaultColor;
                Console.WriteLine($"{new string(' ', Depth * Spacing)}Returned {returnValue} (large num)");
            }
        }
        public static void ReturnedVoidDebugWrite()
        {
            Depth--;
            if (WriteDebugMessages)
            {
                Console.ForegroundColor = DebugDefaultColor;
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
        private static void WriteArgs(string descriptor, bool isStatic, int[] args)
        {
            if (WriteDebugMessages)
            {
                int argIndex = 0;
                int i;
                Console.Write("(");
                for (i = 1; descriptor[i] != ')';)
                {
                    if(i != 1)
                    {
                        Console.Write(", ");
                    }
                    if (descriptor[i] == 'J' || descriptor[i] == 'D')
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        long argument = (args[argIndex], args[argIndex + 1]).ToLong();
                        Console.Write(descriptor[i] == 'J' ? argument.ToString() : JavaHelper.StoredDoubleToDouble(argument).ToString());
                        argIndex += 2;
                        i++;
                    }
                    else
                    {
                        int argument = args[argIndex];
                        //Move to next arg
                        if (descriptor[i] == '[')
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write('[');
                            i++;
                        }
                        if (descriptor[i] == 'L')
                        {
                            for (i++; descriptor[i] != ';'; i++) ;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(Heap.GetObject(argument).ClassFile.Name);

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write('/');

                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write(argument);
                            argIndex++;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(argument);
                            argIndex++;
                        }
                        i++;
                    }
                }
                Console.Write(')');
                if(args.Length > 3)
                {

                }
            }
        }
        public static void PrintStack()
        {
            MethodFrame[] stack = new MethodFrame[Program.MethodFrameStack.Count];
            PrintStack(stack);
        }
        public static void PrintStack(MethodFrame[] stack)
        {
            Console.ForegroundColor = DebugDefaultColor;
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
