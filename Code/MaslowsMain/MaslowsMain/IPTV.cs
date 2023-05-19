using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MaslowsMain
{
    public class IPTV
    {
        string _IPADDRESS, _PORT;

        public ControlSystem cs;

        public string name;

        public IPTV(string ipAddr, int port, string name, ControlSystem contsys)
        {
            cs = contsys;

            this.name = name;
            _IPADDRESS = ipAddr;
            _PORT = port.ToString();

            //initiate comms
            Task.Run(() =>
            {
                PushButton(0);
            });
        }

        public void PushButton(int btnPressed)
        {
            string btnCodeToSend = "";
            switch (btnPressed)
            {
                //Directional Pad
                case 0:
                    btnCodeToSend = "19";
                    break;
                case 1:
                    btnCodeToSend = "21";
                    break;
                case 2:
                    btnCodeToSend = "23";
                    break;
                case 3:
                    btnCodeToSend = "22";
                    break;
                case 4:
                    btnCodeToSend = "20";
                    break;

                //Ch + -
                case 5:
                    btnCodeToSend = "166";
                    break;
                case 6:
                    btnCodeToSend = "167";
                    break;

                //Function Btns
                case 7:
                    btnCodeToSend = "4";
                    break;
                case 8:
                    btnCodeToSend = "23";
                    break;

                //Numpad
                case 9:
                    btnCodeToSend = "8";
                    break;
                case 10:
                    btnCodeToSend = "9";
                    break;
                case 11:
                    btnCodeToSend = "10";
                    break;
                case 12:
                    btnCodeToSend = "11";
                    break;
                case 13:
                    btnCodeToSend = "12";
                    break;
                case 14:
                    btnCodeToSend = "13";
                    break;
                case 15:
                    btnCodeToSend = "14";
                    break;
                case 16:
                    btnCodeToSend = "15";
                    break;
                case 17:
                    btnCodeToSend = "16";
                    break;
                case 18:
                    btnCodeToSend = "7";
                    break;

                //Color Btns
                case 19:
                    btnCodeToSend = "183";
                    break;
                case 20:
                    btnCodeToSend = "184";
                    break;
                case 21:
                    btnCodeToSend = "185";
                    break;
                case 22:
                    btnCodeToSend = "186";
                    break;
            }

            try
            {
                SendHTTPRequest(btnCodeToSend);
            }
            catch (Exception ex)
            {
                cs.logger.WriteLine(ex.ToString());
            }
        }

        public void SendHTTPRequest(string btnCodeToSend)
        {
            Task.Run(() =>
            {
                cs.logger.WriteLine("Trying to send a command to " + name);
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://" + _IPADDRESS + ":" + _PORT + "/api/action");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 100;

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"sender\": \"Web Application Client v2\",\"command\":{\"type\":\"System.Action\",\"text\":\"PressKey\",\"value\":\"" + btnCodeToSend + "\"}}";

                    streamWriter.Write(json);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    result = result.Replace('{', '(');
                    result = result.Replace('}', ')');
                    cs.logger.WriteLine("Received Response from " + name + ": " + result.ToString());
                }
                cs.logger.WriteLine("Command sent");
            });
        }
    }
}
