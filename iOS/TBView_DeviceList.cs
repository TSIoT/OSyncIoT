using Foundation;
using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using UIKit;

using MobileShareLibrary;
using MobileShareLibrary.GUIShare;
using MobileShareLibrary.Utility;
using CoreGraphics;

namespace OSyncIoT
{
	partial class TBViewController_DeviceList : UITableViewController
	{
        public static NSString TbviewId = new NSString("TBView_DeviceList");

        public List<IoTDevice> DeviceList { get; set; }
        public IoTNet iotNet { private get; set;}
        public ISharedGUICom GUIUtility { get; set; }


        public TBViewController_DeviceList (IntPtr handle) : base (handle)
		{
            base.TableView.RegisterClassForCellReuse(typeof(UITableViewCell),TbviewId);
            base.TableView.Source = new IoTDeviceListViewSource(this);            
        }
       

        #region  UITableViewSource
        class IoTDeviceListViewSource : UITableViewSource
        {
            TBViewController_DeviceList controller;            

            public IoTDeviceListViewSource(TBViewController_DeviceList controller)
            {                
                this.controller = controller;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                return "Device list";
            }

            public override string TitleForFooter(UITableView tableView, nint section)
            {
                return "Device Count:" + this.controller.DeviceList.Count.ToString();
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return controller.DeviceList.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {                
                UITableViewCell cell = new UITableViewCell(UITableViewCellStyle.Subtitle, TBViewController_DeviceList.TbviewId);

                int row = indexPath.Row;                
                cell.TextLabel.Text = controller.DeviceList[row].DeviceName;
                cell.DetailTextLabel.Text= controller.DeviceList[row].IoTIP;
                cell.Accessory = UITableViewCellAccessory.Checkmark;
                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {                
                TBView_DeviceContent view_DevieContent = AppDelegate.MainStoryboard.InstantiateViewController("TBView_DeviceContent") as TBView_DeviceContent;
                view_DevieContent.Device = this.controller.DeviceList[indexPath.Row];                
                view_DevieContent.IotNet = this.controller.iotNet;
                AppDelegate.RootNavigationController.PushViewController(view_DevieContent, true);
            }

        }
        #endregion
    }
}
