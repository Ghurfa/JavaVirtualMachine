using JavaVirtualMachine;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JVMUnitTests
{
    public class OpCodesUnitTests
    {
        [Theory]
        [InlineData(5, 0)]
        [InlineData(8, -3)]
        [InlineData(-10, 3)]
        public void iaddTest(int val1, int val2)
        {
            ClassFileManager.InitDictionary(@"..\..\..\..\rt\",
                                            Program.BaseDirectory+ @"build\classes\java\main\");
            ClassFile testClassFile = new ClassFile(@"..\..\..\..\GradleProject\build\classes\java\main\TestClassFile.class");
            DebugWriter.WriteDebugMessages = false;
            MethodInfo mainMethodInfo = testClassFile.MethodDictionary[("main", "()V")];
            MethodFrame mainMethodFrame = new MethodFrame(mainMethodInfo);
            Program.MethodFrameStack.Clear();
            Program.MethodFrameStack.Push(mainMethodFrame);

            MethodInfo iaddMethod = testClassFile.MethodDictionary[("iadd", "(II)I")];
            MethodFrame frame = new MethodFrame(iaddMethod);
            frame.Locals[0] = val1;
            frame.Locals[1] = val2;
            frame.Execute();

            Assert.Equal(val1 + val2, mainMethodFrame.Stack[0]);

            Program.MethodFrameStack.Clear();
        }

        [Theory]
        [InlineData(5, 0)]
        [InlineData(8, -3)]
        [InlineData(1099512347776, 1239233)]
        public void laddTest(long val1, long val2)
        {
            ClassFileManager.InitDictionary(@"..\..\..\..\rt\",
                                            Program.BaseDirectory + @"build\classes\java\main\");
            ClassFile testClassFile = new ClassFile(@"..\..\..\..\GradleProject\build\classes\java\main\TestClassFile.class"); 
            DebugWriter.WriteDebugMessages = false;
            MethodInfo mainMethodInfo = testClassFile.MethodDictionary[("main", "()V")];
            MethodFrame mainMethodFrame = new MethodFrame(mainMethodInfo);
            Program.MethodFrameStack.Clear();
            Program.MethodFrameStack.Push(mainMethodFrame);

            MethodInfo laddMethod = testClassFile.MethodDictionary[("ladd", "(JJ)J")];
            MethodFrame frame = new MethodFrame(laddMethod);
            (int, int) val1Split = Utility.Split(val1);
            (int, int) val2Split = Utility.Split(val2);
            frame.Locals[0] = val1Split.Item1;
            frame.Locals[1] = val1Split.Item2;
            frame.Locals[2] = val2Split.Item1;
            frame.Locals[3] = val2Split.Item2;
            frame.Execute();

            Assert.Equal(val1 + val2, Utility.PeekLong(mainMethodFrame.Stack, 2));

            Program.MethodFrameStack.Clear();
        }

        [Theory]
        [InlineData(3f, 0)]
        [InlineData(8.3f, -3.2f)]
        [InlineData(-10.1f, 3.93f)]
        public void faddTest(float val1, float val2)
        {
            ClassFileManager.InitDictionary(@"..\..\..\..\rt\",
                                            Program.BaseDirectory + @"build\classes\java\main\");
            ClassFile testClassFile = new ClassFile(@"..\..\..\..\GradleProject\build\classes\java\main\TestClassFile.class");
            DebugWriter.WriteDebugMessages = false;
            MethodInfo mainMethodInfo = testClassFile.MethodDictionary[("main", "()V")];
            MethodFrame mainMethodFrame = new MethodFrame(mainMethodInfo);
            Program.MethodFrameStack.Clear();
            Program.MethodFrameStack.Push(mainMethodFrame);

            MethodInfo faddMethod = testClassFile.MethodDictionary[("fadd", "(FF)F")];
            MethodFrame frame = new MethodFrame(faddMethod);
            frame.Locals[0] = JavaHelper.FloatToStoredFloat(val1);
            frame.Locals[1] = JavaHelper.FloatToStoredFloat(val2);
            frame.Execute();

            Assert.Equal(val1 + val2, JavaHelper.StoredFloatToFloat(mainMethodFrame.Stack[0]));

            Program.MethodFrameStack.Clear();
        }

        [Theory]
        [InlineData(3f, 0)]
        [InlineData(8.3f, -3.2f)]
        [InlineData(-10.1f, 3.93f)]
        public void daddTest(double val1, double val2)
        {
            ClassFileManager.InitDictionary(@"..\..\..\..\rt\",
                                            Program.BaseDirectory + @"build\classes\java\main\");
            ClassFile testClassFile = new ClassFile(@"..\..\..\..\GradleProject\build\classes\java\main\TestClassFile.class");
            DebugWriter.WriteDebugMessages = false;
            MethodInfo mainMethodInfo = testClassFile.MethodDictionary[("main", "()V")];
            MethodFrame mainMethodFrame = new MethodFrame(mainMethodInfo);
            Program.MethodFrameStack.Clear();
            Program.MethodFrameStack.Push(mainMethodFrame);

            MethodInfo daddMethod = testClassFile.MethodDictionary[("dadd", "(DD)D")];
            MethodFrame frame = new MethodFrame(daddMethod);
            (int, int) val1Split = Utility.Split(JavaHelper.DoubleToStoredDouble(val1));
            (int, int) val2Split = Utility.Split(JavaHelper.DoubleToStoredDouble(val2));
            frame.Locals[0] = val1Split.Item1;
            frame.Locals[1] = val1Split.Item2;
            frame.Locals[2] = val2Split.Item1;
            frame.Locals[3] = val2Split.Item2;
            frame.Execute();

            Assert.Equal(val1 + val2, JavaHelper.StoredDoubleToDouble(Utility.PeekLong(mainMethodFrame.Stack, 2)));

            Program.MethodFrameStack.Clear();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(-5)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void inegTest(int value)
        {
            ClassFileManager.InitDictionary(@"..\..\..\..\rt\",
                                            Program.BaseDirectory + @"build\classes\java\main\");
            ClassFile testClassFile = new ClassFile(@"..\..\..\..\GradleProject\build\classes\java\main\TestClassFile.class");
            DebugWriter.WriteDebugMessages = false;
            MethodInfo mainMethodInfo = testClassFile.MethodDictionary[("main", "()V")];
            MethodFrame mainMethodFrame = new MethodFrame(mainMethodInfo);
            Program.MethodFrameStack.Clear();
            Program.MethodFrameStack.Push(mainMethodFrame);

            MethodInfo inegMethod = testClassFile.MethodDictionary[("ineg", "(I)I")];
            MethodFrame frame = new MethodFrame(inegMethod);
            frame.Locals[0] = value;
            frame.Execute();

            Assert.Equal(-value, mainMethodFrame.Stack[0]);

            Program.MethodFrameStack.Clear();
        }
    }
}
