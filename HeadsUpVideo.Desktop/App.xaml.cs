using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace HeadsUpVideo.Desktop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// 
        Frame rootFrame;
        private StorageFile _errorFile;
        public StorageFile ErrorFile { get { return _errorFile; } set { _errorFile = value; } }
        
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            CreateErrorFile();
            this.UnhandledException += App_UnhandledException;
        }

        private async void CreateErrorFile()
        {
            try
            { // Open Error File 
                StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                ErrorFile = await local.CreateFileAsync("ErrorLog.txt", CreationCollisionOption.OpenIfExists);
            } catch (Exception) {
                // This *should* never fail.
            }
        }

        public async Task WriteMessage(string strMessage)
        {
            if (ErrorFile != null)
            {
                try
                {
                    await Windows.Storage.FileIO.AppendTextAsync(ErrorFile, string.Format("{0} - {1}\r\n", DateTime.Now.ToLocalTime().ToString(), strMessage));
#if DEBUG
                    Debug.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToLocalTime().ToString(), strMessage));
#endif
                }
                catch (Exception) {
                }
            }
        }

        private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.Exception.GetType() == typeof(System.ArgumentException)) {
                await WriteMessage(string.Format("UnhandledException - Continue - {0}", e.Exception.ToString()));
                e.Handled = true; // Keep Running the app 
            }
            else
            {
                Task t = WriteMessage(string.Format("UnhandledException - Exit - {0}", e.Exception.ToString()));
                t.Wait(3000);
                SaveAppdata(); e.Handled = false;
            }
        }

        private void SaveAppdata()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Task<StorageFile> tFile = folder.CreateFileAsync("AppData.txt").AsTask<StorageFile>();
            tFile.Wait();
            StorageFile file = tFile.Result;
            Task t = Windows.Storage.FileIO.WriteTextAsync(file, "This Is Application data").AsTask();
            t.Wait();
        }


        // In this routine we will decide if we can keep the application running or if we need to save state and exit // Either way we log the exception to our error file. if (e.Exception.GetType() == typeof(System.ArgumentException)) {  e.Handled = true; // Keep Running the app } else { SaveAppdata(); e.Handled = false; } }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();

        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            // Do nothing
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
