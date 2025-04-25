using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace Wiring
{
    public class Data
    {
        public List<List<Wire>> ListOfImportedCabinets { get; set; }

        public static IEnumerable<XElement>? ListOfUsers;
        public static string? SetNumber { get; set; }
        public static string? LoggedPerson { get; set; }
        public static string? LoggedPersonBT { get; set; }
        public static string? ReasonDT { get; set; }
        public static DateTime StartHandling { get; set; }

        public enum Status : int
        {
            Unconfirmed,
            SourceConfirmed,
            TargetConfirmed,
            AllConfirmed
        }

        public static readonly List<string> ReasonList = new List<string>
        {
            "Cykl niezgodny",
            "Niewłaściwy montaż - błędy operatora",
            "Problemy procesowe",
            "Niejasna instrukcja",
            "Wadliwy materiał",
            "Brak materiału",
            "Nauka",
            "Brak sieci",
            "Brak narzędzi",
            "Niewłaściwie przygotowany materiał na offline",
            "Brak materiału z poprzedniego procesu",
            "Nie działa poprawnie aplikacja",
            "Inne - należy dopisać wyjaśnienie"
        };

        private bool textVisibility;

        public bool TextVisibility
        {
            get { return textVisibility; }
            set
            {
                //if (value.Length == 11)
                //{
                //    _data.SerialNumber = value;
                //    StatusInfo = "Barcode OK";
                //}                   
                //else
                //{
                //    _data.SerialNumber = "";
                //    StatusInfo = "Zła ilość znaków";
                //}

                textVisibility = value;
                OnPropertyChanged(nameof(TextVisibility));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
