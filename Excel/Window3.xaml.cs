using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wiring
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        private Wire _wire;
        public Window3(Wire wire)
        {
            InitializeComponent();
            _wire = wire;
            _wire.Addnotations = null;
            textBox.Focus();
        }

        private void textBox_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (textBox.Text.Length > 2)
                {
                    _wire.Addnotations = textBox.Text;
                    FileOperations.SaveComment(_wire.NameOfCabinet, _wire);
                    this.Close();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(textBox.Text.Length > 2)
            {
                _wire.Addnotations = textBox.Text;
                FileOperations.SaveComment(_wire.NameOfCabinet, _wire);
                this.Close();
            }

        }
    }
}
