using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

using CoreGraphics; //CGRect
using MobileShareLibrary.GUIShare; //gui shared functions
using MobileShareLibrary.Utility;  //utility



namespace OSyncIoT
{
	partial class MainView : UIViewController
	{
        private ISharedGUICom guiCom;
        private IoTNet iotNet;        
        
        private int connectWaitTime = 3500;

        public MainView (IntPtr handle) : base (handle)
		{
        }
        #region life cycle                
        public override void AwakeFromNib()
        {
            // Called when loaded from xib or storyboard.
        }

        public override void ViewDidLoad()
        {
            this.guiCom = new GUIShareIOS(this);

            this.iotNet = new IoTNet();
            this.iotNet.GotUDPResponse += netController_GotUDPResponse;
            this.iotNet.GotManagerResponse += netController_GotManagerResponse;

            //this.fileController = new JsFileController();

            this.Btn_Scan.TouchUpInside += this.btnScan_TouchUpInside;
            this.Btn_Connect.TouchUpInside += this.btnConnect_TouchUpInside;
            this.Btn_Disconnect.TouchUpInside +=this.btnDisconnect_TouchUpInside;
            

            //TBView_DeviceList DeviceList = this.Storyboard.InstantiateInitialViewController("TBView_DeviceList") as TBView_DeviceList;            

        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);
            Console.WriteLine("Segue go");
        }

        #endregion

        #region Events handler
        private void netController_GotUDPResponse(object sender, EventArgs e)
        {
            NetworkEventArgs args = (NetworkEventArgs)e;
            InvokeOnMainThread(() =>
            {
                this.guiCom.DisableProcessingCover();
                if (args.isSucceed)
                {
                    Txt_ManagerIP.Text = (string)args.Data;                    
                }
                else
                {
                    //Txt_ManagerIP.Text = "Not found";
                    Txt_ManagerIP.Text = "";
                    this.guiCom.ShowAlertMessage("Cannot found manager");
                }                
            });
        }

        private void netController_GotManagerResponse(object sender, EventArgs e)
        {
            NetworkEventArgs args = (NetworkEventArgs)e;           

            InvokeOnMainThread(() =>
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
        #endregion

        #region GUI Component event
        /**
         * Handle when tap the [Scan] button
         **/
        private void btnScan_TouchUpInside(object sender, EventArgs e)
        {                   
            InvokeOnMainThread(() =>
            {
                guiCom.EnableProcessingCover("Searching Manager...");                              
            });
            this.iotNet.SearchManagerIP(connectWaitTime);
        }

        /**
         * Handle when tap the [Connect] button
         **/
        private void btnConnect_TouchUpInside(object sender, EventArgs e)
        {
            InvokeOnMainThread(() =>
            {
                guiCom.EnableProcessingCover("Connect to Manager...");                
            });
            string managerIp = this.Txt_ManagerIP.Text;
            this.iotNet.ConnectToManager(managerIp, connectWaitTime);
            

        }

        /**
         * Handle when tap the [Disonnect] button
         **/
        private void btnDisconnect_TouchUpInside(object sender, EventArgs e)
        {
            this.iotNet.DisconnectFromManager();
            InvokeOnMainThread(() =>
            {
                this.Txt_ManagerIP.Text = "";
            });
        }



        #endregion

        private void moveToNextPage()
        {
            TBViewController_DeviceList view_DeviceList = AppDelegate.MainStoryboard.InstantiateViewController("TBView_DeviceList") as TBViewController_DeviceList;

            if (view_DeviceList != null)
            {
                view_DeviceList.DeviceList = this.iotNet.GetAllDeviceInfo();
                view_DeviceList.iotNet = this.iotNet;
                view_DeviceList.GUIUtility = this.guiCom;

                //this.NavigationController.PushViewController(view_DeviceList, true);
                AppDelegate.RootNavigationController.PushViewController(view_DeviceList, true);
            }
        }
    }



}
