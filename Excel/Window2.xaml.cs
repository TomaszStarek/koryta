using DocumentFormat.OpenXml.Vml;
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
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
            Data.LoggedPerson = "";
            textBox.Focus();
        }

        private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
                if (e.Key == Key.Enter)
                {
                    LoggingUser.findUser(textBox.Text);
                    if(Data.LoggedPerson != null && Data.LoggedPerson.Length > 2)
                    {

                        MainWindow subWindow = new MainWindow();
                        subWindow.Show();
                    // subWindow.ShowDialog();
                        this.Close();
                    }
                    else
                        textBox.Text = string.Empty;

                }
        }


    }
}
