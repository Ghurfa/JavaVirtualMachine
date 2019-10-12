using System;
using System.Collections.Generic;
using System.Text;

namespace JavaVirtualMachine
{
    class HeapArray : HeapObject
    {
        public Array Array;
        public int ItemTypeClassObjAddr;
        public static int ArrayBaseOffset = 8;
        private int itemSize;
        public int ItemSize => itemSize;
        public HeapArray(Array array, int itemTypeClassObjAddr) : base(ClassFileManager.GetClassFile("java/lang/Object"))
        {
            Array = array;
            ItemTypeClassObjAddr = itemTypeClassObjAddr;

            int doubleClassObject = ClassObjectManager.GetClassObjectAddr("double");
            int longClassObject = ClassObjectManager.GetClassObjectAddr("long");
            itemSize = itemTypeClassObjAddr == doubleClassObject || itemTypeClassObjAddr == longClassObject ? 8 : 4;
            //data = new Memory<byte>(new byte[]);
            NumOfBytes = 8 + 8 + itemSize * array.GetLength(0);
            Address = Heap.AllocateMemory(NumOfBytes);
            //ydata = Heap.GetMemorySlice(Address, size);
            ((long)Array.GetLength(0)).AsByteArray().CopyTo(Heap.GetMemorySlice(Address + 8, 8));
            //SetField("length", "I", );
        }
        public int GetLengthData()
        {
            int length = (int)Heap.GetMemorySlice(Address + 8, 8).ToArray().ToLong();
            if (length != Array.GetLength(0))
            {
                throw new InvalidOperationException();
            }
            return length;
        }
        public int GetItemData(int index)
        {
            if (itemSize == 8)
            {
                long itemData = Heap.GetMemorySlice(Address + 8 + 8 + itemSize * index, itemSize).ToArray().ToLong();
                return (int)itemData;
            }
            else if (itemSize == 4)
            {
                int itemData = Heap.GetMemorySlice(Address + 8 + 8 + itemSize * index, itemSize).ToArray().ToInt();
                return itemData;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        public int GetItemDataByOffset(long offset)
        {
            return GetItemData((int)((offset - ArrayBaseOffset) / itemSize));
        }
        public void SetItem(int index, long itemData)
        {
            Memory<byte> dataSlice = Heap.GetMemorySlice(Address + 8 + 8 + itemSize * index, itemSize);
            byte[] itemDataAsArray = itemSize == 8 ? itemData.AsByteArray() : ((int)itemData).AsByteArray();
            itemDataAsArray.CopyTo(dataSlice);
            if (Array is int[] intArr)
            {
                intArr[index] = (int)itemData;
            }
            else if (Array is byte[] byteArr)
            {
                byteArr[index] = (byte)itemData;
            }
            else if (Array is char[] charArr)
            {
                charArr[index] = (char)itemData;
            }
            else if (Array is short[] shortArr)
            {
                shortArr[index] = (short)itemData;
            }
            else if (Array is long[] longArr)
            {
                longArr[index] = itemData;
            }
            else if (Array is bool[] boolArr)
            {
                boolArr[index] = itemData != 0;
            }
            else if (Array is double[] doubleArr)
            {
                doubleArr[index] = Utility.StoredDoubleToDouble((ulong)itemData);
            }
            else if (Array is float[] floatArr)
            {
                floatArr[index] = (float)Utility.StoredDoubleToDouble((ulong)itemData);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        public void SetItemByOffset(long offset, long itemData) => SetItem((int)((offset - ArrayBaseOffset) / itemSize), itemData);
        public void SetDataByOffset(byte[] data, int offset, int numOfBytes)
        {
            for (int i = 0; i < numOfBytes; i++)
            {
                SetItemByOffset(offset + ItemSize * i, data[i]);
            }
        }
        public override HeapObject Clone()
        {
            HeapArray clone = new HeapArray(Array, ItemTypeClassObjAddr);
            return clone;
        }
    }
}
