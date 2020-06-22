using System;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Threading;
using CefSharp;
using CefSharp.WinForms;
using System.IO;

namespace IEspeedLibrary
{
    [ProgId("IEspeedLibrary.IEspeedControl")]
    [Guid("74627B42-6755-47CB-8402-AB0914774680")]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(IComEvents))]
    public partial class IEspeedControl : UserControl, IComObject, IDisposable
    {

        public delegate void BrowserReady();

        public event BrowserReady OnBrowserReady;

        private ChromiumWebBrowser browser;

        private string html;
        [ComVisible(true)]
        public string HTML
        {
            get
            {
                return html;
            }
            set
            {
                html = value;
                browser.LoadHtml(html);
            }
        }

        [ComVisible(true)]
        public IntPtr hWnd { get; set; }

        public IEspeedControl()
        {
            InitializeComponent();
            hWnd = Handle;
        }

        [ComVisible(true)]
        public void Open(string url)
        {
            browser.Load(url);
        }

        [ComVisible(true)]
        public void navigateBack()
        {
            if (browser.CanGoBack)
            {
                browser.Back();
            }
        }

        [ComVisible(true)]
        public void InitIEspeed()
        {
            this.Invoke(
                (Action)(() =>
                {
                    InitCef();
                    InitBrowser();
                    OnBrowserReady();
                    Controls.Add(browser);
                }));
        }

        //~IEspeedControl()
        //{
        //    Dispose(false);
        //}

        //// Own dispose method to shutdown Cef on main UI Thread and prevent crash SD-8025
        //new public void Dispose()
        //{
        //    Dispose(true);
        //    // This object will be cleaned up by the Dispose method.
        //    // Therefore, you should call GC.SupressFinalize to
        //    // take this object off the finalization queue
        //    // and prevent finalization code for this object
        //    // from executing a second time.
        //    GC.SuppressFinalize(this);
        //}

        public DialogResult DownloadPrompt(string fileName, string fileType)
        {
            OpenSaveForm dialog = new OpenSaveForm();
            dialog.FileNameLabel.Text = fileName;
            dialog.FileTypeLabel.Text = fileType;
            return dialog.ShowDialog();
        }

        [ComRegisterFunction()]
        public static void RegisterClass(string key)
        {
            StringBuilder skey = new StringBuilder(key);
            skey.Replace(@"HKEY_CLASSES_ROOT\", "");
            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(skey.ToString(), true);
            RegistryKey ctrl = regKey.CreateSubKey("Control");
            ctrl.Close();
            RegistryKey inprocServer32 = regKey.OpenSubKey("InprocServer32", true);
            inprocServer32.SetValue("CodeBase", Assembly.GetExecutingAssembly().CodeBase);
            inprocServer32.Close();
            regKey.Close();
        }

        [ComUnregisterFunction()]
        public static void UnregisterClass(string key)
        {
            StringBuilder skey = new StringBuilder(key);
            skey.Replace(@"HKEY_CLASSES_ROOT\", "");
            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(skey.ToString(), true);
            regKey.DeleteSubKey("Control", false);
            RegistryKey inprocServer32 = regKey.OpenSubKey("InprocServer32", true);
            regKey.DeleteSubKey("CodeBase", false);
            regKey.Close();
        }

        private void InitCef()
        {
            CefSettings settings = new CefSettings();
            //settings.CefCommandLineArgs.Add("ignore-certificate-errors", string.Empty);
            if (!Cef.IsInitialized)
            {
                Cef.EnableHighDPISupport();
                Cef.Initialize(settings);
            }
        }

        private void InitBrowser()
        {
            browser = new ChromiumWebBrowser("about:blank")
            {
                Dock = DockStyle.Fill,
                RequestHandler = new BrowserRequestHandler(),
                DownloadHandler = new DownloadRequestHandler(this)
            };
        }

        public class BrowserRequestHandler : CefSharp.Handler.RequestHandler
        {
            protected override bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
            {
                DialogResult dialogResult = MessageBox.Show($"Zertifikatsfehler (ErrorCode: {(int)errorCode} {errorCode}).\nMöchten Sie trotzdem fortfahren?", "Zertifikatsfehler", MessageBoxButtons.YesNo);
                bool confirm = true;
                if (dialogResult == DialogResult.No)
                {
                    confirm = false;
                }
                confirm = true;
                callback.Continue(confirm);
                return true;
            }
        }

        public class DownloadRequestHandler : CefSharp.IDownloadHandler
        {

            public event EventHandler<DownloadItem> OnBeforeDownloadFired;

            public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

            private readonly IEspeedControl control;

            public DownloadRequestHandler(IEspeedControl controlIn)
            {
                control = controlIn;
            }

            public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
            {
                // This line is over complex due to time constraints
                // This line opens the OpenSaveForm on the main UI thread and returns the DialogResult
                // to the excuting thread (a CefSharp thread)
                DialogResult result = (DialogResult)control.Invoke(((Func<string, string, DialogResult>)
                    ((fileName, fileType) => control.DownloadPrompt(fileName, fileType))),
                    downloadItem.SuggestedFileName, downloadItem.MimeType);

                if (!callback.IsDisposed)
                {
                    using (callback)
                    {
                        if (result == DialogResult.Yes)
                        {
                            callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
                        }
                        else if (result == DialogResult.OK)
                        {
                            callback.Continue(Path.GetTempPath() + downloadItem.SuggestedFileName, showDialog: false);
                        }
                    }
                }
            }

            public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
            {
                // Check if this is a temporary file, we know this if it is being saved to the temp folder
                // Not a great way to check, but the easiest way
                int dirEnd = downloadItem.FullPath.LastIndexOf('\\') + 1;
                if (downloadItem.IsComplete && downloadItem.FullPath.Substring(0, dirEnd).Equals(Path.GetTempPath()))
                {
                    System.Diagnostics.Process.Start(downloadItem.FullPath);
                }
            }
        }
    }

    [Guid("DFF92BE5-D3BB-42D2-B8AC-E28C4A0FC3FF")]
    [ComVisible(true)]
    public interface IComObject
    {
        [DispId(0x10000001)]
        void Open(string url);

        [DispId(0x10000002)]
        void navigateBack();

        [DispId(0x10000003)]
        void InitIEspeed();

        string HTML { set; }
        IntPtr hWnd { get; }
    }

    [Guid("F85EAD53-B7F7-43F9-B3B8-4209831536A5")]
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IComEvents
    {
        [DispId(0x00000001)]
        void OnBrowserReady();
    }
}
