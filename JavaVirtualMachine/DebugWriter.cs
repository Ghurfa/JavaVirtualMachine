using JavaVirtualMachine.ConstantPoolInfo;
using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    public static class DebugWriter
    {
        public static int Depth = 0;
        public static bool WriteDebugMessages = false;
        const int Spacing = 2;
        const ConsoleColor DebugDefaultColor = ConsoleColor.DarkGray;
        const ConsoleColor NativeMethodColor = ConsoleColor.Green;
        const ConsoleColor ExceptionThrownColor = ConsoleColor.Yellow;

        //Specially printed
        const ConsoleColor arrayBracketColor = ConsoleColor.Red;
        const ConsoleColor nullColor = ConsoleColor.DarkGreen;
        const ConsoleColor stringColor = ConsoleColor.DarkGreen;
        const ConsoleColor classObjColor = ConsoleColor.DarkBlue;
        const ConsoleColor booleanColor = ConsoleColor.DarkRed;
        const ConsoleColor charColor = ConsoleColor.DarkGreen;

        //Objects
        const ConsoleColor classNameColor = ConsoleColor.Blue;
        const ConsoleColor separatorColor = ConsoleColor.White;
        const ConsoleColor objAddrColor = ConsoleColor.Cyan;
        
        //Numbers - could be confused
        const ConsoleColor byteColor = ConsoleColor.DarkGreen;
        const ConsoleColor floatColor = ConsoleColor.DarkBlue;
        const ConsoleColor integerColor = ConsoleColor.White;
        const ConsoleColor shortColor = ConsoleColor.DarkYellow;
        const ConsoleColor longColor = ConsoleColor.DarkMagenta;
        const ConsoleColor doubleColor = ConsoleColor.DarkRed;
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
                Console.CursorLeft = Depth * Spacing;
                Console.Write("Returned ");

                MethodInfo methodInfo = Program.MethodFrameStack.Peek().MethodInfo;
                string returnType = methodInfo.Descriptor.Split(')')[1];
                WriteValue(returnType, 0, returnValue);
                Console.WriteLine();
            }
        }
        public static void ReturnedValueDebugWrite(long returnValue)
        {
            Depth--;
            if (WriteDebugMessages)
            {
                Console.ForegroundColor = DebugDefaultColor;
                Console.CursorLeft = Depth * Spacing;
                Console.Write("Returned ");

                MethodInfo methodInfo = Program.MethodFrameStack.Peek().MethodInfo;
                string returnType = methodInfo.Descriptor.Split(')')[1];
                WriteWideValue(returnType, 0, returnValue);
                Console.WriteLine();
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
                ConsoleColor originalColor = Console.ForegroundColor;
                int argIndex = 0;
                int i;
                Console.Write("(");
                if (!isStatic)
                {
                    int callerAddr = args[0];
                    ClassFile argCFile = Heap.GetObject(callerAddr).ClassFile;
                    if (argCFile.Name == "java/lang/Class")
                    {
                        Console.ForegroundColor = classObjColor;
                        Console.Write(JavaHelper.ClassObjectName(callerAddr));
                    }
                    else
                    {
                        Console.ForegroundColor = classNameColor;
                        Console.Write(Heap.GetObject(callerAddr).ClassFile.Name);
                    }
                    Console.ForegroundColor = separatorColor;
                    Console.Write('/');

                    Console.ForegroundColor = objAddrColor;
                    Console.Write(callerAddr);
                    argIndex++;
                }
                for (i = 1; descriptor[i] != ')';)
                {
                    if (!isStatic || i != 1)
                    {
                        Console.Write(", ");
                    }
                    if (descriptor[i] == 'J' || descriptor[i] == 'D')
                    {
                        long argument = (args[argIndex], args[argIndex + 1]).ToLong();
                        i = WriteWideValue(descriptor, i, argument);
                        argIndex += 2;
                    }
                    else
                    {
                        int argument = args[argIndex];
                        i = WriteValue(descriptor, i, argument);
                        argIndex++;
                    }
                }
                Console.ForegroundColor = originalColor;
                Console.Write(')');
            }
        }
        private static int WriteWideValue(string descriptor, int i, long argument)
        {
            if(descriptor[i] == 'J')
            {
                Console.ForegroundColor = longColor;
                Console.Write(argument.ToString());
            }
            else
            {
                Console.ForegroundColor = doubleColor;
                Console.Write(JavaHelper.StoredDoubleToDouble(argument).ToString());
            }
            return i + 1;
        }
        private static int WriteValue(string descriptor, int i, int argument)
        {
            if (descriptor[i] == '[')
            {
                Console.ForegroundColor = arrayBracketColor;
                Console.Write('[');
                i++;
            }
            if (descriptor[i] == 'L')
            {
                for (i++; descriptor[i] != ';'; i++) ;
                if (argument == 0)
                {
                    Console.ForegroundColor = nullColor;
                    Console.Write("Null");
                }
                else
                {
                    ClassFile argCFile = Heap.GetObject(argument).ClassFile;
                    if (argCFile.Name == "java/lang/String")
                    {
                        Console.ForegroundColor = stringColor;
                        Console.Write('"' + JavaHelper.ReadJavaString(argument) + '"');
                    }
                    else if (argCFile.Name == "java/lang/Class")
                    {
                        Console.ForegroundColor = classObjColor;
                        Console.Write(JavaHelper.ClassObjectName(argument));
                    }
                    else
                    {
                        Console.ForegroundColor = classNameColor;
                        Console.Write(Heap.GetObject(argument).ClassFile.Name);
                    }
                    Console.ForegroundColor = separatorColor;
                    Console.Write('/');

                    Console.ForegroundColor = objAddrColor;
                    Console.Write(argument);
                }
            }
            else if(descriptor[i] == 'Z')
            {
                Console.ForegroundColor = booleanColor;
                Console.Write(argument != 0 ? "True" : "False");
            }
            else if(descriptor[i] == 'C')
            {
                Console.ForegroundColor = charColor;
                Console.Write("'" + (char)argument + "'");
            }
            else if(descriptor[i] == 'F')
            {
                Console.ForegroundColor = floatColor;
                Console.Write(JavaHelper.StoredFloatToFloat(argument));
            }
            else 
            {
                switch (descriptor[i])
                {
                    case 'B':
                        Console.ForegroundColor = byteColor;
                        break;
                    case 'I':
                        Console.ForegroundColor = integerColor;
                        break;
                    case 'S':
                        Console.ForegroundColor = shortColor;
                        break;
                }
                Console.Write(argument);
            }
            return i + 1;
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
