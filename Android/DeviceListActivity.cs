using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using IoTRepublicPlus;

using MobileShareLibrary.Utility;
using MobileShareLibrary;

namespace OSyncIoT
{
    [Activity(Label = "DeviceList", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DeviceListActivity : Activity
    {
        private MainActivity mainActivity;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DeviceList);
            this.mainActivity = MainActivity.mainActivity;

            this.mainActivity.iotNet.GotManagerCMD += this.iotNet_GotManagerCMD;
            this.initDeviceButtons();                   
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (this.mainActivity.iotNet != null)
            {
                this.mainActivity.iotNet.GotManagerCMD -= this.iotNet_GotManagerCMD;
            }
            //Console.Write("Exit the activity");
        }

        private void initDeviceButtons()
        {
            LinearLayout viewGroup = (LinearLayout)this.FindViewById<LinearLayout>(Resource.Id.linearLayout_DeviceList);
            MainActivity mainActivity = MainActivity.mainActivity;
            List<IoTDevice> devices= mainActivity.iotNet.GetAllDeviceInfo();

            for(int i=0;i<devices.Count;i++)
            {
                Button btn = new Button(Application.Context);
                btn.Text = devices[i].DeviceName;
                btn.Id = i;
                btn.Click += Btn_Click;                
                viewGroup.AddView(btn);
            }                        
        }


        private void iotNet_GotManagerCMD(object sender, EventArgs e)
        {
            IoTCommand cmd = (IoTCommand)sender;
            
            if(cmd.ID=="Rel_Req")
            {
                Console.WriteLine("Command ID:" + cmd.ID);
                RunOnUiThread(() =>
                {
                    //LinearLayout viewGroup = (LinearLayout)this.FindViewById<LinearLayout>(Resource.Id.linearLayout_DeviceList);
                    //viewGroup.RemoveAllViews();
                    this.mainActivity.iotNet.AskAllDeviceInfo();
                    this.Recreate();
                    //this.initDeviceButtons();
                });                                
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Intent deviceContent = new Intent(this, typeof(DeviceContentActivity));
            int deviceIndex = btn.Id;
            deviceContent.PutExtra("deviceIndex", deviceIndex);
            StartActivity(deviceContent);
        }
    }
}