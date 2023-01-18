using JavaVirtualMachine.Heaps;

namespace JavaVirtualMachine
{
    public class HeapArray : HeapObject
    {
        public int Length { get; private set; }
        public int ItemTypeClassObjAddr { get; private set; }
        public int ItemSize { get; private set; }

        public HeapArray(int address, IHeap heap)
            : base (address, heap)
        {
            ItemTypeClassObjAddr = Heap.GetInt(Address + Heap.ArrayItemTypeOffset);
            Length = Heap.GetInt(Address + Heap.ArrayLengthOffset);
            ItemSize = GetItemSize(ItemTypeClassObjAddr);
        }

        public byte GetItemByte(int index)
        {
            if (ItemSize != 1) throw new InvalidOperationException();

            return OwnerHeap.GetByte(Address + OwnerHeap.ArrayBaseOffset + ItemSize * index);
        }

        public short GetItemShort(int index)
        {
            if (ItemSize != 2) throw new InvalidOperationException();

            return OwnerHeap.GetShort(Address + OwnerHeap.ArrayBaseOffset + ItemSize * index);
        }

        public int GetItem(int index)
        {
            if (ItemSize != 4) throw new InvalidOperationException();

            return OwnerHeap.GetInt(Address + OwnerHeap.ArrayBaseOffset + ItemSize * index);
        }

        public long GetItemLong(int index)
        {
            if (ItemSize != 8) throw new InvalidOperationException();

            return OwnerHeap.GetLong(Address + OwnerHeap.ArrayBaseOffset + ItemSize * index);
        }

        public void SetItem(int index, byte itemData)
        {
            if (ItemSize != 1) throw new InvalidOperationException();

            OwnerHeap.PutByte(Address + OwnerHeap.ArrayBaseOffset + ItemSize * index, itemData);
        }

        public void SetItem(int index, short itemData)
        {
            if (ItemSize != 2) throw new InvalidOperationException();

            OwnerHeap.PutShort(Address + OwnerHeap.ArrayBaseOffset + ItemSize * index, itemData);
        }

        public void SetItem(int index, int itemData)
        {
            if (ItemSize != 4) throw new InvalidOperationException();

            OwnerHeap.PutInt(Address + OwnerHeap.ArrayBaseOffset + ItemSize * index, itemData);
        }

        public void SetItem(int index, long itemData)
        {
            if (ItemSize != 8) throw new InvalidOperationException();

            OwnerHeap.PutLong(Address + OwnerHeap.ArrayBaseOffset + ItemSize * index, itemData);
        }

        public Span<byte> GetDataSpan()
        {
            return OwnerHeap.GetSpan(Address + OwnerHeap.ArrayBaseOffset, ItemSize * Length);
        }

        private static int GetItemSize(int itemTypeClassObjAddr)
        {
            int doubleClassObject = ClassObjectManager.GetClassObjectAddr("double");
            int longClassObject = ClassObjectManager.GetClassObjectAddr("long");
            int shortClassObject = ClassObjectManager.GetClassObjectAddr("short");
            int charClassObject = ClassObjectManager.GetClassObjectAddr("char");
            int byteClassObject = ClassObjectManager.GetClassObjectAddr("byte");
            int booleanClassObject = ClassObjectManager.GetClassObjectAddr("boolean");

            if (itemTypeClassObjAddr == doubleClassObject || itemTypeClassObjAddr == longClassObject)
            {
                return 8;
            }
            else if (itemTypeClassObjAddr == shortClassObject || itemTypeClassObjAddr == charClassObject)
            {
                return 2;
            }
            else if (itemTypeClassObjAddr == byteClassObject || itemTypeClassObjAddr == booleanClassObject)
            {
                return 1;
            }
            else return 4;
        }
    }
}
