using JavaVirtualMachine;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JVMUnitTests
{
    public class JavaHelperUnitTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-10.234)]
        [InlineData(18923)]
        public void FloatStoreTest(float num)
        {
            int storedFloat = JavaHelper.FloatToStoredFloat(num);
            float decodedFloat = JavaHelper.StoredFloatToFloat(storedFloat);
            Assert.Equal(num, decodedFloat);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10.234)]
        [InlineData(3.39)]
        public void DoubleStoreTest(double num)
        {
            long storedDouble = JavaHelper.DoubleToStoredDouble(num);
            double decodedDouble = JavaHelper.StoredDoubleToDouble(storedDouble);
            Assert.Equal(num, decodedDouble);
        }

        [Theory]
        [InlineData(123098213721382312)]
        [InlineData(-123123124150982334)]
        [InlineData(0)]
        public void LongStoreTest(long num)
        {
            (int high, int low) = Utility.Split(num);
            long decodedLong = (high, low).ToLong();
            Assert.Equal(num, decodedLong);
        }
    }
}
