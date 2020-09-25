using System;
using System.Threading;
using System.Windows.Forms;
using IEspeedLibrary;

namespace IEspeed
{
    public partial class IEspeed : Form
    {
        public IEspeedControl BrowserControl { get; set; }
        public IEspeedControl BrowserControl2 { get; set; }
        private string url;

        public IEspeed()
        {
            InitializeComponent();
            url = "www.google.com";
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
            BrowserControl.Open(url);

            BrowserControl2 = new IEspeedControl()
            {
                Dock = DockStyle.Fill
            };

            this.Hide();

            Form newForm = new Form();
            newForm.Controls.Add(BrowserControl2);
            BrowserControl2.OnBrowserReady += OpenAndMax;
            BrowserControl2.InitIEspeed();
            BrowserControl2.Open("www.nhl.com");
            newForm.Show();
        }

        private void OpenAndMax()
        {
            //BrowserControl.Open(url);
            //WindowState = FormWindowState.Maximized;
        }
    }
}
