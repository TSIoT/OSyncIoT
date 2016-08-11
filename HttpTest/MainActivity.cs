using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;


using IoTRepublicPlus.Utility;

namespace HttpTest
{
    [Activity(Label = "HttpTest", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        //string targetIp = "192.168.156.199";
        //string targetIp = "184.173.63.172";
        string targetIp = "192.168.156.92";
        TcpClient tcp, tcp2;
        
        NetworkStream stream, stream2;
        StreamReader sr,sr2;
        StreamWriter sw,sw2;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);            

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);
            button.Click += Button_Click;

            Button button1 = FindViewById<Button>(Resource.Id.button1);
            button1.Click += Button1_Click;


            Button button2 = FindViewById<Button>(Resource.Id.button2);
            button2.Click += Button2_Click;

            Button button3 = FindViewById<Button>(Resource.Id.button3);
            button3.Click += Button3_Click;

            Button button4 = FindViewById<Button>(Resource.Id.button4);
            button4.Click += Button4_Click;

            //button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };
        }

        
        private void Button_Click(object sender, EventArgs e)
        {
            this.tcp = new TcpClient(this.targetIp, 6210); // 1.設定 IP:Port 2.連線至伺服器            
            this.stream = new NetworkStream(this.tcp.Client);
            this.sr = new StreamReader(stream);
            this.sw = new StreamWriter(stream);
            
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //sw.WriteLine("Hello!"); // 將資料寫入緩衝
            //string jsonData = "{\"USER\":{\"ID\":\"DDR\",\"PW\":\"AAA\"},\"USER2\":{\"ID\":\"DDR\",\"PW\":\"AAA\"}}";
            //string jsonData = "{\"USER\":{\"ID\":\"DDR\",\"PW\":\"AAA\"},\"USER2\":{\"ID\":\"";
            string jsonData = @"{""USER"":{""ID"":""DDR"",""PW"":""AAA"",""Array"":[{""Element"":""firstElemtn""},{""Element"":""secondElemtn""},{""Element"":""thirdElemtn""}]}}";

            //string jsonData = "{\"USER\":{\"ID\":\"DDR\",\"PW\":\"AAA\",\}}";
            //JsonUtility jsonUtility = new JsonUtility();
            JObject jsonObj = JsonUtility.LoadJsonData(jsonData);
            IoTRepublicPlus.IoTUtility ioTUtility = new IoTRepublicPlus.IoTUtility("IUDP1.0",'\0');
            /*
            JArray arrayObj = jsonObj["USER"]["Array"].ToObject<JArray>();
            
            for(int i=0;i<arrayObj.Count;i++)
            {
                string temp=arrayObj[i]["Element"].ToString();
                arrayObj[i]["Element"] = i.ToString() + " element";
            }

            jsonObj["USER"]["Array"][0]["Element"] = "just try";

            int arrayCount = jsonObj["USER"]["Array"].ToObject<JArray>().Count;
            */
            string firstKey = "";
            string value = "";

            if (jsonObj!=null)
            {
                firstKey = JsonUtility.GetFirstKeyName(jsonObj);
                value = JsonUtility.GetValueInFirstObject(jsonObj, "ID");
                JsonUtility.SetValueInFirstObject(jsonObj, "ID","Loki");
                int arrayLength = JsonUtility.GetArrayLengthInFirstObject(jsonObj,"Array");

                for(int i=0;i< arrayLength;i++)
                {
                    string arrayElement= JsonUtility.GetArrayValueInFirstObject(jsonObj, "Array", i, "Element");                    
                }
                //JsonUtility.GetArrayValueInFirstObject(jsonObj, "Array", 8, "Element");

            }

            List<char> data = new List<char>();
            IoTRepublicPlus.IoTPackage package = new IoTRepublicPlus.IoTPackage("IUDP1.0","0","0", data);
             

            Socket mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mainSocket.Connect(this.targetIp, 6210);
            mainSocket.Send(package.DataVectorForSending);

            byte[] readBuffer = new byte[2000];
            List<char> responseData = new List<char>(2000);

            IAsyncResult asyncReader = mainSocket.BeginReceive(readBuffer, 0, readBuffer.Length, 0, null, null);
            System.Threading.WaitHandle handle = asyncReader.AsyncWaitHandle;
            
               
            bool dataArrived = handle.WaitOne(1000, false);
            if (dataArrived)
            {
                //int n = networkStream.EndRead(asyncReader);
                int n = mainSocket.EndReceive(asyncReader);
                for (int i = 0; i < n; i++)
                    responseData.Add((char)readBuffer[i]);

                IoTRepublicPlus.IoTUtility.GetPackageError error = IoTRepublicPlus.IoTUtility.GetPackageError.UnknownError;
                IoTRepublicPlus.IoTPackage recvPackage = ioTUtility.GetCompletedPackage(responseData, ref error);

                IoTRepublicPlus.IoTCommand cmd = new IoTRepublicPlus.IoTCommand(
                    IoTRepublicPlus.IoTCommand.Type.Management,
                    "Dis_NPx",
                    "0"
                    );
                

            }
           

            //JObject jsonObj = JObject.Parse(jsonData);

            //JObject child1 = jsonObj["USER"].ToObject<JObject>();

            //IList<string> keys= jsonObj.Properties().Select(p => p.Name).ToList();
            //List<string> keys = child1.Properties().Select(p => p.Name).ToList();


            //string jsonData = "{\"REGISTER\":{\"TYPE\":\"Manager\",\"ID\":\"DDR\",\"PW\":\"AAA\"}}";
            //string jsonData = "{\"REGISTER\":{\"TYPE\":\"Mobile\",\"ID\":\"DDR\",\"PW\":\"AAA\"},\"USER2\":{\"ID\":\"DDR\",\"PW\":\"AAA\"}}";
            //sw.Write("Hello\0");

            /*
            //Cloud service test
            string jsonData = "{\"REGISTER\":{\"TYPE\":\"MANAGER\",";
            sw.Write(jsonData);
            sw.Flush(); // 刷新緩衝並將資料上傳到伺服器

            jsonData = "\"ID\":\"DDR\",\"PW\":\"AAA\"}}";
            sw.Write(jsonData);
            sw.Flush(); // 刷新緩衝並將資料上傳到伺服器
            */

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.tcp2 = new TcpClient(this.targetIp, 6210); // 1.設定 IP:Port 2.連線至伺服器            
            this.stream2 = new NetworkStream(this.tcp2.Client);
            this.sr2 = new StreamReader(stream2);
            this.sw2 = new StreamWriter(stream2);

        }

        private void Button3_Click(object sender, EventArgs e)
        {
            string jsonData = "{\"REGISTER\":{\"TYPE\":\"END_NODE\",";
            sw2.Write(jsonData);
            sw2.Flush(); // 刷新緩衝並將資料上傳到伺服器

            jsonData = "\"ID\":\"DDR\",\"PW\":\"AAA\"}}";
            sw2.Write(jsonData);
            sw2.Flush(); // 刷新緩衝並將資料上傳到伺服器
            
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            this.tcp.Close();

        }



    }
}


