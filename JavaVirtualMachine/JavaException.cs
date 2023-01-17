namespace JavaVirtualMachine
{
    public class JavaException : InvalidOperationException
    {
        public ClassFile ClassFile;
        public MethodFrame[] Stack;
        public JavaException(ClassFile type, string message)
            : base(message)
        {
            ClassFile = type;
            Stack = new MethodFrame[Program.MethodFrameStack.Count];
            Program.MethodFrameStack.CopyTo(Stack, 0);
        }
        public JavaException(ClassFile type)
            : base()
        {
            ClassFile = type;
            Stack = new MethodFrame[Program.MethodFrameStack.Count];
            Program.MethodFrameStack.CopyTo(Stack, 0);
        }
    }
}
