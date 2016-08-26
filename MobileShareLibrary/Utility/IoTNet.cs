using System;
using System.Collections.Generic;
using System.Text;

//********for socket************
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
//*****************************

using System.Threading.Tasks; //Task
using System.Diagnostics; //Stop watch

//********IoT Republic************
using IoTRepublicPlus;
using IoTRepublicPlus.Utility;

//using IOTProtocol.Utility;
//using IOTProtocol.JsonUtility;
//*****************************
using ProtocolType = System.Net.Sockets.ProtocolType;
using SocketType = System.Net.Sockets.SocketType;
//********Android WIFI************
#if __ANDROID__
using Android.Content;
using Android.App;
using Android.Net;
using Android.Net.Wifi;
//using OSyncIoT;
#endif

namespace MobileShareLibrary.Utility
{
    /**
     * Method of join IoT Republic
     * */
    public class IoTNet
    {
        public EventHandler GotUDPResponse;
        public EventHandler GotManagerResponse;
        public EventHandler GotResponseCMD;
        public EventHandler GotManagerCMD;
        

        private IoTUtility iotUtility;
        private EndDeviceController endDeviceController;
        //private JsFileController jsFileController;
        private Socket mainSocket = null;
        private Socket requestDevListSocket = null;
        private int broadcastServerPort = 6215;
        private int managerPort = 6210;
        private string managerIPAddress = "";

        //For IoTRepublic protocol
        private string iotProtocolVersion = "IUDP1.0";
        private string managerIoTIP = "";
        private string selfIoTIP = "";
        private int maxRecvBuffer=2000;

        //Register to manager
        private byte[] descriptionBuffer;
        //private char[] descriptionPackageBuffer;
        private List<char> descriptionPackageBuffer;        

        //Listen common command from manager 
        private byte[] cmdReadBuffer;
        private List<char> cmdPackageBuffer;
        //private char[] cmdPackageBuffer;        

        public IoTNet()
        {            
            this.iotUtility = new IoTUtility(this.iotProtocolVersion,IoTPackage.SegmentSymbol);
            this.endDeviceController = new EndDeviceController();            
                        
            this.mainSocket= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            this.requestDevListSocket= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            this.descriptionPackageBuffer = new List<char>(this.maxRecvBuffer);
            this.descriptionBuffer = new byte[this.requestDevListSocket.ReceiveBufferSize];
            
            
        }        

        public bool isConnected()
        {
            return this.mainSocket.Connected;
        }


        public void SendCMD(IoTCommand cmd, string targetIp)
        {
            IoTPackage package = new IoTPackage(this.iotProtocolVersion, this.selfIoTIP, targetIp, cmd.sendedData);           
            this.sendDataToManager(this.mainSocket, package.DataVectorForSending);
        }
    

#region Methods of register
        public void ConnectToManager(string ip, int timeout)
        {
            bool isSucceed = false;
            Task.Run(() =>
            {
#region tcp layer
                //make the socket clean
              
                if (this.mainSocket==null)
                {                    
                    this.mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);                 
                }
                else
                {
                    this.mainSocket.Close();
                    this.mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                    
                

                try
                {
                    IAsyncResult result = this.mainSocket.BeginConnect(ip, this.managerPort, null, null);
                    isSucceed = result.AsyncWaitHandle.WaitOne(1000, true);
                    //this.registedSocket.Connect("192.168.156.92", 6210);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                    isSucceed = false;
                }
#endregion

#region IoT Net layer
                if (isSucceed)// tcp connection created
                {
                    this.managerIPAddress = ip;

                    //send a IP request package
                    IoTPackage package = new IoTPackage(this.iotProtocolVersion,"0","0");                    
                    this.sendDataToManager(this.mainSocket, package.DataVectorForSending);

                    //receive response
                    IoTPackage ipData = this.waitAcompletedPackage(this.mainSocket, timeout);                    
                    
                    if (ipData == null)
                    {
                        isSucceed = false;
                    }
                    else //got IoTIP start regiser
                    {                        
                        this.managerIoTIP = ipData.SorIp;
                        this.selfIoTIP = ipData.DesIp;

                        //Send self device description to manager for register
                        string fileDesc = OSyncUtility.GetSelfFileDescription();
                        IoTDevice selfDevice = new IoTDevice();
                        selfDevice.SetJSDescription(fileDesc);                        
                        IoTPackage devDescriptionPackage = new IoTPackage(
                            this.iotProtocolVersion,
                            this.selfIoTIP,
                            this.managerIoTIP,
                            fileDesc);
                        this.sendDataToManager(this.mainSocket, devDescriptionPackage.DataVectorForSending);

                        this.AskAllDeviceInfo();// request all the end device info on manager

                        this.initNetWorkReader(); //init listenr the command from manager
                        this.startReadNetWorkData();//start common listenr

                    }
                    
                    //this.sendDataToManager(this.registedSocket, sendBuf, 0, n);
                }

                
#endregion
            });
        }

        public void DisconnectFromManager()
        {
            this.mainSocket.Disconnect(true);
        }

        public void SearchManagerIP(int maxSearchTime)
        {            
            UdpClient udpClient = new UdpClient();

            string broadcastAddress = this.getBroadcastAddress();

            if (broadcastAddress.Length < 0)
                return;
         
            IPEndPoint boradCastIp = new IPEndPoint(IPAddress.Parse(broadcastAddress), this.broadcastServerPort);
            byte[] bytes = Encoding.ASCII.GetBytes("Where is IOT Server?");

            Task.Run(() =>
            {
                //UdpReceiveResult receivedResults = udpClient.ReceiveAsync();
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                IAsyncResult asynResult = udpClient.BeginReceive(null, null);
                asynResult.AsyncWaitHandle.WaitOne(maxSearchTime);

                NetworkEventArgs args = new NetworkEventArgs();
                if (asynResult.IsCompleted)
                {
                    //var state = asynResult.sy;
                    udpClient.EndReceive(asynResult, ref remoteIpEndPoint);
                    this.managerIPAddress = remoteIpEndPoint.Address.ToString();
                    //string managerAddress = udpClient.Client.RemoteEndPoint.ToString();
                    args.isSucceed = true;
                    args.Data = this.managerIPAddress;
                }
                else
                {
                    args.isSucceed = false;
                    args.Data = null;
                }

                this.GotUDPResponse(this, args);

                udpClient.Close();
            });


            udpClient.Send(bytes, bytes.Length, boradCastIp);

        }    
       
        /**         
         *maxSearchTime=milliseconde
         **/
        private bool sendDataToManager(Socket socket, byte[] sendData)
        {
            if (socket.Connected)
            {                               
                try
                {
                    socket.Send(sendData);
                }
                catch (Exception)
                {
                    //print(e.ToString());
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private string getBroadcastAddress()
        {
            string resultIP = "";
#if __IOS__
            NetworkInterface[] netInterfaceList = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface netInterface in netInterfaceList)
            {
                if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {

                    foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            int maskAddress = BitConverter.ToInt32(addrInfo.IPv4Mask.GetAddressBytes(), 0);
                            int ipAddress = BitConverter.ToInt32(addrInfo.Address.GetAddressBytes(), 0);
                            int broadcast = ipAddress | (~maskAddress);
                            //IPAddress ipAddress = addrInfo.Address;
                            //address = ipAddress.ToString();
                            resultIP = this.intToIp(broadcast);
                        }
                    }
                }
            }
#elif __ANDROID__            
            WifiManager wifiManager = Application.Context.GetSystemService(Context.WifiService) as WifiManager;
            if (wifiManager != null)
            {
                DhcpInfo dhcpInfo = wifiManager.DhcpInfo;
                int mask = dhcpInfo.Netmask;
                int broadcast = (dhcpInfo.IpAddress & dhcpInfo.Netmask) | ~dhcpInfo.Netmask;
                resultIP = this.intToIp(broadcast);
            }            
#endif
            return resultIP;
        }

        private string intToIp(int addr)
        {
            return ((addr & 0xFF) + "." +
                    ((addr >>= 8) & 0xFF) + "." +
                    ((addr >>= 8) & 0xFF) + "." +
                    ((addr >>= 8) & 0xFF));
        }
#endregion

#region Get device info    
        public List<IoTDevice> GetAllDeviceInfo()
        {
            return this.endDeviceController.GetAllDeviceInfo();
        }             

        public void AskAllDeviceInfo()
        {        
            
            if(this.requestDevListSocket.Connected)
            {
                throw new Exception("The requestDevListSocket is busy, it shouldn't happened");
            }
            else
            {
                //*****************send request all device command*****************
                IoTCommand cmd = new IoTCommand(IoTCommand.Type.Management, "Dis_NPx", "0");
                IoTPackage package = new IoTPackage(this.iotProtocolVersion,this.selfIoTIP,this.managerIoTIP,cmd.sendedData);
                
                this.requestDevListSocket.Connect(this.managerIPAddress, this.managerPort);
                this.sendDataToManager(this.requestDevListSocket, package.DataVectorForSending);
                //***********************************************

                //****************Start async receive device list***********
                this.startDescriptionReceive();                
            }
            
        }

        private void startDescriptionReceive()
        {
            if (this.requestDevListSocket.Connected)
            {                
                Array.Clear(this.descriptionBuffer, 0, this.descriptionBuffer.Length);
                this.requestDevListSocket.BeginReceive(this.descriptionBuffer, 0, this.descriptionBuffer.Length, 0,
                new AsyncCallback(this.descriptionArrived), null);
            }
        }

        private void descriptionArrived(IAsyncResult ar)
        {            
            int n = this.requestDevListSocket.EndReceive(ar);            
            IoTUtility.GetPackageError result = IoTUtility.GetPackageError.UnknownError;
            IoTPackage package = null;

            if (n > 0)
            {                
                char[] temp = this.byteArrayToCharArray(this.descriptionBuffer, 0, n);
                this.descriptionPackageBuffer.AddRange(temp);                
                package = this.iotUtility.GetCompletedPackage(this.descriptionPackageBuffer,ref result);
            }

            if (result == IoTUtility.GetPackageError.NoError)
            {               
                this.requestDevListSocket.Disconnect(true);                                
                this.endDeviceController.SetDeviceInfo(package);
                if (this.GotManagerResponse != null)
                {
                    NetworkEventArgs args = new NetworkEventArgs();
                    args.isSucceed = true;
                    this.GotManagerResponse(this, args);
                }
            }
            else
            {
                this.startDescriptionReceive();
            }
            
        }
#endregion

#region Listen data from server
        private void initNetWorkReader()
        {
            if (this.mainSocket.Connected)
            {                
                this.cmdReadBuffer = new byte[this.mainSocket.ReceiveBufferSize];
                this.cmdPackageBuffer = new List<char>(this.maxRecvBuffer);
                //this.cmdPackageBuffer = new char[this.maxRecvBuffer];
            }
        }

        private void startReadNetWorkData()
        {
            //this.networkStream.BeginRead(this.readBuffer, 0, this.readBuffer.Length, new AsyncCallback(this.dataArrived), this.networkStream);
            //this.registedNetworkStream.BeginRead(this.cmdReadBuffer, 0, this.cmdReadBuffer.Length, new AsyncCallback(this.dataArrived), null);        
            if (this.mainSocket.Connected)
            {
                Array.Clear(this.cmdReadBuffer, 0, this.cmdReadBuffer.Length);
                this.mainSocket.BeginReceive(this.cmdReadBuffer, 0, this.cmdReadBuffer.Length, 0,
                   new AsyncCallback(this.commandArrived), null);
            }
        }

        private void commandArrived(IAsyncResult ar)
        {
            
            try
            {
                int n = this.mainSocket.EndReceive(ar);
                if (n > 0)
                {
                    //int isCompletedPackage = 0;
                    IoTUtility.GetPackageError result = IoTUtility.GetPackageError.UnknownError;
                    IoTPackage package = null;
                    do
                    {                        
                        char[] temp = this.byteArrayToCharArray(this.cmdReadBuffer, 0, n);
                        this.cmdPackageBuffer.AddRange(temp);
                        package=this.iotUtility.GetCompletedPackage(this.cmdPackageBuffer,ref result);
                        Array.Clear(this.cmdReadBuffer, 0, n);                        

                        if (result == IoTUtility.GetPackageError.NoError)
                        {
                            this.handlePackage(package);
                        }

                    } while (result == IoTUtility.GetPackageError.NoError && this.cmdPackageBuffer.Count > 0);
                }
                this.startReadNetWorkData();
            }
            catch (ObjectDisposedException normalDispose)
            {
                Console.WriteLine("The main socket is disposed");
                Console.WriteLine(normalDispose);
            }
            catch (SocketException inturrupt)
            {
                Console.WriteLine("The main socket is disposed");
                Console.WriteLine(inturrupt);
            }
           
        }

        private void handlePackage(IoTPackage package)
        {            
            IoTCommand cmd = new IoTCommand(new string(package.DataVector.ToArray()));            

            switch (cmd.CmdType)
            {
                case IoTCommand.Type.ReadResponse:
                    if(this.GotResponseCMD!=null)
                    {
                        this.GotResponseCMD(cmd, null);
                    }
                    break;

                case IoTCommand.Type.Management:

                    //Console.WriteLine("Received manager command!");

                    if(this.GotManagerCMD != null)
                    {
                        this.GotManagerCMD(cmd,null);
                    }
                    break;

                default:
                    throw new Exception("Received unknow command type");
                    //WarningController.ShowMessage_st("Received unknow command type");                    

            }         
            
        }        
#endregion


        private IoTPackage waitAcompletedPackage(Socket socket,int waitTime)
        {
            IoTPackage package = null;
            //char[] packageBuffer = new char[JsFileParser.MAXPACKETSIZE];
            List<char> packageBuffer = new List<char>(this.maxRecvBuffer);
            byte[] readBuffer = new byte[socket.ReceiveBufferSize];
            
            IAsyncResult asyncReader = socket.BeginReceive(readBuffer, 0, readBuffer.Length, 0, null, null);
            WaitHandle handle = asyncReader.AsyncWaitHandle;
            while (true)
            {
                IoTUtility.GetPackageError result= IoTUtility.GetPackageError.UnknownError;
                bool dataArrived = handle.WaitOne(waitTime, false);
                if (dataArrived)
                {
                    //int n = networkStream.EndRead(asyncReader);
                    int n = socket.EndReceive(asyncReader);

                    char[] currentReceiveBuf = byteArrayToCharArray(readBuffer, 0, n);
                    packageBuffer.AddRange(currentReceiveBuf);
                    //this.iotUtility.charcat(packageBuffer, currentReceiveBuf, offset, n);
                    package=this.iotUtility.GetCompletedPackage(packageBuffer, ref result);

                    if (result == IoTUtility.GetPackageError.NoError)
                        break;
                }
                else
                {
                    package = null;
                    //WarningController.ShowMessage_st("Manager no response!");
                    break;

                }
            }
            return package;
        }

        private char[] byteArrayToCharArray(byte[] byteArray, int offset, int length)
        {
            char[] charArray = new char[length];
            for (int i = offset; i < length; i++)
            {
                charArray[i] = (char)byteArray[i];
            }

            return charArray;
        }

        private byte[] charArrayToByteArray(char[] charArray, int offset, int length)
        {
            byte[] byteArray = new byte[length];
            for (int i = offset; i < length; i++)
            {
                byteArray[i] = (byte)charArray[i];
            }

            return byteArray;
        }
    }
}


