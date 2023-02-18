using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaslowsMain
{
    public static class TPLocations
    {
        public static List<string> availableLocations = new List<string>() {
            "Meeting Room 5.11", 
            "Meeting Room 4.17", 
            "Meeting Room 3.17", 
            "Meeting Room 2.17", 
            "Meeting Room 2.16", 
            "Meeting Room 2.14", 
            "L1 Event Space",
            "L1 Private Dining",
            "Meeting Room 2.15"
        };

        public static bool[] availableLocationsIndex = { true, true, true, true, true, true, true, true, true };
    }
}
