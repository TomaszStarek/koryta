using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Wiring
{
    public abstract class ComPort
    {
        internal SerialPort Port { get; set; }
        //     internal SerialPort Port;

        private TextBox _textBox;


        // this will prevent cross-threading between the serial port
        // received data thread & the display of that data on the central thread
        internal delegate void preventCrossThreading(string x);
        internal preventCrossThreading accessControlFromCentralThread;

        public ComPort(string com, TextBox textBox)
        {
            _textBox = textBox;
            //           const string com = "COM4";            
            Port = new SerialPort(com, 115200, Parity.None, 8, StopBits.One);
            //   port.ErrorReceived += new SerialErrorReceivedEventHandler();
            try
            {
                Port.Open();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Error: Port " + com + " jest zajęty\n" + ex, "COM zajęty", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uart exception: " + ex, "Uart exception", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }

            if (Port.IsOpen)
            {
                // set the 'invoke' delegate and attach the 'receive-data' function
                // to the serial port 'receive' event.

                accessControlFromCentralThread = displayTextReadIn;
                Port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

            }
            _textBox = textBox;
        }

        // this is called when the serial port has receive-data for us.
        public abstract void port_DataReceived(object sender, SerialDataReceivedEventArgs rcvdData);


    // this, hopefully, will prevent cross threading.
    public void displayTextReadIn(string ToBeDisplayed)
        {
        //    this.Dispatcher.Invoke(new Action(() => this._textBox.Text = "Wkręć śruby!"));


            Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => {
                    this._textBox.Text = ToBeDisplayed;
                     }));
            //if (_textBox.InvokeRequired)
            //    _textBox.BeginInvoke(accessControlFromCentralThread, ToBeDisplayed);
            //else
            //    _textBox.Text = ToBeDisplayed;
        }





    }
}
