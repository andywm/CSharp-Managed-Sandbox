//andywm, 2017, UoH 08985 ACW1
using System;
using System.Collections.Generic;
using TrustworthyACW1.utilities;

namespace TrustworthyACW1.user_interface
{
    public class CommandLine
    {
        //----------------------------------------------------------------------
        //----------Class Attribute Declarations--------------------------------
        //----------------------------------------------------------------------

        private string mConfig;

        public bool supress { get; internal set; } = false;
        public bool keepTerminalAlive { get; internal set; } = false;

        public string executionPath { get; internal set; } = null;
        public string jailPath { get; internal set; } = null;
        public string argumentList { get; internal set; } = null;

        public List<Aggregate> permissions { get; internal set; } 
            = new List<Aggregate>();

        const string args_delim = "-a";
        const string conf_delim = "-c";
        const string exec_delim = "-ex";
        const string jail_delim = "-jail";
        const string supr_delim = "--supress";
        const string retain_delim = "--retain";

        //----------------------------------------------------------------------
        //----------Implementation Code-----------------------------------------
        //----------------------------------------------------------------------

        public CommandLine(string[] str)
        {
            if (str.Length == 1 && str[0] == "--help")
            {
                manPage(); 
            }
            else
            {
                interpreter(str);
            }
        }

        private void interpreter(string[] str)
        {
            try
            {
                int offset = 1;
                for (int i = 0; i < str.Length; i++)
                {
                    string arg = str[i];
                    switch (arg)
                    {
                        case args_delim:

                            while (queryIsDelimiter(str[i + offset])
                                && i + offset < str.Length)
                            {
                                argumentList+=(str[i + offset]);
                                offset++;
                            }
                            i += offset;
                            break;
                        case conf_delim:
                            if (queryIsDelimiter(str[i + offset]))
                            {
                                mConfig = str[i + offset];
                            }
                            offset++;
                            break;
                        case exec_delim:
                            if (queryIsDelimiter(str[i + offset]))
                            {
                                executionPath = str[i + offset];
                            }
                            offset++;
                            break;
                        case jail_delim:
                            if (queryIsDelimiter(str[i + offset]))
                            {
                                jailPath = str[i + offset];
                            }
                            offset++;
                            break;
                        case supr_delim:
                            supress = true;
                            break;
                        case retain_delim:
                            keepTerminalAlive = true;
                            break;
                    }
                }
                if (mConfig == null) throw new Exception("No Config File");
                if (executionPath == null) throw new Exception("No executable");
            }
            catch (Exception e)
            {
                Log.advisory("Bad Arguments...");
            }
        }

        //----------------------------------------------------------------------
        //----------Utilities---------------------------------------------------
        //----------------------------------------------------------------------

        private bool queryIsDelimiter(string str)
        {
            if (str == args_delim) return true;
            if (str == conf_delim) return true;
            if (str == exec_delim) return true;
            if (str == jail_delim) return true;
            if (str == supr_delim) return true;
            if (str == retain_delim) return true;

            return false;
        }

        private void manPage()
        {
            Console.WriteLine("Sandboxing Utility, Trustworthy ACW1");
            Console.WriteLine("andywm 2017\n");
            Console.WriteLine("Usage --");
            Console.WriteLine("program.exe" + 
                " [{0} <...>] " +
                " {1} path " +
                " {2} path " + 
                " [{3} path] " + 
                " [{4}] " + 
                " [{5}] ", args_delim, conf_delim, exec_delim,
                jail_delim, supr_delim, retain_delim);
            Console.WriteLine("Synopsis --");
            Console.WriteLine("A utility program to sandbox" +
                " .net applications.");
            Console.WriteLine("[{0} <...>] : argument list.", args_delim);
            Console.WriteLine("{0} path : config file location", conf_delim);
            Console.WriteLine("{0} path : executable location", exec_delim);
            Console.WriteLine("[{0} path] : chroot jail path", jail_delim);
            Console.WriteLine("   -:defaults to same folder as executable");
            Console.WriteLine("[{0}] : supress sandbox messages", supr_delim);
            Console.WriteLine("[{0}] : keep terminal alive.", retain_delim);
        }
    }
}
//andywm, 2017, UoH 08985 ACW1
