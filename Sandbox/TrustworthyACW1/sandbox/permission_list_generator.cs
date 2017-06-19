//andywm, 2017, UoH 08985 ACW1
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrustworthyACW1
{
    public partial class Sandbox
    {
        //----------------------------------------------------------------------
        //----------Class Attribute Declarations--------------------------------
        //----------------------------------------------------------------------

        private static List<Aggregate> mPermissionsTree
            = new List<Aggregate>();

        //----------------------------------------------------------------------
        //----------Implementation Code-----------------------------------------
        //----------------------------------------------------------------------
        
        /// <summary>
        /// Constructs a representation of available system permissions using
        /// the aggregate class. Event Fields not populated.
        /// </summary>
        /// <returns></returns>
        public static List<Aggregate> genPermissionAggregate()
        {
            var permissions = querySystemPermissions();
            foreach (var permission in permissions)
            {
                //suppressed permissions
                if (permission == typeof(
                    System.Security.Permissions.PublisherIdentityPermission) ||
                    permission == typeof(
                    System.Security.Permissions.KeyContainerPermission))
                {
                    continue;
                }

                Aggregate current = Discover(permission, permission.Name);

                if (current.handleAs != Aggregate.HANDLE_AS.INVALID)
                {
                    mPermissionsTree.Add(current);
                }
            }

            Aggregate manual = Discover(typeof(System.Net.SocketPermission),
                typeof(System.Net.SocketPermission).Name);

            if (manual.handleAs != Aggregate.HANDLE_AS.INVALID)
            {
                mPermissionsTree.Add(manual);
            }

            removeRedundantElements();

            return mPermissionsTree;
        }


        /// <summary>
        /// Recurssively parses an obect, and all objects required to build that
        /// object, ad infinitium; building a branching tree of aggregate
        /// structs of the nth degree.
        /// 
        /// **WARNING: Ensure any object passed to this does not take itself as
        /// a argument in any of its constructors.**
        /// </summary>
        /// <param name="t"></param>
        /// <param name="identifier"></param>
        /// <returns>Aggregate</returns>
        static Aggregate Discover(Type t, string identifier)
        {
            //Base is current to level aggregate.
            Aggregate Base = new Aggregate();
            Base.aggregation = new List<Aggregate>();
            Base.name = t.Name;//identifier;
            Base.type = t;

            if (t.IsEnum)
            {
                //if enum, flag to handle as enumeration. Populate aggregation.
                Base.handleAs = Aggregate.HANDLE_AS.ENUM;
                var names = Enum.GetNames(t);

                foreach (var name in names)
                {
                    Aggregate value = new Aggregate();
                    value.handleAs = Aggregate.HANDLE_AS.OPTION;
                    value.name = name;
                    value.type = value.GetType();
                    //add enumeration to current base aggregation.
                    Base.aggregation.Add(value);
                }
            }
            else if (t.IsClass && !t.IsInterface && !t.IsAbstract)
            {
                //if a real class, flag as class, populate aggregation
                //(representing all data needed to construct the class).

                /***************************************************************
                WARNING: As currently implemented any object which takes 
                  itself as a constructor argument will cause an infinite loop.
                  Code does not deal with hard links.
                ***************************************************************/

                Base.handleAs = Aggregate.HANDLE_AS.CLASS;
                Base.type = t;

                //gen aggregates for construtors
                if (!isSystemType(t))
                {
                    var constructors = t.GetConstructors();
                    foreach (var c in constructors)
                    {
                        var parameters = c.GetParameters();

                        Aggregate constructor = new Aggregate();
                        if (parameters.Length != 0)
                        {
                            //mark as constructor.
                            //PARAMS means aggregate is a constructor.
                            constructor.name = "<constructor>";
                            constructor.handleAs = Aggregate.HANDLE_AS.PARAMS;
                            constructor.aggregation = new List<Aggregate>();
                            constructor.type = t;
                        }
                        else continue; // cannot be constructed.

                        foreach (var p in parameters)
                        {
                            Aggregate parm;
                            //check if system class.
                            if (!isSystemType(p.ParameterType))
                            {
                                //if not recurse on type.
                                parm = Discover(
                                p.ParameterType, p.Name);
                            }
                            else
                            {
                                //otherwise hangle as SYSCLASS.
                                parm = new Aggregate();
                                parm.handleAs = Aggregate.HANDLE_AS.SYSCLASS;
                                parm.name = p.Name;
                                parm.type = p.ParameterType;
                            }
                            //add parameter to constructor aggregatuion.
                            constructor.aggregation.Add(parm);
                        }
                        //add constructor to current base aggregation.
                        Base.aggregation.Add(constructor);
                    }
                }
                else
                {
                    //if of "primitive" type and not an enumeration. Then flag
                    //as a SYSCLASS. (this means do not look for a constructor)
                    Base.handleAs = Aggregate.HANDLE_AS.SYSCLASS;
                    Base.name = identifier;
                    Base.type = t;
                }
            }
            else
            {
                //is abstract or an inteface. Cannot implement.
                Base.handleAs = Aggregate.HANDLE_AS.INVALID;
            }

            return Base;
        }

        /// <summary>
        /// Takes the aggregate which represents the largest constructor.
        /// Additionally, finds and retains the PermissionState constructor.
        /// All other aggregates are dropped.
        /// </summary>
        static void removeRedundantElements()
        {
            foreach (var perm in mPermissionsTree)
            {
                int numberOfOptions = 0;
                Aggregate currentLargest = new Aggregate();
                Aggregate genericOption = new Aggregate();
                foreach (var constructor in perm.aggregation)
                {
                    if (constructor.aggregation.Count > numberOfOptions)
                    {
                        bool generic = false;
                        foreach (var agg in constructor.aggregation)
                        {
                            if (agg.name == "PermissionState") generic = true;
                        }
                        if (!generic)
                        {
                            currentLargest = constructor;
                            numberOfOptions = constructor.aggregation.Count;
                        }
                        else
                        {
                            genericOption = constructor;
                        }
                    }
                }
                perm.aggregation.Clear();
                perm.aggregation.Add(genericOption);
                perm.aggregation.Add(currentLargest);
            }
        }

        //----------------------------------------------------------------------
        //----------Utilities---------------------------------------------------
        //----------------------------------------------------------------------

        /// <summary>
        /// Returns a list of all non-abstract classes which inherit from
        /// System.Security.Permissions.
        /// </summary>
        /// <returns>IEnumerable<Type></returns>
        static IEnumerable<Type> querySystemPermissions()
        {
            //Maybe Query IPermission instead of System.Security.Permission?
            return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t => t.GetTypes())
                    .Where(t => t.IsClass &&
                        t.Namespace == "System.Security.Permissions" &&
                        t.Name.EndsWith("Permission"));

        }

        /// <summary>
        /// Given a type, query .net TypeCodes to determine if there is a match.
        /// If there is returns true, else returns false.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        static bool isSystemType(Type t)
        {
            //make this a map? It looks like it would be painfully inefficient.

            string typeName = t.Name;
            if (typeName.EndsWith("[]"))
                typeName = typeName.Remove(typeName.Length - 2, 2);

            foreach (TypeCode sysT in Enum.GetValues(typeof(TypeCode)))
            {
                if (typeName == sysT.ToString())
                    return true;
            }
            return false;
        }
    }
}
//andywm, 2017, UoH 08985 ACW1