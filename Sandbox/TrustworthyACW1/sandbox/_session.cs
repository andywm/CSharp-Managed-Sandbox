//andywm, 2017, UoH 08985 ACW1
using System;
using System.Security;
using System.Security.Policy;
using TrustworthyACW1.utilities;

namespace TrustworthyACW1
{
    public partial class Sandbox
    {
        private class Session : MarshalByRefObject
        {
            //------------------------------------------------------------------
            //----------Class Attribute Declarations----------------------------
            //------------------------------------------------------------------

            private AppDomainSetup mAppDomainSetup = new AppDomainSetup();
            private PermissionSet mPermissions;
            private AppDomain mDomain;
            
            private string[] mArgumentList;
            private string mExecutablePath;

            //------------------------------------------------------------------
            //----------Implementation Code-------------------------------------
            //------------------------------------------------------------------

            /// <summary>
            /// Initialise session, execution is deffered.
            /// </summary>
            /// <param name="permissions"></param>
            /// <param name="ex_path"></param>
            /// <param name="arguments"></param>
            public Session(PermissionSet permissions, string ex_path,
                string[] arguments, string jail_path = null)
            {
                mPermissions = permissions;
                mExecutablePath = ex_path;
                mArgumentList = arguments;

                if (jail_path == null)
                {
                    mAppDomainSetup.ApplicationBase = cdDotDot(mExecutablePath);
                }
                else
                {
                    mAppDomainSetup.ApplicationBase = jail_path;
                }

                StrongName fullTrustAssembly = typeof(Session)
                    .Assembly.Evidence.GetHostEvidence<StrongName>();

                mDomain = AppDomain.CreateDomain(
                    "Sandbox",
                    null,
                    mAppDomainSetup,
                    mPermissions,
                    fullTrustAssembly);
            }

            /// <summary>
            /// Begin Execution of target assembly in sandboxed domain.
            /// </summary>
            public void execute()
            {
                try
                {
                    mDomain.ExecuteAssembly(mExecutablePath, mArgumentList);
                }
                catch (Exception e)
                {
                    Log.protectionFault(e.ToString());
                }
            }

            //------------------------------------------------------------------
            //----------Utilities-----------------------------------------------
            //------------------------------------------------------------------

            private string cdDotDot(string path)
            {
                string[] pathSplit = path.Split('\\');
                string removeThis = pathSplit[pathSplit.Length - 1];

                return path.Substring(
                    0,
                    mExecutablePath.Length - removeThis.Length);
            }
        }
    }
}
//andywm, 2017, UoH 08985 ACW1
