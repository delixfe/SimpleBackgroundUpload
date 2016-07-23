
using System;
using CoreGraphics;

using Foundation;
using UIKit;
using System.IO;
using AI.XamarinSDK.Abstractions;

namespace SimpleBackgroundUpload
{
    public partial class UploadController : UIViewController
    {
        public UploadController () : base ("UploadController", null)
        {
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();



            button1.SetTitle("Start file upload", UIControlState.Normal);
            button1.TouchUpInside += async delegate 
            {
                TelemetryManager.TrackEvent("Start file upload.");

                var appDel = UIApplication.SharedApplication.Delegate as AppDelegate;
                await appDel.Uploader.PrepareUpload();
            };
        }
    }
}

