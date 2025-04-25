using DocumentFormat.OpenXml.Drawing.Charts;
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
    /// Interaction logic for WireToShow.xaml
    /// </summary>
    public partial class WireToShow : Window
    {
        private Data _data;
        private int _index;
        private int _indexWire;
        public WireToShow(Data data, int index, int indexWire)
        {
            InitializeComponent();
            _data = data;
            _index = index;
            _indexWire = indexWire;


            Dispatcher.Invoke(new Action(() => label1.Content = data.ListOfImportedCabinets[_index][_indexWire].NameOfCabinet));
            Dispatcher.Invoke(new Action(() => label2.Content = data.ListOfImportedCabinets[_index][_indexWire].Number));
            Dispatcher.Invoke(new Action(() => label3.Content = data.ListOfImportedCabinets[_index][_indexWire].DtSource));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _data.ListOfImportedCabinets[_index][_indexWire].IsConfirmed = true;
            //     MainWindow.MyWindow.RefreshList();  myData.ListOfImportedCabinets[_findedCabinetIndex][1]
            this.Close();
        }
    }
}
