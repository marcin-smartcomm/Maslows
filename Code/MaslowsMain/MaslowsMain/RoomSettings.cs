using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaslowsMain
{
    public class RoomSettings
    {
        public string roomName { get; set; }
        public string[] sources { get; set; }
        public short sourceSelected { get; set; }
        public short neighbourRoom { get; set; }
        public bool joined { get; set; }
        public bool MasterPanel { get; set; }
        public bool slave { get; set; }
        public int volume { get; set; }
    }
}
