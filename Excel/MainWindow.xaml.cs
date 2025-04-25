using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Vml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Formats.Tar;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Application;
using Image = System.Windows.Controls.Image;

namespace Wiring
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public Data myData = new Data();

        private Wire _wire = new Wire();
        private int _findedCabinetIndex = 0;
        public static MainWindow MyWindow { get; private set; }
        private static List<string> ListOfNames = new List<string>();


        private DispatcherTimer timer, timer2;
        public BadgeReader reader;
        private static bool _searchingMode = false;

        private void Timer_Tick2(object sender, EventArgs e)
        {
            UpdateTimer();

        }

        private void UpdateTimer()
        {
            var selectedWire = listView.SelectedItem as Wire;

            if (selectedWire == null)
            {
                return;
            }
            // Do something with the selected wire
            if (!Skipped)
            {
                if (selectedWire.WireStatus != (int?)Data.Status.AllConfirmed)
                {
                    var timespan = DateTime.Now - selectedWire.Start;
                    var seconds = timespan.TotalSeconds;

                    var timespanHandling = DateTime.Now - Data.StartHandling;
                    var secondsHandling = timespanHandling.TotalSeconds - seconds;


                    selectedWire.HandlingTime = Math.Round(secondsHandling, 1);
                    selectedWire.Seconds = Math.Round(seconds, 1);
                    if (selectedWire.WireStatus == 0)
                        LabelValue = Math.Round(selectedWire.Seconds + secondsHandling, 1);
                    else if (selectedWire.WireStatus == 1)
                        LabelValue = Math.Round(selectedWire.Seconds + selectedWire.SecondsSource + secondsHandling, 1);
                    else if (selectedWire.WireStatus == 2)
                        LabelValue = Math.Round(selectedWire.Seconds + selectedWire.SecondsDestination + secondsHandling, 1);
                }
                else
                    LabelValue = Math.Round(selectedWire.Seconds + selectedWire.SecondsSource + selectedWire.SecondsDestination + selectedWire.HandlingTime, 1);
            }


            //if (LabelValue > selectedWire.TimeForExecuting)
            //    selectedWire.Overtime = true;
            //else
            //    selectedWire.Overtime = false;
            //Overtime = selectedWire.Overtime;
        }

        public MainWindow()
        {
            InitializeComponent();
            MyWindow = this;
            Application.Current.MainWindow = this;
            DataContext = this;
            //  reader = new BadgeReader("COM10", textBoxReader);
            LoadDataFromExcel(); //pobieranie danych z listy excel
            FileOperations.ReadMemory(ref _findedCabinetIndex, myData.ListOfImportedCabinets, @"memory.txt"); // czytanie danych na temat ostatniej robionej szafy

            listView.ItemsSource = myData.ListOfImportedCabinets[_findedCabinetIndex]; //wyświetlanie danych z listy jako listview

            MoveDownSelectedItemFromList(listView); //odświeżenie wyświerlanych danych na aplikacji

            Dispatcher.Invoke(new Action(() => textBlockSet.Text = $"Set:{Data.SetNumber}")); //wyświetlanie numeru seta

            if(Data.LoggedPerson != null)
            {
                Dispatcher.Invoke(new Action(() => textBlockLogged.Text = $"Zalogowany/a: {Data.LoggedPersonBT}"));
                buttonLogging.Content = "Wyloguj";
                buttonLogging.Visibility = Visibility.Visible;
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1); // Set the delay time here (1 second in this example)
            timer.Tick += Timer_Tick;

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(100); // Set the delay time here (1 second in this example)
            timer2.Tick += Timer_Tick2;
            timer2.Start();
            //  SetTimer();
            //  aTimer.Start();
            CountSummaryTime(myData.ListOfImportedCabinets[_findedCabinetIndex]);
            Data.StartHandling = DateTime.Now;
          //  CountProgress();
        }


        private void LoadDataFromExcel()
        {
            string fileName = "\\\\KWIPUBV04\\General$\\Enercon\\Shared\\wiring\\PrzewodyProgramWszystkie.xlsx"; //śzieżka pod którą jest lista excel z której są pobierane dane

         //   string fileName = "C:\\ener\\PrzewodyProgramWszystkie.xlsx"; //śzieżka pod którą jest lista excel z której są pobierane dane


            using (var excelWorkbook = new XLWorkbook(fileName)) //otwiera podany plik excel
            {
                //           var nonEmptyDataRows = excelWorkbook.Worksheet(2).RowsUsed();

                myData.ListOfImportedCabinets = new List<List<Wire>>(); //tworzy nową listę szaf w której są listy przewodów do zrobienia

                var counter = 0;
                foreach (var item in excelWorkbook.Worksheets)
                {
                    ListOfNames.Add(item.Name); // lista nazw szaf potrzebna do wyboru szafy poprzez combobox

                    if (!item.Name.Equals("Podsumowanie"))  // nazwa zakładki nie może być: "Podsumowanie"
                    {
                        var nonEmptyDataRows = item.RowsUsed(); //czytamy tylko wiersze które nie są puste
                        myData.ListOfImportedCabinets.Add(new List<Wire>());  //dodajemy nową listę np. szafę xxxx1

                        foreach (var dataRow in nonEmptyDataRows) //iterujemy po każdym wierszu który załadowaliśmy z aplikacji
                        {
                            if (dataRow.RowNumber() >= 3) //zaczyna od 3 wiersza
                            {
                                _wire = new Wire(); //tworzymy nowy przewód i dodajemy do niego atrybuty:

                                _wire.NameOfCabinet = item.Name; //nazwa szafy brana jest z nazwy zakładki
                                _wire.Number = dataRow.Cell(1).Value.GetText(); //czytamy pierwszą kolumnę jako numer itd
                                _wire.DtSource = dataRow.Cell(4).Value.GetText();
                                _wire.WireEndDimensionSource = dataRow.Cell(7).Value.GetText();
                                _wire.WireEndTerminationSource = dataRow.Cell(6).Value.GetText();

                                _wire.DtTarget = dataRow.Cell(8).Value.GetText();
                                _wire.WireEndTerminationTarget = dataRow.Cell(10).Value.GetText();
                                _wire.WireEndDimensionTarget = dataRow.Cell(11).Value.GetText();
                                _wire.Colour = dataRow.Cell(12).Value.GetText();
                                _wire.CrossSection = ParseFromStringToDouble(dataRow.Cell(13).Value.GetText().Replace('.',','));
                                _wire.Type = dataRow.Cell(14).Value.GetText();
                                _wire.Lenght = ParseFromStringToDouble(dataRow.Cell(16).Value.GetText());

                                _wire.TimeForExecuting = ParseFromStringToDouble(dataRow.Cell(18).Value.GetText());

                                myData.ListOfImportedCabinets[counter].Add(_wire); //finalne dodanie przewodu do listy
                            }
                        }

                        counter++;
                    }
                }

            }
            comboBox.ItemsSource = ListOfNames; //kopiowanie nazw szaf do comboboxa żeby były one do wyboru
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double _labelValue;
        public double LabelValue
        {
            get => _labelValue;
            set
            {
                _labelValue = value;
                OnPropertyChanged(nameof(LabelValue)); // Notify UI of the change
            }
        }
        private double? _totalTime;
        public double? TotalTime
        {
            get => _totalTime;
            set
            {
                _totalTime = value;
                OnPropertyChanged(nameof(TotalTime)); // Notify UI of the change
            }
        }
        private double? _totalExpectedTime;
        public double? TotalExpectedTime
        {
            get => _totalExpectedTime;
            set
            {
                _totalExpectedTime = value;
                OnPropertyChanged(nameof(TotalExpectedTime)); // Notify UI of the change
            }
        }
        private bool _overtime;
        public bool Overtime
        {
            get => _overtime;
            set
            {
                if (_overtime != value)
                {
                    _overtime = value;
                    OnPropertyChanged(nameof(Overtime)); // Notify UI of the change
                }
            }
        }
        private bool _skipped;
        public bool Skipped
        {
            get => _skipped;
            set
            {
                if (_skipped != value)
                {
                    _skipped = value;
                    OnPropertyChanged(nameof(Skipped)); // Notify UI of the change
                }
            }
        }


        private void CountSummaryTime(List<Wire> list)
    {
            TotalTime = Math.Round(list
                .Where(w => w.WireStatus == 3) // Filtruj po statusie
                .Sum(w => (w.Seconds + w.HandlingTime + w.SecondsDestination + w.SecondsSource)),1);         // Zsumuj Time

             TotalExpectedTime = Math.Round(list
                .Where(w => w.WireStatus == 3) // Filtruj po statusie
                .Sum(w => w.TimeForExecuting), 1);          // Zsumuj Time
            if(TotalTime > TotalExpectedTime)
                Overtime = true;
            else Overtime = false;
        }


    private void Timer_Tick(object sender, EventArgs e)
        {
            expander.IsExpanded = false; // Hide the ListView when the timer ticks
            timer.Stop(); // Stop the timer after hiding
        }
        private void Expander_MouseEnter(object sender, MouseEventArgs e)
        {
            //  listView.Width = 300;
            expander.IsExpanded = true;
            timer.Stop();
        }

        private void Expander_MouseLeave(object sender, MouseEventArgs e)
        {
            //  listView.Width = 10;
            timer.Start();
            // expander.IsExpanded = false;
        }
        private void expander_GotMouseCapture(object sender, MouseEventArgs e)
        {
            //  listView.Width = 500;
            expander.IsExpanded = true;
            timer.Stop();
        }

        public double ParseFromStringToDouble(string stringToParse)
        {
            if (stringToParse.Contains("m"))// czasami potrafiło się pojawić m w listach (podawanie długości w metrach zamiast mm)
                stringToParse = stringToParse.Substring(0, stringToParse.Length - 3); // usuwanie wtedy 3 ostatnich liter -> mm2
            double result;
            if (Double.TryParse(stringToParse, out result)) //parsowanie danych na double 
                return result;
            else return 0.0;  //jeśli się nie uda to zwraca 0.0
        }

        public void ClearAllConfirms() //czyści listę i ładuje na nowo z danych z excela
        {
            myData.ListOfImportedCabinets.Clear();
            LoadDataFromExcel();
        }


        private void ChooseImage(int index, Image image, int PictureNumber) //wyświetlanie konkretnego zdjęcia w podanej kontrolce Image
        {

            var selectedWire = listView.SelectedItem as Wire;

            if (selectedWire == null)
                return;

            //var folderCabinetName = myData.ListOfImportedCabinets[_findedCabinetIndex][index].NameOfCabinet;
            //var folderWireName = myData.ListOfImportedCabinets[_findedCabinetIndex][index].Number;
            var folderCabinetName = selectedWire.NameOfCabinet;
            var folderWireName = selectedWire.Number;

            var nameOfImage = @$"\{folderCabinetName}\{folderWireName}\{PictureNumber}.png";

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory
                            + nameOfImage))
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new Action(() =>
                        {
                            image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory
                            + nameOfImage, UriKind.Absolute));
                        }));
                }
                catch (Exception)
                {
                    ;
                }
            }
            else
            {
                if(image.Source != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() =>
                    {
                        image.Source = null;
                    }));
                }

            }


        }



        //public void HideImages() nieużywane ale zostawię bo można użyć do chowania wyświetlanych zdjęć na które się kliknie
        //{
        //    foreach (Window item in App.Current.Windows)
        //    {
        //        if (item != this)
        //        {
        //            Dispatcher.Invoke(new Action(() => item.Close()));

        //        }
        //    }
        //}

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) //odpalane przy wyborze nowej safy poprzez combobox
        {
            if (comboBox.SelectedIndex != -1)
            {
                Window1 subWindow = new Window1();
                subWindow.ShowDialog();

                if (Data.SetNumber == null)
                {
                    MessageBox.Show("Nie podano numeru seta!");
                    return;
                }


                Dispatcher.Invoke(new Action(() => textBlockSet.Text = $"Set:{Data.SetNumber}"));
                //   string inputRead = new InputBox("Insert something", "Title", "Arial", 20).ShowDialog();

                ClearAllConfirms();
                Dispatcher.Invoke(new Action(() => labelPotwierdzonoWszystkiePrzewody.Visibility = Visibility.Hidden));

                _findedCabinetIndex = comboBox.SelectedIndex;

                var name = myData.ListOfImportedCabinets[_findedCabinetIndex][0].NameOfCabinet;

                if (File.Exists($@"{name}_{Data.SetNumber}")) //sprawdzanie czy już dana szafa była robiona ->jeśli była to ładuje dane na temat potwierdzonych przewodów
                {
                    FileOperations.ReadMemory(ref _findedCabinetIndex, myData.ListOfImportedCabinets, $@"{name}_{Data.SetNumber}"); // dane są zapisywane w pliku nazwaszafy_numerseta
                }
                else
                {
                    //   myData.ListOfImportedCabinets[_findedCabinetIndex][1].IsConfirmed = true; 
                    //   listView.ItemsSource = myData.ListOfImportedCabinets[_findedCabinetIndex];
                }
                myData.ListOfImportedCabinets[_findedCabinetIndex][1].IsConfirmed = true;

                listView.ItemsSource = myData.ListOfImportedCabinets[_findedCabinetIndex]; // ładuje nową szafę do listview

                MoveDownSelectedItemFromList(listView); //odświeżanie widoku aplikcaji

                Dispatcher.Invoke(new Action(() => btnTargetConfirm.Visibility = Visibility.Visible));

                Data.StartHandling = DateTime.Now;
            }

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var item = listView.SelectedItem;
            if (item != null)
            {
                // MessageBox.Show(item.ToString());
            }
            else
                return;

            int index = listView.Items.IndexOf(item);

            var hux = myData.ListOfImportedCabinets[_findedCabinetIndex][index].IsConfirmed = true;

            MoveDownSelectedItemFromList(listView);
            listView.Items.Refresh();

            var allValid = myData.ListOfImportedCabinets[_findedCabinetIndex].Any() && myData.ListOfImportedCabinets[_findedCabinetIndex].All(item => item.IsConfirmed);

            if (allValid)
            {
                Dispatcher.Invoke(new Action(() => labelPotwierdzonoWszystkiePrzewody.Visibility = Visibility.Visible));
            }
            else
                Dispatcher.Invoke(new Action(() => labelPotwierdzonoWszystkiePrzewody.Visibility = Visibility.Hidden));


        }

        private void MoveDownSelectedItemFromList(ListView listView)
        {
            if (listView.SelectedIndex < listView.Items.Count - 1)
            {
                listView.SelectedIndex = listView.SelectedIndex + 1;
            }
        }

        private void RefreshList(ListView listView)
        {
            if (listView.SelectedIndex < listView.Items.Count - 1)
            {
                listView.SelectedIndex = listView.SelectedIndex + 1;
                listView.SelectedIndex = listView.SelectedIndex - 1;
            }
            else
            {
                listView.SelectedIndex = listView.SelectedIndex - 1;
                listView.SelectedIndex = listView.SelectedIndex + 1;
            }
        }

        private void listView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = listView.SelectedItem; //sprawdzanie czy mamy jakieś przewody do zatwierdzenia
            if (item != null)
            {
                // MessageBox.Show(item.ToString());
            }
            else
                return;

            if (_searchingMode)
            {
                Dispatcher.Invoke(new Action(() => textBox.Text = string.Empty));
                listView.ItemsSource = myData.ListOfImportedCabinets[_findedCabinetIndex];
            }


            int index = listView.Items.IndexOf(item);
            myData.ListOfImportedCabinets[_findedCabinetIndex][index].Start = DateTime.Now; //sprawdzanie statusu wykonania przewodu
            //////////var item = (sender as ListView).SelectedItem;
            //////////if (item != null)
            //////////{
            //////////   // MessageBox.Show(item.ToString());
            //////////}
            //////////else
            //////////    return;

            //////////int index = listView.Items.IndexOf(item);

            //////////var hux = myData.ListOfImportedCabinets[_findedCabinetIndex][index].IsConfirmed = true;
            //////////listView.Items.Refresh();

            //           ShowImage(_findedCabinetIndex, index);

            //foreach (var itemf in myData.ListOfImportedCabinets)
            //{

            //}

        }


        private void TextBlock_TargetUpdated(object sender, DataTransferEventArgs e) //wykorzystywane do wyświetlania obrazków
        {
            var selectedNumber = listView.SelectedIndex;
            if (selectedNumber >= 0)
            {
                //  ChooseImage(selectedNumber);
                ChooseImage(selectedNumber, image_Source, 2);
                ChooseImage(selectedNumber, image_All, 1);
                ChooseImage(selectedNumber, image_Target, 3);
            }

        }

        private void image_Source_GotMouseCapture(object sender, MouseEventArgs e) //po kliknięcu na dany obrazek odpala się zdjęcie, które kliknęlismy w windowsowym edytorze zdjęć
        {
            Image image = (Image)sender;
            int PictureNumberToShow = 0;
            switch (image.Name)
            {
                case "image_Source":
                    PictureNumberToShow = 2;
                    break;
                case "image_All":
                    PictureNumberToShow = 1;
                    break;
                case "image_Target":
                    PictureNumberToShow = 3;
                    break;
                default:
                    PictureNumberToShow = 1;
                    break;
            }

            var selectedWire = listView.SelectedItem as Wire;

            if (selectedWire == null)
                return;

            //var folderCabinetName = myData.ListOfImportedCabinets[_findedCabinetIndex][index].NameOfCabinet;
            //var folderWireName = myData.ListOfImportedCabinets[_findedCabinetIndex][index].Number;
            var folderCabinetName = selectedWire.NameOfCabinet;
            var folderWireName = selectedWire.Number;

            var nameOfImage = @$"\{folderCabinetName}\{folderWireName}\{PictureNumberToShow}.png";

            var selectedNumber = listView.SelectedIndex;
            if (selectedNumber >= 0)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(@$"{AppDomain.CurrentDomain.BaseDirectory}\{nameOfImage}") { UseShellExecute = true });
                }
                catch (Exception)
                {
                    ;
                }
            }
        }

        private void SourceConfirm_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            if (Data.LoggedPerson == null || Data.LoggedPerson.Length < 2)
            {
                MessageBox.Show("Operacja wymaga zalogowania się!");
                return;
            }

            myData.TextVisibility ^= true;

            var item = listView.SelectedItem; //sprawdzanie czy mamy jakieś przewody do zatwierdzenia
            if (item != null)
            {
                // MessageBox.Show(item.ToString());
            }
            else
                return;


            var selectedWire = listView.SelectedItem as Wire;

            if (selectedWire == null)
            {
                return;
                // Do something with the selected wire
                // MessageBox.Show($"Selected Wire: {bus}, {number}");
            }
            
            int index = listView.Items.IndexOf(item);
            var statusValue = myData.ListOfImportedCabinets[_findedCabinetIndex][index].WireStatus; //sprawdzanie statusu wykonania przewodu

            var timespan = DateTime.Now - myData.ListOfImportedCabinets[_findedCabinetIndex][index].Start;
            var seconds = Math.Round(timespan.TotalSeconds,2); 
         //   myData.ListOfImportedCabinets[_findedCabinetIndex][index].Seconds = Math.Round(myData.ListOfImportedCabinets[_findedCabinetIndex][index].Seconds  + seconds,2);
         //dodawanie powodu
            //if(selectedWire.Overtime && selectedWire.WireStatus != (int?)Data.Status.AllConfirmed)
            //{
            //    ReasonOvertimeWindowKafelki subWindow = new ReasonOvertimeWindowKafelki();
            //    subWindow.ShowDialog();

            //    if (Data.ReasonDT == null)
            //    {
            //        MessageBox.Show("Nie podano powodu DT!");
            //        return;
            //    }
            //    else
            //        selectedWire.ReasonDT = Data.ReasonDT;
            //}

       //     var timespanHandling = DateTime.Now - Data.StartHandling;
      //      var secondsHandling = timespanHandling.TotalSeconds;

            switch (btn.Name) // sprawdzanie który przysk wybraliśmy i w zależności od niego dodajemy do parametru wireStatus wartość 1 = potwierdzone source,2 = potwierdzone target,3 = potwierdzone wszystko
            {
                case "btnSourceConfirm":
                    if (statusValue != (int?)Data.Status.SourceConfirmed && statusValue < (int?)Data.Status.AllConfirmed)
                    {
                        myData.ListOfImportedCabinets[_findedCabinetIndex][index].WireStatus += (int?)Data.Status.SourceConfirmed;
                        selectedWire.SecondsSource = seconds + selectedWire.HandlingTime;
                   //     selectedWire.HandlingTime = secondsHandling - seconds;

                        if (statusValue == (int?)Data.Status.TargetConfirmed)
                            CountSummaryTime(myData.ListOfImportedCabinets[_findedCabinetIndex]);

                        Data.StartHandling = DateTime.Now;
                    }
                    else if (statusValue == (int?)Data.Status.SourceConfirmed || statusValue == (int?)Data.Status.AllConfirmed)
                    {
                        myData.ListOfImportedCabinets[_findedCabinetIndex][index].WireStatus -= (int?)Data.Status.SourceConfirmed;
                        selectedWire.HandlingTime = 0;
                        Data.StartHandling = Data.StartHandling.AddSeconds(-selectedWire.SecondsSource);
                        //  Dispatcher.Invoke(new Action(() => btnSourceConfirm.Content = "Potwierdz Source"));
                    }
                   // listView.Items.Refresh();
                    break;
                case "btnTargetConfirm":
                    if (statusValue != (int?)Data.Status.TargetConfirmed && statusValue < (int?)Data.Status.AllConfirmed)
                    {
                        myData.ListOfImportedCabinets[_findedCabinetIndex][index].WireStatus += (int?)Data.Status.TargetConfirmed;
                        selectedWire.SecondsDestination = seconds + selectedWire.HandlingTime;
                     //   selectedWire.HandlingTime = secondsHandling - seconds;

                        //  Dispatcher.Invoke(new Action(() => btnTargetConfirm.Content = "Odznacz Target"));
                        if(statusValue == (int?)Data.Status.TargetConfirmed)
                            CountSummaryTime(myData.ListOfImportedCabinets[_findedCabinetIndex]);

                        Data.StartHandling = DateTime.Now;
                    }
                    else if (statusValue == (int?)Data.Status.TargetConfirmed || statusValue == (int?)Data.Status.AllConfirmed)
                    {
                        myData.ListOfImportedCabinets[_findedCabinetIndex][index].WireStatus -= (int?)Data.Status.TargetConfirmed;
                        selectedWire.HandlingTime = 0;
                        Data.StartHandling = Data.StartHandling.AddSeconds(-selectedWire.SecondsDestination);
                        //  Dispatcher.Invoke(new Action(() => btnTargetConfirm.Content = "Potwierdź Target"));
                    }
                   // listView.Items.Refresh();
                    break;
                case "btnConfirmBoth":
                    if (statusValue != (int?)Data.Status.AllConfirmed)
                    {
                        myData.ListOfImportedCabinets[_findedCabinetIndex][index].WireStatus = (int?)Data.Status.AllConfirmed;
                     //   selectedWire.HandlingTime = secondsHandling - seconds;
                        Data.StartHandling = DateTime.Now;

                        CountSummaryTime(myData.ListOfImportedCabinets[_findedCabinetIndex]);
                    }

                    else
                    {
                        myData.ListOfImportedCabinets[_findedCabinetIndex][index].WireStatus = (int?)Data.Status.Unconfirmed;
                        selectedWire.HandlingTime = 0;
                        CountSummaryTime(myData.ListOfImportedCabinets[_findedCabinetIndex]);
                        //   btnConfirmBoth.Content = "asdasd";
                        // Dispatcher.Invoke(new Action(() => btnConfirmBoth.Content = "Potwierdź wszystkie"));
                    }
                   // listView.Items.Refresh();
                    break;

                default:
                    break;
            }
            myData.ListOfImportedCabinets[_findedCabinetIndex][index].MadeBy = Data.LoggedPerson;

            double countOfProgress = 0;
            for (int i = 0; i < myData.ListOfImportedCabinets[_findedCabinetIndex].Count; i++)
            {
                if (myData.ListOfImportedCabinets[_findedCabinetIndex][i].WireStatus == (int?)Data.Status.SourceConfirmed ||
                    myData.ListOfImportedCabinets[_findedCabinetIndex][i].WireStatus == (int?)Data.Status.TargetConfirmed)
                    countOfProgress++;
                else if (myData.ListOfImportedCabinets[_findedCabinetIndex][i].WireStatus == (int?)Data.Status.AllConfirmed)
                    countOfProgress += 2;

            }

            myData.ListOfImportedCabinets[_findedCabinetIndex].ForEach(x => x.Progress = Math.Round((countOfProgress / (myData.ListOfImportedCabinets[_findedCabinetIndex].Count * 2) * 100), 2));
          //  CountProgress();

            //   if (myData.ListOfImportedCabinets[_findedCabinetIndex][index].WireStatus == (int?)Data.Status.AllConfirmed) //sprawdzenie czy przewód ma wszystko już potwierdzone
            //  MoveDownSelectedItemFromList(listView); //jeśli tak to przechodzimy do kolejnego przewodu
            //   else

            myData.ListOfImportedCabinets[_findedCabinetIndex][index].DateOfFinish = DateTime.Now;

            FileOperations.WriteListStatusToFile(_findedCabinetIndex, myData.ListOfImportedCabinets[_findedCabinetIndex], LabelValue); //zapisywanie do pamięci danych o statusie potwierdzeń wszystkich przewodów w danej szafie
 

            var allValid = myData.ListOfImportedCabinets[_findedCabinetIndex].Any() && myData.ListOfImportedCabinets[_findedCabinetIndex].All(item => item.WireStatus == 3);


            if (allValid) //sprawdzanie czy wykonaliśmy już wszystkie przeowdy
            {
                Dispatcher.Invoke(new Action(() => labelPotwierdzonoWszystkiePrzewody.Visibility = Visibility.Visible));
                FileOperations.SaveLog(myData.ListOfImportedCabinets[_findedCabinetIndex][0].NameOfCabinet, myData.ListOfImportedCabinets[_findedCabinetIndex]);
            }
            else
            {
                Dispatcher.Invoke(new Action(() => labelPotwierdzonoWszystkiePrzewody.Visibility = Visibility.Hidden));
                FileOperations.SaveSingleLog(myData.ListOfImportedCabinets[_findedCabinetIndex][0].NameOfCabinet, myData.ListOfImportedCabinets[_findedCabinetIndex][index]);

                if (myData.ListOfImportedCabinets[_findedCabinetIndex][index].WireStatus != (int?)Data.Status.AllConfirmed)
                {
                    myData.ListOfImportedCabinets[_findedCabinetIndex][index].Start = DateTime.Now;
                    myData.ListOfImportedCabinets[_findedCabinetIndex][index].Seconds = 0;
                }
                //else
                //    myData.ListOfImportedCabinets[_findedCabinetIndex][index].Seconds = LabelValue - selectedWire.HandlingTime;


                //if (selectedWire.WireStatus == (int?)Data.Status.AllConfirmed)
                //{
                //    if (selectedWire.Addnotations != null && selectedWire.Addnotations.Length > 0)
                //    {
                //        FileOperations.SaveComment(myData.ListOfImportedCabinets[_findedCabinetIndex][0].NameOfCabinet, selectedWire);
                //    }
                //}
            }
         //   Application.Current.MainWindow = this;
        //    this.UpdateLayout();
            RefreshList(listView); // jeśli nie to odświeżamy tylko widok aplikacji
            listView.Items.Refresh();
            Dispatcher.Invoke(new Action(() => textBox.Focus()));
        }

        public static List<List<string>> orginalItems;
        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var selectedWire = listView.SelectedItem as Wire;
                if (selectedWire == null)
                {
                    return;
                    // Do something with the selected wire
                    // MessageBox.Show($"Selected Wire: {bus}, {number}");
                }

                if (listView.Items[0] != selectedWire)
                {
                    // Jeśli poprzedni element ma WireStatus <= 0, zablokuj zmianę zaznaczenia
                    if (selectedWire.WireStatus <= 0)
                    {
                        // Zablokuj zmianę zaznaczenia
                        e.Handled = true;
                        textBox.Text = string.Empty;
                        // Wyświetl komunikat lub wykonaj inną akcję
                        //  MessageBox.Show($"Nie możesz zmienić zaznaczenia, ponieważ element \"{selectedWire.Number}\" nie został zatwierdzony.", "Uwaga");
                        return;
                    }

                }
                //    var tempList = new List<Wire>();
                //    tempList = myData.ListOfImportedCabinets[_findedCabinetIndex];
                //    listView.ItemsSource = tempList.Select(item => item.DtSource.Contains(textBox.Text) || item.DtTarget.Contains(textBox.Text));


                var snToFind = textBox.Text.ToUpper();  // poczukiwany tekst to ten który został wpisany do kontorlki

                //  int curIndex = myData.ListOfImportedCabinets[_findedCabinetIndex].FindIndex(a => a.DtSource.ToUpper().Contains(snToFind));


                if (_searchingMode)
                {
                    var tempList = listView.Items;
                    var curIssandex = myData.ListOfImportedCabinets[_findedCabinetIndex].Where(a => a.DtSource.ToUpper().Contains(snToFind) || a.DtTarget.ToUpper().Contains(snToFind));
                    listView.ItemsSource = curIssandex;
                }
                else
                {
                    int curIndex = myData.ListOfImportedCabinets[_findedCabinetIndex].FindIndex(a => $"{a.DtSource.ToUpper()} <> {a.DtTarget.ToUpper()}".Equals(snToFind));

                    if (curIndex >= 0) // jeśli index jest znaleziony
                    {
                        listView.SelectedIndex = curIndex;
                        listView.Items.Refresh();
                        listView.Focus();
                        // listView.SetSelected(curIndex, true);
                    }
                }


                textBox.Text = string.Empty;

                Dispatcher.Invoke(new Action(() => btnTargetConfirm.Visibility = Visibility.Visible));
                return;
            }
            //if(textBox.Text.Length == 0)
            //    listView.ItemsSource = myData.ListOfImportedCabinets[_findedCabinetIndex];
            //if (_searchingMode)
            //{
            //    // listView.ItemsSource = myData.ListOfImportedCabinets[_findedCabinetIndex];
            //    image_Source.Source = null;
            //    image_All.Source = null;
            //    image_Target.Source = null;

            //    var snToFind2 = textBox.Text.ToUpper();


            //    Dispatcher.Invoke(new Action(() => expander.IsExpanded = true));
            //    var tempList = listView.Items;
            //    var curIssandex = myData.ListOfImportedCabinets[_findedCabinetIndex].Where(a => a.DtSource.ToUpper().Contains(snToFind2));
            //    listView.ItemsSource = curIssandex;
            //}

        }

        private void buttonLogging_Click(object sender, RoutedEventArgs e)
        {
            ListOfNames.Clear();
           // Dispatcher.Invoke(new Action(() => comboBox.Clear()));
            
            Window2 subWindow = new Window2();
            subWindow.ShowDialog();
            this.Close();

            //if (buttonLogging.Content != null)
            //{

            //    if(Data.LoggedPerson == null)
            //    {
            //        Data.LoggedPerson = "";
            //    }
            //    if (buttonLogging.Content.ToString().ToLower().Equals("zaloguj") && Data.LoggedPerson.Length == 0)
            //    {

            //        buttonLogging.Visibility = Visibility.Hidden;


            //        if (Data.LoggedPerson == null)
            //        {
            //            MessageBox.Show("Logowanie się nie powiodło!");
            //            buttonLogging.Content = "Zaloguj";
            //            buttonLogging.Visibility = Visibility.Visible;
            //            return;
            //        }
            //        Dispatcher.Invoke(new Action(() => textBlockLogged.Text = $"Zalogowany/a: {Data.LoggedPerson}"));
            //        buttonLogging.Content = "Wyloguj";
            //        buttonLogging.Visibility = Visibility.Visible;
            //    }
            //    else //if (buttonLogging.Content.ToString().ToLower().Equals("wyloguj")) 
            //    {
            //        Data.LoggedPerson = "";
            //        buttonLogging.Content = "Zaloguj";
            //        buttonLogging.Visibility = Visibility.Visible;
            //        Dispatcher.Invoke(new Action(() => textBlockLogged.Text = $"Zalogowany/a: {Data.LoggedPerson}"));
            //    }

            //}


        }

        private void tex(object sender, KeyEventArgs e)
        {

        }

        private void text(object sender, RoutedEventArgs e)
        {
            {
              //  textBox.Focus();
            }
        }

        private void textBoxFocus(object sender, RoutedEventArgs e)
        {

        }

        private void textBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
           // textBox.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => textBox.Focus()));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _searchingMode = !_searchingMode;

            if (_searchingMode)
            {
                Dispatcher.Invoke(new Action(() => buttonMode.Content = "Ręczny"));
            }
            else
            {
                Dispatcher.Invoke(new Action(() => buttonMode.Content = "Skaner"));
                listView.ItemsSource = myData.ListOfImportedCabinets[_findedCabinetIndex];
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var selectedWire = listView.SelectedItem as Wire;
            if (selectedWire == null)
            {
                // Do something with the selected wire
                // MessageBox.Show($"Selected Wire: {bus}, {number}");
            }



            var snToFind = textBox.Text.ToUpper();

            if (_searchingMode)
            {
                if (selectedWire != null && listView.Items[0] != selectedWire)
                {
                    // Jeśli poprzedni element ma WireStatus <= 0, zablokuj zmianę zaznaczenia
                    if(!Skipped)
                    {
                        if (selectedWire.WireStatus <= 0)
                        {
                            // Zablokuj zmianę zaznaczenia
                            e.Handled = true;
                            textBox.Text = string.Empty;
                            // Wyświetl komunikat lub wykonaj inną akcję
                            MessageBox.Show($"Nie możesz zmienić zaznaczenia, ponieważ element \"{selectedWire.Number}\" nie został zatwierdzony.", "Uwaga");
                            return;
                        }
                    }

                }
                var filteredList = myData.ListOfImportedCabinets[_findedCabinetIndex]
                    .Where(a => a.DtSource.ToUpper().Contains(snToFind) || a.DtTarget.ToUpper().Contains(snToFind));

                listView.ItemsSource = filteredList;
            }
            else
            {
                int curIndex = myData.ListOfImportedCabinets[_findedCabinetIndex]
                    .FindIndex(a => $"{a.DtSource.ToUpper()} <> {a.DtTarget.ToUpper()}".Equals(snToFind));

                if (curIndex >= 0) // Jeśli znaleziono indeks
                {
                    if (selectedWire != null && listView.Items[0] != selectedWire)
                    {
                        if (!Skipped)
                        {
                            // Jeśli poprzedni element ma WireStatus <= 0, zablokuj zmianę zaznaczenia
                            if (selectedWire.WireStatus <= 0)
                            {
                                // Zablokuj zmianę zaznaczenia
                                e.Handled = true;

                                textBox.Text = string.Empty;
                                // Wyświetl komunikat lub wykonaj inną akcję
                                //  MessageBox.Show($"Nie możesz zmienić zaznaczenia, ponieważ element \"{selectedWire.Number}\" nie został zatwierdzony.", "Uwaga");
                                return;
                            }
                        }

                    }


                    listView.SelectedIndex = curIndex;
                    listView.Items.Refresh();
                    listView.Focus();
                    textBox.Text = string.Empty;
                }
            }
        }


        private void listView_GotFocus(object sender, RoutedEventArgs e)
        {

        }
        private object _previousSelectedItem;
        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = listView.SelectedItem; //sprawdzanie czy mamy jakieś przewody do zatwierdzenia
            if (item != null)
            {
                // MessageBox.Show(item.ToString());
            }
            else
                return;


            var selectedWire = listView.SelectedItem as Wire;
            if (selectedWire == null)
            {
                return;
                // Do something with the selected wire
                // MessageBox.Show($"Selected Wire: {bus}, {number}");
            }

            selectedWire.Start = DateTime.Now;
            Skipped = false;
            selectedWire.Skipped = false;
        }

        private void listView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedWire = listView.SelectedItem as Wire;
            if (selectedWire == null)
            {
                return;
                // Do something with the selected wire
                // MessageBox.Show($"Selected Wire: {bus}, {number}");
            }

            if (listView.Items[0] != selectedWire)
            {
                if (!Skipped)
                {
                    // Jeśli poprzedni element ma WireStatus <= 0, zablokuj zmianę zaznaczenia
                    if (selectedWire.WireStatus <= 0)
                    {
                        // Zablokuj zmianę zaznaczenia
                        e.Handled = true;

                        // Wyświetl komunikat lub wykonaj inną akcję
                        MessageBox.Show($"Nie możesz zmienić zaznaczenia, ponieważ element \"{selectedWire.Number}\" nie został zatwierdzony.", "Uwaga");
                        return;
                    }
                }
                
            }

        }
        private void CountProgress()
        {
            double countOfProgress = 0;
            double totalCount = 0;
            for (int i = 0; i < myData.ListOfImportedCabinets[_findedCabinetIndex].Count; i++)
            {
                if (myData.ListOfImportedCabinets[_findedCabinetIndex][i].WireStatus == (int?)Data.Status.SourceConfirmed)
                {
                    countOfProgress += myData.ListOfImportedCabinets[_findedCabinetIndex][i].TimeForExecuting / 2;
                }
                else if (myData.ListOfImportedCabinets[_findedCabinetIndex][i].WireStatus == (int?)Data.Status.TargetConfirmed)
                {
                    countOfProgress += myData.ListOfImportedCabinets[_findedCabinetIndex][i].TimeForExecuting / 2;
                }
                else if (myData.ListOfImportedCabinets[_findedCabinetIndex][i].WireStatus == (int?)Data.Status.AllConfirmed)
                {
                    countOfProgress += myData.ListOfImportedCabinets[_findedCabinetIndex][i].TimeForExecuting;
                }
                totalCount += myData.ListOfImportedCabinets[_findedCabinetIndex][i].TimeForExecuting;

            }
            myData.ListOfImportedCabinets[_findedCabinetIndex].ForEach(x => x.Progress = Math.Round((countOfProgress / (totalCount) * 100), 2));

        }

        private void button_Click_2(object sender, RoutedEventArgs e)
        {
            var selectedWire = listView.SelectedItem as Wire;
            if (selectedWire == null)
            {
                return;
                // Do something with the selected wire
                // MessageBox.Show($"Selected Wire: {bus}, {number}");
            }
            selectedWire.MadeBy = Data.LoggedPerson;
            selectedWire.HandlingTime += selectedWire.Seconds;
            selectedWire.Seconds = 0;
            FileOperations.SaveSingleLog(myData.ListOfImportedCabinets[_findedCabinetIndex][0].NameOfCabinet, selectedWire);
            Skipped = true;
            selectedWire.Skipped = true;
            Data.StartHandling = DateTime.Now;  
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var selectedWire = listView.SelectedItem as Wire;
            if (selectedWire == null)
            {
                return;
                // Do something with the selected wire
                // MessageBox.Show($"Selected Wire: {bus}, {number}");
            }

            Window3 subWindow = new Window3(selectedWire);
            subWindow.ShowDialog();
            RefreshList(listView);
        }
    }
}
