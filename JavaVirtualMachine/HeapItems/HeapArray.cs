using System;
using System.Reflection;

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

        public short GetItemDataByte(int index)
        {
            if (ItemSize != 1) throw new InvalidOperationException();

            return Heap.GetShort(Address + Heap.ArrayBaseOffset + ItemSize * index);
        }

        public short GetItemDataShort(int index)
        {
            if (ItemSize != 2) throw new InvalidOperationException();

            return Heap.GetShort(Address + Heap.ArrayBaseOffset + ItemSize * index);
        }

        public int GetItemData(int index)
        {
            if (ItemSize != 4) throw new InvalidOperationException();

            return Heap.GetInt(Address + Heap.ArrayBaseOffset + ItemSize * index);
        }

        public long GetItemDataLong(int index)
        {
            if (ItemSize != 8) throw new InvalidOperationException();

            return Heap.GetLong(Address + Heap.ArrayBaseOffset + ItemSize * index);
        }

        public short GetItemDataByOffsetByte(int offset)
        {
            if (ItemSize != 1) throw new InvalidOperationException();

            return Heap.GetByte(Address + offset);
        }

        public short GetItemDataByOffsetShort(int offset)
        {
            if (ItemSize != 2) throw new InvalidOperationException();

            return Heap.GetShort(Address + offset);
        }

        public int GetItemDataByOffset(int offset)
        {
            if (ItemSize != 4) throw new InvalidOperationException();

            return Heap.GetInt(Address + offset);
        }

        public long GetItemDataByOffsetLong(int offset)
        {
            if (ItemSize != 8) throw new InvalidOperationException();

            return Heap.GetLong(Address + offset);
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

        public void SetItemByOffset(int offset, byte itemData)
        {
            if (ItemSize != 1) throw new InvalidOperationException();

            Heap.PutByte(Address + offset, itemData);
        }

        public void SetItemByOffset(int offset, short itemData)
        {
            if (ItemSize != 2) throw new InvalidOperationException();

            Heap.PutShort(Address + offset, itemData);
        }

        public void SetItemByOffset(int offset, int itemData)
        {
            if (ItemSize != 4) throw new InvalidOperationException();

            Heap.PutInt(Address + offset, itemData);
        }

        public void SetItemByOffset(int offset, long itemData)
        {
            if (ItemSize != 8) throw new InvalidOperationException();

            Heap.PutLong(Address + offset, itemData);
        }

        public Span<byte> GetDataSpan()
        {
            return Heap.GetSpan(Address + Heap.ArrayBaseOffset, ItemSize * Length);
        }
    }
}
