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
            "MEETING ROOM 501", 
            "MEETING ROOM 401", 
            "MEETING ROOM 301", 
            "MEETING ROOM 204",
            "MEETING ROOM 203",
            "MEETING ROOM 201", 
            "EVENT SPACE",
            "PRIVATE DINING",
            "MEETING ROOM 202"
        };

        public static bool[] availableLocationsIndex = { true, true, true, true, true, true, true, true, true };
    }
}
