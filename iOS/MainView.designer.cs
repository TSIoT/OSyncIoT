// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace OSyncIoT
{
    [Register ("MainView")]
    partial class MainView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton Btn_Connect { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton Btn_Disconnect { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton Btn_Scan { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField Txt_ManagerIP { get; set; }


        void ReleaseDesignerOutlets ()
        {
            if (Btn_Connect != null) {
                Btn_Connect.Dispose ();
                Btn_Connect = null;
            }

            if (Btn_Disconnect != null) {
                Btn_Disconnect.Dispose ();
                Btn_Disconnect = null;
            }

            if (Btn_Scan != null) {
                Btn_Scan.Dispose ();
                Btn_Scan = null;
            }

            if (Txt_ManagerIP != null) {
                Txt_ManagerIP.Dispose ();
                Txt_ManagerIP = null;
            }
        }
    }
}