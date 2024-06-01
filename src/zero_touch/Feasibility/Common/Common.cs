using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// A class containing methods for common math operations.
    /// </summary>
    public class Math
    {
        // hides the overall class as a node.
        private Math() { }

        /// <summary>
        /// Add two integer values.
        /// </summary>
        /// <param name="a">the first integer</param>
        /// <param name="b">the second integer</param>
        /// <returns name="value">the added value</returns>
        public static int IntAdd(int a, int b) 
        {
            // returns the addition of two integers.
            return a + b; 
        }

        /// <summary>
        /// Divides a range into a required number count.
        /// </summary>
        /// <param name="start">the start number of the range</param>
        /// <param name="end">the end number of the range</param>
        /// <param name="count"></param>
        /// <returns name="numbers">the range of numbers</returns>
        /// <exception cref="ArgumentException"></exception>
        public static List<double> Range(double start=0, double end=9, int count=10) 
        {
            // create a new list to hold the numbers.
            var range = new List<double>();

            // check if the count input is less than 1.
            if (count < 2)
                throw new ArgumentException("Count must be at least 2.");

            // create the step value.
            double step = (end - start) / (count - 1);

            // add the range of values to the list.
            for (int i = 0; i < count; i++)
            { 
                range.Add(start + i * step);  
            }

            return range;
        }
    }
}
