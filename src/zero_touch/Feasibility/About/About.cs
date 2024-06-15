using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace About
{
    /// <summary>
    /// Wrapper class for the about information.
    /// </summary>
    public class About
    {
        // this hides the overall class as a node.
        private About() { }

        
        /// <summary>
        /// Capitalizes the first letter of a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns></returns>
        private static string CapitalizeFirstLetter(string input) 
        {
            if (string.IsNullOrEmpty(input)) 
            { 
                return input;
            } 
            return $"{char.ToUpper(input[0])}{input.Substring(1)}";
        }


        /// <summary>
        /// Provides information about the Feasibility plugin for Dynamo.
        /// </summary>
        /// <returns></returns>
        public static string AboutFeasibility() 
        { 
            // get the username.
            string userName = CapitalizeFirstLetter(Environment.UserName);

            // create the about information.
            string aboutFeasibility = $"Hello {userName}, Feasibility is designed as a flexible toolkit to automate " +
                $"various aspects of urban layout design using Dynamo ZeroTouch nodes. Leveraging the power of Dynamo, " +
                $"this solution provides an efficient and scalable way to design and manipulate parking layouts, erven " +
                $"(plot) layouts, and building massing. Feasibility encompasses a variety of computational operations " +
                $"required to design complex urban layouts, making it an invaluable tool for architects, urban planners, " +
                $"civil engineers, and other professionals in the AEC industry. The first iteration of the package focuses " +
                $"on parking layouts. Feasibility is developed by Bayo Windapo of Onile. For inquiries, visit the Onile website " +
                $"at onile.ai.";

            return aboutFeasibility;
        }
    }
}
