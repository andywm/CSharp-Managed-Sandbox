//andywm, 2017, UoH 08985 ACW1
using System;
using System.Collections.Generic;

namespace TrustworthyACW1
{
    public class Aggregate
    {
        public class EventData
        {
            public string sData { get; set; } 
            public bool bData { get; set; } 
        }

        public enum HANDLE_AS {CLASS, SYSCLASS, PARAMS, ENUM, OPTION, INVALID};
        public HANDLE_AS handleAs { get; set; }

        public string name { get; set; }
        public Type type { get; set; }
        public List<Aggregate> aggregation;// { get; set; }

        public EventData value { get; set; } = new EventData();
        public bool isMutuallyExclusive { get; set; }
    }
}
//andywm, 2017, UoH 08985 ACW1