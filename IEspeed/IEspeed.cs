using System;
using System.Windows.Forms;
using IEspeedLibrary;

namespace IEspeed
{
    public partial class IEspeed : Form
    {
        public IEspeedControl BrowserControl { get; set; }
        private string url;

        public IEspeed()
        {
            InitializeComponent();
            url = "about:blank";
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && args[1] != null)
            {
                url = args[1];
            }
            BrowserControl = new IEspeedControl()
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(BrowserControl);
            BrowserControl.OnBrowserReady += OpenAndMax;  
            BrowserControl.InitIEspeed();
        }

        private void OpenAndMax()
        {
            BrowserControl.Open(url);
            WindowState = FormWindowState.Maximized;
        }
    }
}
