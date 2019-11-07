using System;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Reflection;
using CefSharp;
using CefSharp.WinForms;

namespace IEspeedLibrary
{
    [ProgId("IESpeedLibrary.IESpeedControl")]
    [Guid("74627B42-6755-47CB-8402-AB0914774680")]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(IComEvents))]
    public partial class IEspeedControl : UserControl, IComObject
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
            InitCef();
            InitBrowser();
            OnBrowserReady();
            Controls.Add(browser);
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
                RequestHandler = new BrowserRequestHandler()
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
                callback.Continue(confirm);
                return true;
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

        string HTML { get; set; }
        IntPtr hWnd { get; set; }
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
