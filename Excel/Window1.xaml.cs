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
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            Data.SetNumber = null;
            textBox.Focus();
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Sprawdź, czy wprowadzone dane to cyfry
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Zezwalaj na klawisze kontrolne (np. Backspace, Delete)
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Escape)
            {
                e.Handled = false;
            }
        }

        // Metoda sprawdzająca, czy tekst to liczba
        private bool IsTextNumeric(string text)
        {
            return int.TryParse(text, out _); // Możesz użyć innych metod, jeśli akceptujesz liczby zmiennoprzecinkowe
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && textBox.Text.Length <= 8)
            {
                    Data.SetNumber = textBox.Text;
                    this.Close();
                
            }
        }
    }
}
