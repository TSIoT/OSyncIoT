using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using MobileShareLibrary;
using MobileShareLibrary.Utility;
using MobileShareLibrary.GUIShare;

namespace OSyncIoT
{
    [Activity(Label = "OSyncIoT", MainLauncher = true, Icon = "@drawable/icon", 
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        public static MainActivity mainActivity;

        public ISharedGUICom guiCom;
        public IoTNet iotNet;

        private int connectWaitTime = 3500;

        private Button Btn_Scan;
        private Button Btn_Login;
        private Button Btn_Logout;
        private EditText Txt_ManagerIP;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            mainActivity = this;            
            SetContentView(Resource.Layout.Main);
            
            this.guiCom = new GUIShareAndroid(this);
            
            this.iotNet = new IoTNet();
            
            this.iotNet.GotUDPResponse += this.netController_GotUDPResponse;
            this.iotNet.GotManagerResponse += this.netController_GotManagerResponse;

            this.Btn_Scan= FindViewById<Button>(Resource.Id.Btn_Scan);            
            this.Btn_Login = FindViewById<Button>(Resource.Id.Btn_Login);
            this.Btn_Logout = FindViewById<Button>(Resource.Id.Btn_Logut);
            this.Txt_ManagerIP= FindViewById<EditText>(Resource.Id.Txt_ManagerIP);

            this.Btn_Logout.Click += Btn_Logout_Click;
            this.Btn_Login.Click += Btn_Login_Click;
            this.Btn_Scan.Click += Btn_Scan_Click;
            
            
        }        

        #region Events handler
        private void netController_GotManagerResponse(object sender, EventArgs e)
        {
            NetworkEventArgs args = (NetworkEventArgs)e;
            
            RunOnUiThread(() =>
            {
                this.guiCom.DisableProcessingCover();
                if (args.isSucceed)
                {
                    //Txt_ManagerIP.Text = "Got IoT IP";
                    if (this.iotNet.isConnected())
                    {
                        moveToNextPage();                        
                    }
                }
                else
                {
                    //Txt_ManagerIP.Text = "Manager no response";
                    Txt_ManagerIP.Text = "";
                    this.guiCom.ShowAlertMessage("Manager no response");
                }

            });

        }

        private void netController_GotUDPResponse(object sender, EventArgs e)
        {
            NetworkEventArgs args = (NetworkEventArgs)e;
            this.guiCom.DisableProcessingCover();
            if (args.isSucceed)
            {
                RunOnUiThread(()=>
                {
                    this.Txt_ManagerIP.Text = (string)args.Data;
                });                
            }
            else
            {
                //Txt_ManagerIP.Text = "Not found";
                RunOnUiThread(() =>
                {                    
                    this.Txt_ManagerIP.Text = "";
                    this.guiCom.ShowAlertMessage("Cannot found manager");
                });
            }            
        }
        #endregion

        #region GUI Component event
        private void Btn_Logout_Click(object sender, EventArgs e)
        {
            this.iotNet.DisconnectFromManager();
        }

        private void Btn_Login_Click(object sender, EventArgs e)
        {
            string managerIp = "";
            RunOnUiThread(() =>
            {
                this.guiCom.EnableProcessingCover("Connecting to manager...");
                managerIp = this.Txt_ManagerIP.Text;
            });
            this.iotNet.ConnectToManager(managerIp, connectWaitTime);            
        }

        private void Btn_Scan_Click(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                this.guiCom.EnableProcessingCover("Searching manager...");
            });
            this.iotNet.SearchManagerIP(connectWaitTime);
        }
        #endregion


        private void moveToNextPage()
        {
            Intent deviceList = new Intent(this, typeof(DeviceListActivity));
            StartActivity(deviceList);
        }
        
    }
}

