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
    }
}
