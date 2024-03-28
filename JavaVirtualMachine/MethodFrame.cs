using JavaVirtualMachine.Attributes;
using JavaVirtualMachine.ConstantPoolItems;

namespace JavaVirtualMachine
{
    public struct MethodFrame
    {
        public readonly MethodInfo Method;
        public readonly int BaseOffset;
        public readonly IEnumerator<MethodInfo>? NativeState;

        public int SP
        {
            get => Executor.GlobalStack.Span[BaseOffset + Method.MaxLocals];
            set => Executor.GlobalStack.Span[BaseOffset + Method.MaxLocals] = value;
        }

        public int IP
        {
            get => Executor.GlobalStack.Span[BaseOffset + Method.MaxLocals + 1];
            set => Executor.GlobalStack.Span[BaseOffset + Method.MaxLocals + 1] = value;
        }

        public Span<int> Stack => Executor.GlobalStack.Span.Slice(BaseOffset + Method.MaxLocals + 2, Method.MaxStack);

        public MethodFrame(MethodInfo methodInfo, int baseOffset, IEnumerator<MethodInfo>? nativeState)
        {
            Method = methodInfo;
            BaseOffset = baseOffset;
            NativeState = nativeState;
            SP = 0;
            IP = 0;
        }
    }
}
