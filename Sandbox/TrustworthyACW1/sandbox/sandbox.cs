//andywm, 2017, UoH 08985 ACW1
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

//------------------------------------------------------------------------------
namespace TrustworthyACW1
{
    public partial class Sandbox
    {
        //----------------------------------------------------------------------
        //----------Class Attribute Declarations--------------------------------
        //----------------------------------------------------------------------

        private PermissionSet mPermissions;
        private Session mSession;
        public bool error { get; set; }
        public Exception exception { get; private set; }

        //----------------------------------------------------------------------
        //----------Implementation Code-----------------------------------------
        //----------------------------------------------------------------------

        /// <summary>
        /// Ready sandbox, implictly initialises a Session (Execution deffered).
        /// 
        /// Should it fail, error status and exeception thrown are queryable.
        /// </summary>
        /// <param name="permissionList"></param>
        /// <param name="ex_path"></param>
        /// <param name="args"></param>
        public Sandbox(List<Aggregate> permissionList,
            string ex_path, string args, string jail_path = null)
        {
            //init with no permissions.
            mPermissions = new PermissionSet(PermissionState.None);

            error = false;
            exception = null;          
            foreach (var permission in permissionList)
            {
                try { addPermission(permission); }
                catch (Exception e)
                {
                    exception = e;
                    error = true;
                    break;
                }
            }

            if (!error)
            {
                string[] defaultArgs = { };

                try
                {
                    mSession = new Session(mPermissions, 
                        ex_path, 
                        args == null? defaultArgs : args.Split(','),
                        jail_path);
                }
                catch (Exception e)
                {
                    error = true;
                    exception = e;
                }
            }
        }

        /// <summary>
        /// Invoke session execute method.
        /// Program flow passed to another assembly.
        /// </summary>
        public void execute()
        {
            mSession.execute();
        }

        /// <summary>
        /// attempts to add a permission to the permission set. 
        /// 
        /// NoThrow, will fail silently.
        /// </summary>
        /// <param name="permissionAgg"></param>
        private void addPermission(Aggregate permissionAgg)
        {
            if (permissionAgg == null) return;

            //picks the constructor data which has been flaged for use.     
            foreach (var permSubSet in permissionAgg.aggregation)
            {
                if (permSubSet.value.bData)
                {
                    permissionAgg = permSubSet;
                    break;
                }
            }

            var permission = buildPermission(permissionAgg);
            if (permission != null)
            { 
                dynamic castedParam = Convert.ChangeType(permission,
                    permissionAgg.type);
                mPermissions.AddPermission(castedParam); 
            }
        }

        /// <summary>
        /// Give a aggregation root representing a permission, this uses a 
        /// depth first search to build that permission. 
        /// 
        /// Returning the permission, or null (failed).
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private object buildPermission(Aggregate res)
        {
            //constructor parameter list.
            List<object> pList = new List<object>();

            switch (res.handleAs)
            {
                case Aggregate.HANDLE_AS.PARAMS:
                case Aggregate.HANDLE_AS.CLASS:
                    //non-trivial object, must recurse.
                    foreach (var agg in res.aggregation)
                    {
                        pList.Add(buildPermission(agg));
                    }
                    
                    break;
                case Aggregate.HANDLE_AS.SYSCLASS:
                    //is of base pritive type - construct.
                    if (res.type == typeof(bool))
                    {
                        return res.value.bData;
                    }
                    else
                    {
                        if (res.value.sData == null) res.value.sData = "";
                        return res.value.sData;
                    }
                case Aggregate.HANDLE_AS.ENUM:
                    //build enumeration.
                    var enumeration = Activator.CreateInstance(res.type);
                    Type enumT = Type.GetType(res.type.FullName);

                    foreach (var agg in res.aggregation)
                    {
                        dynamic result = 
                            Convert.ChangeType(enumeration, res.type);

                        //perform bitwise logic on enumeration.
                        if (agg.value.bData == true)
                        {
                            var val = Enum.Parse(enumT, agg.name);
                            dynamic eVal = Convert.ChangeType(val, res.type);
                            result |= eVal;
                            enumeration = result;
                        }
                    }
                    return enumeration;
            }

            //atempt to build object.
            object[] objs = pList.ToArray();        
            return Activator.CreateInstance(res.type, objs);
        }
    }
}
//andywm, 2017, UoH 08985 ACW1
