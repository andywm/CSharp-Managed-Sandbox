//andywm, 2017, UoH 08985 ACW1
using System;
using TrustworthyACW1.user_interface;
using TrustworthyACW1.utilities;

namespace TrustworthyACW1
{
    //key password: alphabeta1
    class Application
    { 
        static void Main(string[] args)
        {
            bool keepAlive = false, terminate = false, onceOnly = false;
            string ex_path=null, arguments=null, jail_path=null;

            CommandLine cmd = null;
            GUI gui = null;

            var permissionsData = Sandbox.genPermissionAggregate();
            
            do
            {
                if (args.Length == 0)
                {
                    try
                    {
                        gui = new GUI(permissionsData);
                    }
                    catch(Exception e)
                    {
                        Log.protectionFault(e.ToString());
                    }
                    keepAlive = gui.keepTerminalAlive;
                    terminate = gui.terminate;
                    ex_path = gui.path.value.sData;
                    jail_path = gui.path.value.sData;
                    arguments = gui.arguments.value.sData;
                    Log.enabled = gui.supress;
                }
                else
                {
                    try
                    {
                        cmd = new CommandLine(args);
                    }
                    catch (Exception e)
                    {
                        Log.protectionFault(e.ToString());
                    }
                keepAlive = cmd.keepTerminalAlive;
                    terminate = true;
                    onceOnly = true;
                    ex_path = cmd.executionPath;
                    arguments = cmd.argumentList;
                    Log.enabled = true;
                }

                if (!terminate || onceOnly)
                {
                    Sandbox sb;
                    if (jail_path == null)
                    {
                        sb = new Sandbox(permissionsData,
                                    ex_path,
                                    arguments);
                    }
                    else
                    {
                        sb = new Sandbox(permissionsData,
                                    ex_path,
                                    arguments,
                                    jail_path);
                    }

                    if (!sb.error) sb.execute();
                }
            } while (!terminate);

            //wait?
            if(keepAlive) Console.Read();
        }
    }
}
//andywm, 2017, UoH 08985 ACW1
