//andywm, 2017, UoH 08985 ACW1
using System;

namespace TrustworthyACW1.utilities
{
    public static class Log
    {
        /// <summary>
        /// Enable or disable logging to console. 
        /// </summary>
        public static bool enabled { get; set; }

        /// <summary>
        /// If console logging is enabled, this logs the error message with the
        /// banner of protection fault.
        /// </summary>
        /// <param name="error"></param>
        public static void protectionFault(string error)
        {
            if (!enabled) return;
            Console.WriteLine("Protection Fault!");
            Console.WriteLine(new String('-', 30));
            Console.WriteLine(error);
        }

        /// <summary>
        /// If console logging is enabled, this logs the error message with the
        /// banner of advisory.
        /// </summary>
        /// <param name="error"></param>
        public static void advisory(string error)
        {
            if (!enabled) return;
            Console.WriteLine("Advisory!");
            Console.WriteLine(new String('-', 30));
            Console.WriteLine(error);
        }
    }
}
//andywm, 2017, UoH 08985 ACW1