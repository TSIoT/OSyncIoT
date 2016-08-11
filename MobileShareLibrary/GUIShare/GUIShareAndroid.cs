using System;
using System.Collections.Generic;
using System.Text;



#if __ANDROID__
using AndroidHUD;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace MobileShareLibrary.GUIShare
{
    public class GUIShareAndroid:ISharedGUICom
    {
        private Activity mainActivity;

        public GUIShareAndroid(Activity activity)
        {
            this.mainActivity = activity;
        }            

        public void EnableProcessingCover(string processMsg)
        {
            AndHUD.Shared.Show(this.mainActivity, processMsg, -1, MaskType.Black, null);
            
        }

        public void DisableProcessingCover()
        {
            AndHUD.Shared.Dismiss(this.mainActivity);
        }

        public void ShowAlertMessage(string message)
        {
            AlertDialog.Builder builder;
            builder = new AlertDialog.Builder(this.mainActivity);
            builder.SetTitle("Alert");
            builder.SetMessage(message);
            builder.SetCancelable(false);
            builder.SetPositiveButton("OK", delegate {;}); //do nothing
            builder.Show();
        }


    }
}

#endif