using Newtonsoft.Json;
using System.IO;

namespace MaslowsMain
{
    public static class FileOperations
    {
        public static RoomSettings loadJson(string roomNum)
        {
            StreamReader sr = new StreamReader("../nvram/Room" + roomNum +  ".json");

            string json = sr.ReadToEnd();
            sr.Close();

            return JsonConvert.DeserializeObject<RoomSettings>(json);
        }

        public static void UpdateSettings(string roomNum, RoomSettings rs)
        {
            File.Delete("../nvram/Room" + roomNum + ".json");
            File.WriteAllText(
                "../nvram/Room" + roomNum + ".json",
                JsonConvert.SerializeObject(rs, Formatting.Indented));
        }
    }
}
