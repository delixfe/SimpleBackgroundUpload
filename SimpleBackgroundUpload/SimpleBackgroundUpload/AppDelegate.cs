using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SimpleBackgroundUpload
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		Uploader uploader;

		public Uploader Uploader { get { return uploader; }}
		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			// If you have defined a root view controller, set it here:
			// window.RootViewController = myViewController;

			// Enter your webAPI address below
			//webApiAddress = "http://YourWebAPIAddress/SimpleBackgroundUploadWebAPI/File/PostFile";
			var webApiAddress = "http://192.168.71.1:8080/";

			uploader = new Uploader(webApiAddress);

			window.RootViewController = new UploadController ();

			var settings = UIUserNotificationSettings.GetSettingsForTypes(
				  UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null);
			UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);

			// make the window visible
			window.MakeKeyAndVisible ();
			
			return true;
		}

		//private int backgroundTaskId;

		/// <Docs>Reference to the UIApplication that invoked this delegate method.</Docs>
		/// <remarks>Application are allocated approximately 5 seconds to complete this method. Application developers should use this
		/// time to save user data and tasks, and remove sensitive information from the screen.</remarks>
		/// <altmember cref="M:MonoTouch.UIKit.UIApplicationDelegate.WillEnterForeground"></altmember>
		/// <summary>
		/// Dids the enter background.
		/// </summary>
		/// <param name="application">Application.</param>
		public override void DidEnterBackground(UIApplication application){
			Console.WriteLine("DidEnterBackground called...");

			//NSAction timedOutAction = () => {
			//	if (backgroundTaskId != 0)
			//	{
			//		Console.WriteLine($"BackgroundTask {backgroundTaskId} timed out.");
			//	}
			//};

			//NSAction finishedAction = () => {
			//	Console.WriteLine("Ended background call.");
			//	application.InvokeOnMainThread(() => application.EndBackgroundTask(backgroundTaskId));
			//};

			//backgroundTaskId = application.BeginBackgroundTask(timedOutAction);

			//var task = uploader.PrepareUpload();
			//task.ContinueWith((t) => finishedAction());
		}
	}


}

