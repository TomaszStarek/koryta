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
    /// Interaction logic for ReasonOvertimeWindowKafelki.xaml
    /// </summary>
    public partial class ReasonOvertimeWindowKafelki : Window
    {
        public ReasonOvertimeWindowKafelki()
        {
            InitializeComponent();
            Data.ReasonDT = null;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            // Pobranie klikniętego przycisku
            if (sender is Button clickedButton)
            {
                // Pobranie tekstu z przycisku
                Data.ReasonDT = clickedButton.Content is TextBlock textBlock ? textBlock.Text : clickedButton.Content.ToString();
                this.Close();

            }


        }
    }
}
