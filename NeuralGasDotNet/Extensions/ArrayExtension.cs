using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralGasDotNet.Extensions
{
    public static class ArrayExtension
    {
        public static T[] GetRow<T>(this T[,] input2DArray, int row) where T : IComparable
        {
            var width = input2DArray.GetLength(0);
            var height = input2DArray.GetLength(1);

            if (row >= width)
                throw new IndexOutOfRangeException("Row Index Out of Range");
            // Ensures the row requested is within the range of the 2-d array


            var returnRow = new T[width];
            for (var i = 0; i < height; i++)
                returnRow[i] = input2DArray[row, i];

            return returnRow;
        }
    }
}
