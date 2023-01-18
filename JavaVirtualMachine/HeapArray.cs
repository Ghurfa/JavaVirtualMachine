namespace JavaVirtualMachine
{
    public class HeapArray : HeapObject
    {
        public int ItemSize { get; private set; }
        public int Length => Heap.GetInt(Address + Heap.ArrayLengthOffset);
        public int ItemTypeClassObjAddr => Heap.GetInt(Address + Heap.ArrayItemTypeOffset);

        public HeapArray(int address, int itemSize)
            : base (address)
        {
            ItemSize = itemSize;
        }

        public byte GetItemByte(int index)
        {
            if (ItemSize != 1) throw new InvalidOperationException();

            return Heap.GetByte(Address + Heap.ArrayBaseOffset + ItemSize * index);
        }

        public short GetItemShort(int index)
        {
            if (ItemSize != 2) throw new InvalidOperationException();

            return Heap.GetShort(Address + Heap.ArrayBaseOffset + ItemSize * index);
        }

        public int GetItem(int index)
        {
            if (ItemSize != 4) throw new InvalidOperationException();

            return Heap.GetInt(Address + Heap.ArrayBaseOffset + ItemSize * index);
        }

        public long GetItemLong(int index)
        {
            if (ItemSize != 8) throw new InvalidOperationException();

            return Heap.GetLong(Address + Heap.ArrayBaseOffset + ItemSize * index);
        }

        public void SetItem(int index, byte itemData)
        {
            if (ItemSize != 1) throw new InvalidOperationException();

            Heap.PutByte(Address + Heap.ArrayBaseOffset + ItemSize * index, itemData);
        }

        public void SetItem(int index, short itemData)
        {
            if (ItemSize != 2) throw new InvalidOperationException();

            Heap.PutShort(Address + Heap.ArrayBaseOffset + ItemSize * index, itemData);
        }

        public void SetItem(int index, int itemData)
        {
            if (ItemSize != 4) throw new InvalidOperationException();

            Heap.PutInt(Address + Heap.ArrayBaseOffset + ItemSize * index, itemData);
        }

        public void SetItem(int index, long itemData)
        {
            if (ItemSize != 8) throw new InvalidOperationException();

            Heap.PutLong(Address + Heap.ArrayBaseOffset + ItemSize * index, itemData);
        }

        public Span<byte> GetDataSpan()
        {
            return Heap.GetSpan(Address + Heap.ArrayBaseOffset, ItemSize * Length);
        }
    }
}
