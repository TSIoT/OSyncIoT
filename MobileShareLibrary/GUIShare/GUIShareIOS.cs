using System;
using System.Collections.Generic;
using System.Text;



#if __IOS__
using UIKit;
using CoreGraphics;


namespace MobileShareLibrary.GUIShare
{    
    public class GUIShareIOS : ISharedGUICom
    {
        private LoadingOverlay loadingOverlay;
        private UIViewController rootView;
        private CGRect bounds;
        public GUIShareIOS(UIViewController view)
        {
            this.rootView = view;
            this.bounds = UIScreen.MainScreen.Bounds;            
        }

        public void EnableProcessingCover(string processMsg)
        {
            this.loadingOverlay = new LoadingOverlay(bounds, processMsg);
            this.rootView.View.Add(loadingOverlay);
        }

        public void DisableProcessingCover()
        {
            this.loadingOverlay.Hide();
            this.loadingOverlay.Dispose();
            this.loadingOverlay = null;
        }

        public void ShowAlertMessage(string message)
        {
            var alert = UIAlertController.Create("Alert", message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
            this.rootView.PresentViewController(alert, true, null);            
        }
    }

    public class LoadingOverlay : UIView
    {
        // control declarations
        UIActivityIndicatorView activitySpinner;
        UILabel loadingLabel;

        public LoadingOverlay(CGRect frame, string processMsg) : base(frame)
        {
            // configurable bits
            BackgroundColor = UIColor.Black;
            Alpha = 0.75f;
            AutoresizingMask = UIViewAutoresizing.All;

            nfloat labelHeight = 22;
            nfloat labelWidth = Frame.Width - 20;

            // derive the center x and y
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            // create the activity spinner, center it horizontall and put it 5 points above center x
            activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            activitySpinner.Frame = new CGRect(
                centerX - (activitySpinner.Frame.Width / 2),
                centerY - activitySpinner.Frame.Height - 20,
                activitySpinner.Frame.Width,
                activitySpinner.Frame.Height);
            activitySpinner.AutoresizingMask = UIViewAutoresizing.All;
            AddSubview(activitySpinner);
            activitySpinner.StartAnimating();

            // create and configure the "Loading Data" label
            loadingLabel = new UILabel(new CGRect(
                centerX - (labelWidth / 2),
                centerY + 20,
                labelWidth,
                labelHeight
                ));
            loadingLabel.BackgroundColor = UIColor.Clear;
            loadingLabel.TextColor = UIColor.White;
            //loadingLabel.Text = "Scanning...";
            loadingLabel.Text = processMsg;
            loadingLabel.TextAlignment = UITextAlignment.Center;
            loadingLabel.AutoresizingMask = UIViewAutoresizing.All;
            AddSubview(loadingLabel);
        }

        /// <summary>
        /// Fades out the control and then removes it from the super view
        /// </summary>
        public void Hide()
        {
            UIView.Animate(
                0.5, // duration
                () => { Alpha = 0; },
                () => { RemoveFromSuperview(); }
            );
        }
    }


}

#endif