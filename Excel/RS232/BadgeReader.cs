using DocumentFormat.OpenXml.Vml;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wiring;
using static System.Net.Mime.MediaTypeNames;

namespace Wiring
{
    public class BadgeReader : ComPort
    {
        public bool CameraIsWorking = false;
        private string _serialNumber;
        private string _lineReadIn;

        public BadgeReader(string com,System.Windows.Controls.TextBox textBox) : base(com, textBox)
        {
        }

        public override void port_DataReceived(object sender, SerialDataReceivedEventArgs rcvdData)
        {
                
            while (Port.BytesToRead > 0)
            {

                _lineReadIn += Port.ReadExisting();
                //   lineReadIn += Environment.NewLine;

                Thread.Sleep(15);
            }

            _lineReadIn = Regex.Replace(_lineReadIn, @"\s+", string.Empty);

            LoggingUser.findUser(_lineReadIn);

            displayTextReadIn(_lineReadIn);

            _lineReadIn = String.Empty;

        }

    }
}
