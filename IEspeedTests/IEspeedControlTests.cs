using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IEspeedLibrary;
using System.Windows.Forms;

namespace IEspeedTests
{
    [TestClass]
    public class IEspeedControlTests
    {
        [TestMethod]
        public void Constructor_Test1()
        {
            IEspeedControl control = new IEspeedControl();

            Assert.IsNotNull(control);
            Assert.IsNotNull(control.hWnd);
            
            control.InitIEspeed();
            control.Open("www.google.com");

            Assert.IsTrue(control.HTML.Contains("google"));

            control.Open("www.hintertuxergletscher.at");

            Assert.IsTrue(control.HTML.Contains("hintertux"));

            control.navigateBack();

            Assert.IsTrue(control.HTML.Contains("google"));
        }
    }
}
