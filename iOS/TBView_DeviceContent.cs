using Foundation;
using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using UIKit;

using MobileShareLibrary;
using MobileShareLibrary.Utility;
//using IOTProtocol.Utility;
using IoTRepublicPlus;

namespace OSyncIoT
{
	partial class TBView_DeviceContent : UITableViewController
	{
        public static NSString TbviewId = new NSString("TBView_DeviceContent");

        public IoTDevice Device { get; set; }
        public IoTNet IotNet
        {
            private get
            {
                return this.iotNet;
            }

            set
            {
                this.iotNet = value;
                this.iotNet.GotResponseCMD += this.iotNet_GotResponseCMD;
            }
        }

        private IoTNet iotNet;

        public TBView_DeviceContent (IntPtr handle) : base (handle)
		{
            base.TableView.RegisterClassForCellReuse(typeof(UITableViewCell),TbviewId);
            base.TableView.Source = new IoTDeviceContentViewSource(this);   
		}

        //when move to parent, remove event subscribe
        public override void ViewWillDisappear(Boolean animated)
        {
            if(this.iotNet!=null)
            {
                //Console.WriteLine("TBView_DeviceContent Disappear");
                this.iotNet.GotResponseCMD -= this.iotNet_GotResponseCMD;
            }            
        }


        private void iotNet_GotResponseCMD(object sender, EventArgs e)
        {            
            IoTCommand cmd =(IoTCommand)sender;
            Console.WriteLine("Receive Command ["+cmd.ID+"]");
            InvokeOnMainThread(() =>
            {
                base.TableView.Source = new IoTDeviceContentViewSource(this, cmd);
                base.TableView.ReloadData();
            });
                
        }


        #region  UITableViewSource
        class IoTDeviceContentViewSource : UITableViewSource
        {
            private TBView_DeviceContent controller;
            private IoTCommand recvCmd;
            private int baseCount=4;

            public IoTDeviceContentViewSource(TBView_DeviceContent controller)
            {
                this.controller = controller;
                this.recvCmd = null;
            }

            public IoTDeviceContentViewSource(TBView_DeviceContent controller, IoTCommand cmd)
            {
                this.controller = controller;
                this.recvCmd = cmd;
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                int totleRowCount = baseCount; //IoTIP,DeviceID,DeviceName,FunctionGroup
                totleRowCount += this.controller.Device.Component.Count; //component count

                return totleRowCount;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                
                int comIndex = -1;                
                int row = indexPath.Row;
                UITableViewCell cell;
                if(row<4)
                {
                    //cell = tableView.DequeueReusableCell(TBView_DeviceContent.TbviewId);
                    cell = new UITableViewCell(UITableViewCellStyle.Subtitle, TBView_DeviceContent.TbviewId);
                }
                else
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Default, TBView_DeviceContent.TbviewId);
                }

                switch (row)
                {
                    case 0:
                        cell.TextLabel.Text =this.controller.Device.IoTIP;
                        cell.DetailTextLabel.Text="IoT IP";
                        cell.Accessory = UITableViewCellAccessory.None;
                        break;

                    case 1:                        
                        cell.TextLabel.Text = this.controller.Device.DeviceID;
                        cell.DetailTextLabel.Text = "Device ID";
                        cell.Accessory = UITableViewCellAccessory.None;
                        break;

                    case 2:                        
                        cell.TextLabel.Text = this.controller.Device.DeviceName;
                        cell.DetailTextLabel.Text = "Device Name";

                        cell.Accessory = UITableViewCellAccessory.None;
                        break;

                    case 3:                        
                        cell.TextLabel.Text = this.controller.Device.FunctionGroup;
                        cell.DetailTextLabel.Text = "Function";
                        cell.Accessory = UITableViewCellAccessory.None;
                        break;

                    default:
                        comIndex = row - this.baseCount;
                        IoTComponent com=this.controller.Device.Component[comIndex];

                        if(this.recvCmd != null && recvCmd.ID == com.ID)
                        {
                            cell.TextLabel.Text =  com.Name + "["+this.recvCmd.Value+"]";
                        }
                        else
                        {
                            cell.TextLabel.Text = com.Name;
                        }
                        
                        cell.Accessory = UITableViewCellAccessory.None;
                        break;
                }

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {                
                int row = indexPath.Row;
                if (row >= this.baseCount)
                {
                    int comIndex = row - this.baseCount;
                    IoTComponent com = this.controller.Device.Component[comIndex];
                    //IoTCommand cmd = new IoTCommand();
                    IoTCommand.Type cmdType = IoTCommand.Type.None;

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

                    IoTCommand cmd = new IoTCommand(cmdType, com.ID, "0");

                    this.controller.iotNet.SendCMD(cmd,this.controller.Device.IoTIP);
                }                
            }

        }
        #endregion
    }
}
