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

//using IOTProtocol.Utility;
using IoTRepublicPlus;

using MobileShareLibrary;
using MobileShareLibrary.Utility;

namespace OSyncIoT
{
    [Activity(Label = "DeviceContentActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class DeviceContentActivity : Activity
    {
        private IoTDevice device;
        //private IoTNet iotNet;
        private MainActivity mainActivity;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DeviceContent);

            //this.iotNet = MainActivity.mainActivity.iotNet;
            this.mainActivity = MainActivity.mainActivity;

            this.mainActivity.iotNet.GotResponseCMD += this.iotNet_GotResponseCMD;
            this.initDevice();
            // Create your application here
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (this.mainActivity.iotNet != null)
            {
                this.mainActivity.iotNet.GotResponseCMD -= this.iotNet_GotResponseCMD;
            }
            //Console.Write("Exit the activity");
        }


        private void iotNet_GotResponseCMD(object sender, EventArgs e)
        {
            IoTCommand cmd = (IoTCommand)sender;
            Console.WriteLine("Receive Command [" + cmd.ID + "]");
            LinearLayout viewGroup = (LinearLayout)this.FindViewById<LinearLayout>(Resource.Id.linearLayout_DeviceContent);
            int comIndex = this.device.Component.FindIndex(item=>item.ID==cmd.ID);

            RunOnUiThread(()=>
            {
                Button targetButton = viewGroup.FindViewById<Button>(comIndex);
                targetButton.Text = this.device.Component[comIndex].Name + "[" + cmd.Value + "]";
            });
            

        }

        private void initDevice()
        {
            //MainActivity mainActivity = MainActivity.mainActivity;
            Intent intent = this.Intent;
            int deviceIndex= intent.GetIntExtra("deviceIndex", 0);
            this.device = this.mainActivity.iotNet.GetAllDeviceInfo()[deviceIndex];

            LinearLayout viewGroup = (LinearLayout)this.FindViewById<LinearLayout>(Resource.Id.linearLayout_DeviceContent);

            TextView iotIp = new TextView(Application.Context);
            iotIp.TextSize = 20;
            TextView deviceName = new TextView(Application.Context);
            deviceName.TextSize = 20;
            TextView deviceId = new TextView(Application.Context);
            deviceId.TextSize = 20;
            TextView functionGroup = new TextView(Application.Context);
            functionGroup.TextSize = 20;

            iotIp.Text = "IoTIP:" + this.device.IoTIP;
            deviceName.Text = "DeviceName:" + this.device.DeviceName;
            deviceId.Text = "DeviceID:" + this.device.DeviceID;
            functionGroup.Text = "FunctionGroup:" + this.device.FunctionGroup;

            viewGroup.AddView(iotIp);
            viewGroup.AddView(deviceName);
            viewGroup.AddView(deviceId);
            viewGroup.AddView(functionGroup);

            for(int i=0;i<this.device.Component.Count;i++)
            {
                Button btn = new Button(Application.Context);
                btn.Id = i;
                btn.Text = this.device.Component[i].Name;
                btn.Click += this.com_sendCmd;
                viewGroup.AddView(btn);
            }
        }

        private void com_sendCmd(object sender, EventArgs e)
        {
            
            Button btn = sender as Button;
            int comIndex = btn.Id;
            IoTComponent com = this.device.Component[comIndex];
            //IoTCommand cmd = new IoTCommand();
            IoTCommand.Type cmdType= IoTCommand.Type.None;

            switch (com.Type)
            {
                case IoTComponent.ComType.Button:
                    cmdType = IoTCommand.Type.Write;
                    break;

                case IoTComponent.ComType.Content:
                    cmdType = IoTCommand.Type.ReadRequest;
                    break;

                default:
                    cmdType = IoTCommand.Type.None;
                    throw new Exception("Unknown component type");
                    
            }

            //cmd.ID = com.ID;
            //cmd.Value = "0";
            IoTCommand cmd = new IoTCommand(cmdType, com.ID, "0");


            this.mainActivity.iotNet.SendCMD(cmd, this.device.IoTIP);
            
        }
    }
}