using Newtonsoft.Json;
using System;
using System.IO;

namespace MaslowsMain
{
    public static class FileOperations
    {
        public static RoomSettings loadJson(string roomNum)
        {
            StreamReader sr = new StreamReader("../Nvram/Room" + roomNum +  ".json");

            string json = sr.ReadToEnd();
            sr.Close();

            return JsonConvert.DeserializeObject<RoomSettings>(json);
        }

        public static void UpdateSettings(string roomNum, RoomSettings rs)
        {
            File.Delete("../Nvram/Room" + roomNum + ".json");
            File.WriteAllText(
                "../Nvram/Room" + roomNum + ".json",
                JsonConvert.SerializeObject(rs, Formatting.Indented));
        }
    }
}
