using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace xTrade
{
    public partial class App
    {
 
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        private PhoneApplicationFrame RootFrame { get; set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters
                Current.Host.Settings.EnableFrameRateCounter = true;

                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void ApplicationLaunching(object sender, LaunchingEventArgs e)
        {

        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void ApplicationActivated(object sender, ActivatedEventArgs e)
        {

            if (e.IsApplicationInstancePreserved)
            {
                return;
            }

            // Check to see if the key for the application state data is in the State dictionary.
            if (PhoneApplicationService.Current.State.ContainsKey("ApplicationDataObject"))
            {
                // If it exists, assign the data to the application member variable.
                var data = PhoneApplicationService.Current.State["ApplicationDataObject"] as string;
                DeSerializeSettings(data);
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void ApplicationDeactivated(object sender, DeactivatedEventArgs e)
        {
            // Serialize the settings into a string that can be easily stored
            string settings = SerializeSettings();

            // Store it in the State dictionary.
            PhoneApplicationService.Current.State["ApplicationDataObject"] = settings;

            // Also store it in Isolated Storage, in case the application is never reactivated.
            SaveDataToIsolatedStorage("xTradeDataFile.txt", settings);
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void ApplicationClosing(object sender, ClosingEventArgs e)
        {
            string settings = SerializeSettings();

            // Also store it in Isolated Storage, in case the application is never reactivated.
            SaveDataToIsolatedStorage("xTradeDataFile.txt", settings);
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool _phoneApplicationInitialized;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (_phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            _phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        #region Settings Management

        // Declare an event for when the application data changes.
        public event EventHandler ApplicationDataObjectChanged;

        // Declare a private variable to store the host (server) name
        private string _hostName;

        // Declare a public property to access the application data variable.
        public string HostName
        {
            get { return _hostName; }
            set
            {
                if (value != _hostName)
                {
                    _hostName = value;
                    OnApplicationDataObjectChanged(EventArgs.Empty);
                }
            }
        }

        // Declare a private variable to store the port number used by the application
        // NOTE: The port number in this application and the port number in the server must match.
        // Remember to open the port you choose on the computer running the server.
        private int _portNumber;

        // Declare a public property to access the application data variable.
        public int PortNumber
        {
            get { return _portNumber; }
            set
            {
                if (value != _portNumber)
                {
                    _portNumber = value;
                    OnApplicationDataObjectChanged(EventArgs.Empty);
                }
            }
        }

        // Declare a private variable to store the port number used by the application
        // NOTE: The port number in this application and the port number in the server must match.
        // Remember to open the port you choose on the computer running the server.
        private string _userName;

        // Declare a public property to access the application data variable.
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    OnApplicationDataObjectChanged(EventArgs.Empty);
                }
            }
        }

        // Declare a private variable to store the port number used by the application
        // NOTE: The port number in this application and the port number in the server must match.
        // Remember to open the port you choose on the computer running the server.
        private string _pass;

        // Declare a public property to access the application data variable.
        public string Pass
        {
            get { return _pass; }
            set
            {
                if (value != _pass)
                {
                    _pass = value;
                    OnApplicationDataObjectChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Produce a string that contains all the settings used by the application
        /// </summary>
        /// <returns>The string containing the serialized data</returns>
        private string SerializeSettings()
        {
            // This string will be of the form
            // PlayAsX|Hostname|PortNumber

            string result = string.Empty;

            result += _hostName;
            result += "|";
            result += _portNumber.ToString(CultureInfo.InvariantCulture);
            result += "|";
            result += _userName;
            result += "|";
            result += _pass;
            return result;
        }

        /// <summary>
        /// Given a string of serialized settings, re-hydrate each settings variable
        /// </summary>
        /// <param name="data">The string of serialized settings</param>
        private void DeSerializeSettings(string data)
        {
            // Split the string using the '|' delimiter
            string[] values = data.Split("|".ToCharArray(), StringSplitOptions.None);

            // The string is of the form
            // PlayAsX|Hostname|PortNumber

            HostName = values[0];
            PortNumber = Convert.ToInt32(values[1]);
            UserName = values[2];
            Pass = values[3];
        }

        // Create a method to raise the ApplicationDataObjectChanged event.
        private void OnApplicationDataObjectChanged(EventArgs e)
        {
            EventHandler handler = ApplicationDataObjectChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Save data to a file in IsolatedStorage
        /// </summary>
        /// <param name="isoFileName">The name of the file</param>
        /// <param name="value">The string value to write to the given file</param>
        private void SaveDataToIsolatedStorage(string isoFileName, string value)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var sw = new StreamWriter(isoStore.OpenFile(isoFileName, FileMode.Create));
                sw.Write(value);
                sw.Close();
            }
        }

        /// <summary>
        /// Call GetData on a different thread, i.e, asynchronously
        /// </summary>
        public void GetDataAsync()
        {
            // Call the GetData method on a new thread.
            var t = new Thread(GetData);
            t.Start();
        }

        /// <summary>
        /// Retrieve data from IsolatedStorage
        /// </summary>
        private void GetData()
        {
            // Check to see if data exists in Isolated Storage 
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (isoStore.FileExists("xTradeDataFile.txt"))
            {
                // This method loads the data from Isolated Storage, if it is available.
                var sr = new StreamReader(isoStore.OpenFile("xTradeDataFile.txt", FileMode.Open));
                string data = sr.ReadToEnd();

                sr.Close();

                DeSerializeSettings(data);
            }
        }

        #endregion

        #region Exchange data

        public event EventHandler ApplicationDataObjectExchange;

        private ReqClass _nreq;

        public ReqClass Nreq
        {
            get { return _nreq; }
            set
            {
                if (value != _nreq)
                {
                    _nreq = value;
                    OnApplicationDataObjectExchange(EventArgs.Empty);
                }
            }
        }

        private void OnApplicationDataObjectExchange(EventArgs e)
        {
            EventHandler handler = ApplicationDataObjectExchange;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region inf data

        public event EventHandler ApplicationDataObjectInf;

        private InfData _inf;

        public InfData Inf
        {
            get { return _inf; }
            set
            {
                if (value != _inf)
                {
                    _inf = value;
                    OnApplicationDataObjectInf(EventArgs.Empty);
                }
            }
        }

        private void OnApplicationDataObjectInf(EventArgs e)
        {
            EventHandler handler = ApplicationDataObjectInf;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region inf data

        public event EventHandler ApplicationDataObjectProdClass;

        private ProdClass _PrCl;

        public ProdClass PrCl
        {
            get { return _PrCl; }
            set
            {
                if (value != _PrCl)
                {
                    _PrCl = value;
                    OnApplicationDataObjectProdClass(EventArgs.Empty);
                }
            }
        }

        private void OnApplicationDataObjectProdClass(EventArgs e)
        {
            EventHandler handler = ApplicationDataObjectProdClass;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region inf data

        public event EventHandler ApplicationDataObjectSelProd;

        private ReqItem _selProd;

        public ReqItem SelProd
        {
            get { return _selProd; }
            set
            {
                if (value != _selProd)
                {
                    _selProd = value;
                    OnApplicationDataObjectSelProd(EventArgs.Empty);
                }
            }
        }

        private void OnApplicationDataObjectSelProd(EventArgs e)
        {
            EventHandler handler = ApplicationDataObjectSelProd;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
    }
}