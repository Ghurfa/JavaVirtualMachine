using JavaVirtualMachine;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JVMUnitTests
{
    public class UtilityUnitTests
    {
        [Theory]
        [InlineData(123)]
        [InlineData(-77123)]
        [InlineData(0)]
        public void IntStackTest(int num)
        {
            int[] stack = new int[2];
            int sp = 1;
            Utility.Push(ref stack, ref sp, num);
            Assert.Equal(num, stack[1]);
            int popped = Utility.PopInt(stack, ref sp);
            Assert.Equal(num, popped);
        }

        [Theory]
        [InlineData(123098134923842)]
        [InlineData(-123098134923842)]
        [InlineData(0)]
        public void LongStackTest(long num)
        {
            int[] stack = new int[3];
            int sp = 1;
            Utility.Push(ref stack, ref sp, num);
            Assert.Equal(num, (stack[1], stack[2]).ToLong());
            long popped = Utility.PopLong(stack, ref sp);
            Assert.Equal(num, popped);
        }

        [Theory]
        [InlineData(2134.123f)]
        [InlineData(-129909.2f)]
        [InlineData(0)]
        public void FloatStackTest(float num)
        {
            int[] stack = new int[2];
            int sp = 1;
            Utility.Push(ref stack, ref sp, num);
            Assert.Equal(num, JavaHelper.StoredFloatToFloat(stack[1]));
            float popped = Utility.PopFloat(stack, ref sp);
            Assert.Equal(num, popped);
        }

        [Theory]
        [InlineData(2134.123f)]
        [InlineData(-129909.2f)]
        [InlineData(0)]
        public void DoubleStackTest(double num)
        {
            int[] stack = new int[3];
            int sp = 1;
            Utility.Push(ref stack, ref sp, num);
            Assert.Equal(num, JavaHelper.StoredDoubleToDouble((stack[1], stack[2]).ToLong()));
            double popped = Utility.PopDouble(stack, ref sp);
            Assert.Equal(num, popped);
        }
    }
}
