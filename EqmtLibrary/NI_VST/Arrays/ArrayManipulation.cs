using System;

namespace NationalInstruments.SystemsEngineering.ArrayManipulation
{
    class ArrayManipulation
    {
        public static T[] Merge2DArrayInto1DArray<T>(T[,] sourceArray)
        {
            T[] desinationArray = new T[sourceArray.Length];
            int rows = sourceArray.GetLength(0);
            int cols = sourceArray.GetLength(1);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    desinationArray[i * cols + j] = sourceArray[i, j];
            return desinationArray;
        }

        public static T[,] Split1DArrayInto2DArray<T>(T[] sourceArray, int rows, int cols)
        {
            T[,] destinationArray = new T[rows, cols];
            if (sourceArray.Length != destinationArray.Length)
                throw new ArgumentException("The length of the source array must match the length of the destination array.");
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    destinationArray[i, j] = sourceArray[i * cols + j];
            return destinationArray;
        }
    }
}
