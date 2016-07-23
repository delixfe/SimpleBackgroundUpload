using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SimpleBackgroundUpload
{
	public class Uploader
	{
		NSUrlSession session;
		int taskId;
		string webApiAddress;


		public Uploader(string url)
		{
			webApiAddress = url;
		}

		public async Task Upload() {
			
		}

		/// <summary>
		/// Prepares the upload.
		/// </summary>
		/// <returns>The upload.</returns>
		public async Task PrepareUpload()
		{
			try
			{
				Console.WriteLine("PrepareUpload called...");

				if (session == null)
					session = InitBackgroundSession();

				// Check if task already exits
				//var tsk = await GetPendingTask();
				//if (tsk != null)
				//{
				//	Console.WriteLine("TaskId {0} found, state: {1}", tsk.TaskIdentifier, tsk.State);

				//	// If our task is suspended, resume it.
				//	if (tsk.State == NSUrlSessionTaskState.Suspended)
				//	{
				//		Console.WriteLine("Resuming taskId {0}...", tsk.TaskIdentifier);
				//		tsk.Resume();
				//	}

				//	return; // exit, we already have a task
				//}

				// For demo purposes file is attached to project as "Content" and PDF is 8.1MB.
				var fileToUpload = "UIKitUICatalog.pdf";

				var bodyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BodyData.tmp");

				if (!File.Exists(fileToUpload))
				{
					Console.WriteLine("Upload file doesn't exist. File: {0}", fileToUpload);
					return;
				}
					
				// Create request
				NSUrl uploadHandleUrl = NSUrl.FromString(webApiAddress + "File/PutFile");
				NSMutableUrlRequest request = new NSMutableUrlRequest(uploadHandleUrl);
				request.HttpMethod = "PUT";
				//request["Content-Type"] = "multipart/form-data; boundary=" + boundary;
				request["FileName"] = Path.GetFileName(fileToUpload);
				//WriteMultiPartBodyFile(fileToUpload, bodyPath, boundary);

				// Creating upload task
				var uploadTask = session.CreateUploadTask(request, NSUrl.FromFilename(fileToUpload));
				Console.WriteLine("New TaskID: {0}", uploadTask.TaskIdentifier);

				// Start task
				uploadTask.Resume();


			}
			catch (Exception ex)
			{
				Console.WriteLine("PrepareUpload Ex: {0}", ex.Message);
			}
		}


		/// <summary>
		/// Gets the pending task.
		/// </summary>
		/// <returns>The pending task.</returns>
		/// <remarks>For demo purposes we are only starting a single task so that's why we are returning only one.</remarks>
		private async Task<NSUrlSessionUploadTask> GetPendingTask()
		{
			NSUrlSessionUploadTask uploadTask = null;

			if (session != null)
			{
				var tasks = await session.GetTasks2Async();

				var taskList = tasks.UploadTasks;
				if (taskList.Count() > 0)
					uploadTask = (NSUrlSessionUploadTask)taskList[0];
			}

			return uploadTask;
		}

		/// <summary>
		/// Processes the completed task.
		/// </summary>
		/// <param name="sessionTask">Session task.</param>
		public void ProcessCompletedTask(NSUrlSessionTask sessionTask)
		{
			try
			{
				var message = string.Format("Task ID: {0}, State: {1}, Response: {2}", sessionTask.TaskIdentifier, sessionTask.State, sessionTask.Response);
				Console.WriteLine(message);



				UIApplication.SharedApplication.InvokeOnMainThread(() =>
				{
					UILocalNotification notification = new UILocalNotification();
					notification.AlertAction = $"Task {sessionTask.TaskIdentifier} finished";
					notification.AlertBody = message;
					notification.SoundName = UILocalNotification.DefaultSoundName;
					UIApplication.SharedApplication.PresentLocationNotificationNow(notification);
				});

				// Make sure that we have a response to process
				if (sessionTask.Response == null || sessionTask.Response.ToString() == "")
				{
					Console.WriteLine("ProcessCompletedTask no response...");
				}
				else
				{
					// Get response
					var resp = (NSHttpUrlResponse)sessionTask.Response;

					// Check that our task completed and server returned StatusCode 201 = CREATED.
					if (sessionTask.State == NSUrlSessionTaskState.Completed && resp.StatusCode == 201)
					{
						// Do something with the uploaded file...
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("ProcessCompletedTask Ex: {0}", ex.Message);
			}
		}

		/// <summary>
		/// Initializes the background session.
		/// </summary>
		/// <returns>The background session.</returns>
		public NSUrlSession InitBackgroundSession()
		{
			// See URL below for configuration options
			// https://developer.apple.com/library/ios/documentation/Foundation/Reference/NSURLSessionConfiguration_class/index.html

			// Use same identifier for background tasks so in case app terminiated, iOS can resume tasks when app relaunches.
			string identifier = "MyBackgroundTaskId";

			using (var config = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(identifier))
			{
				config.HttpMaximumConnectionsPerHost = 4; //iOS Default is 4
				config.TimeoutIntervalForRequest = 600.0; //30min allowance; iOS default is 60 seconds.
				config.TimeoutIntervalForResource = 120.0; //2min; iOS Default is 7 days
				return NSUrlSession.FromConfiguration(config, new UploadDelegate(), new NSOperationQueue());
			}
		}
	}

	public class UploadDelegate : NSUrlSessionTaskDelegate
	{
		// Called by iOS when the task finished trasferring data. It's important to note that his is called even when there isn't an error.
		// See: https://developer.apple.com/library/ios/documentation/Foundation/Reference/NSURLSessionTaskDelegate_protocol/index.html#//apple_ref/occ/intfm/NSURLSessionTaskDelegate/URLSession:task:didCompleteWithError:
		public override void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
		{
			Console.WriteLine(string.Format("DidCompleteWithError TaskId: {0}{1}", task.TaskIdentifier, (error == null ? "" : " Error: " + error.Description)));

			if (error == null)
			{
				var appDel = UIApplication.SharedApplication.Delegate as AppDelegate;
				appDel.Uploader.ProcessCompletedTask(task);
			}
		}

		// Called by iOS when session has been invalidated.
		// See: https://developer.apple.com/library/ios/documentation/Foundation/Reference/NSURLSessionDelegate_protocol/index.html#//apple_ref/occ/intfm/NSURLSessionDelegate/URLSession:didBecomeInvalidWithError:
		public override void DidBecomeInvalid(NSUrlSession session, NSError error)
		{
			Console.WriteLine("DidBecomeInvalid" + (error == null ? "undefined" : error.Description));
		}

		// Called by iOS when all messages enqueued for a session have been delivered.
		// See: https://developer.apple.com/library/ios/documentation/Foundation/Reference/NSURLSessionDelegate_protocol/index.html#//apple_ref/occ/intfm/NSURLSessionDelegate/URLSessionDidFinishEventsForBackgroundURLSession:
		public override void DidFinishEventsForBackgroundSession(NSUrlSession session)
		{
			Console.WriteLine("DidFinishEventsForBackgroundSession");
		}

		// Called by iOS to periodically inform the progress of sending body content to the server.
		// See: https://developer.apple.com/library/ios/documentation/Foundation/Reference/NSURLSessionTaskDelegate_protocol/index.html#//apple_ref/occ/intfm/NSURLSessionTaskDelegate/URLSession:task:didSendBodyData:totalBytesSent:totalBytesExpectedToSend:
		public override void DidSendBodyData(NSUrlSession session, NSUrlSessionTask task, long bytesSent, long totalBytesSent, long totalBytesExpectedToSend)
		{
			
			// Uncomment line below to see file upload progress outputed to the console. You can track/manage this in your app to monitor the upload progress.
			Console.WriteLine ("DidSendBodyData bSent: {0}, totalBSent: {1} totalExpectedToSend: {2}", bytesSent, totalBytesSent, totalBytesExpectedToSend);
		}
	}

}

