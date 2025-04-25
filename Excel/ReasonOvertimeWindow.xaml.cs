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
    /// Interaction logic for ReasonOvertime.xaml
    /// </summary>
    public partial class ReasonOvertimeWindow : Window
    {
        public ReasonOvertimeWindow()
        {
            InitializeComponent();
            comboBoxReasons.ItemsSource = Data.ReasonList;
            Data.ReasonDT = null;

            textBoxOtherReason.Visibility = Visibility.Hidden;
            labelOtherReason.Visibility = Visibility.Hidden;
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxReasons.SelectedItem.ToString() == "Inne - należy dopisać wyjaśnienie")
            {
                textBoxOtherReason.Visibility = Visibility.Visible;
                labelOtherReason.Visibility = Visibility.Visible;
            }
            else
            {
                Data.ReasonDT = comboBoxReasons.SelectedItem.ToString();
                this.Close();

                textBoxOtherReason.Visibility = Visibility.Hidden;
                labelOtherReason.Visibility = Visibility.Hidden;
            }
        }

        private void textBoxOtherReason_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && textBoxOtherReason.Text.Length >= 2)
            {
                Data.ReasonDT = textBoxOtherReason.Text;
                this.Close();

            }
        }
    }
}
