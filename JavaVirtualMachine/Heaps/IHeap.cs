namespace JavaVirtualMachine.Heaps
{
    public interface IHeap
    {
        public int ObjectFieldOffset { get; }
        public int ObjectFieldSize { get; }
        public int ArrayMarkOffset { get; }
        public int ArrayItemTypeOffset { get; }
        public int ArrayLengthOffset { get; }
        public int ArrayBaseOffset { get; }

        public int CreateObject(int classFileIdx);
        public int CreateArray(int[] arr, int itemTypeClassObjAddr);
        public int CreateArray(int itemSize, int itemCount, int itemTypeClassObjAddr);
        public int CloneObject(int address);

        public HeapObject GetObject(int address);
        public HeapArray GetArray(int address);

        public byte GetByte(int address);
        public short GetShort(int address);
        public int GetInt(int address);
        public long GetLong(int address);
        public Span<byte> GetSpan(int address, int length);

        public void PutByte(int address, byte value);
        public void PutShort(int address, short value);
        public void PutInt(int address, int value);
        public void PutLong(int address, long value);

        public int AllocateMemory(long size);
    }
}
