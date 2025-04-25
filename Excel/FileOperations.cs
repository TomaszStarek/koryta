using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wiring
{
    public static class FileOperations
    {


        public static int WriteListStatusToFile(int index, List<Wire> list, double czas)
        {
           // index = System.Text.RegularExpressions.Regex.Replace(index, @"\s+", string.Empty);

            // textBox1.Text = sn;

            string sciezka = (@"memory.txt");      //definiowanieścieżki do której zapisywane logi
            //var date = DateTime.Now;
            //if (Directory.Exists(sciezka))       //sprawdzanie czy sciezka istnieje
            //{
            //    ;
            //}
            //else
            //    System.IO.Directory.CreateDirectory(sciezka); //jeśli nie to ją tworzy

            try
            {
                using (StreamWriter sw = new StreamWriter(sciezka))
                {
                    sw.WriteLine(Data.SetNumber);
                    sw.WriteLine(index);
                    foreach (var item in list)
                    {
                        var time = Math.Round(item.Seconds + item.SecondsDestination + item.SecondsSource + item.HandlingTime,1);
                        sw.WriteLine($"{item.WireStatus};{time};{item.DateOfFinish}; {item.MadeBy}") ;
                    }

                }
                File.Copy(sciezka, @$"{list[0].NameOfCabinet}_{Data.SetNumber}",true);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }

            return 1;

        }



        public static int ReadMemory(ref int index, List<List<Wire>> list, string path)
        {
            bool IsItParseSuccess, IsItParseSuccess2, IsItParseSuccess3;
            string sciezka = (path);
            int i = 0;
            try
            {
                using (StreamReader sr = new StreamReader(sciezka))
                {
                    //   ListOfWarnings.Clear();
                    double countOfProgress = 0;

                    while (sr.Peek() >= 0)
                    {
                        if(i==0)
                        {
                            Data.SetNumber = sr.ReadLine();
                        }
                        else
                        {
                            int parsedNumber = 0;
                            double parsedSeconds = 0.0;
                            DateTime parsedDateTime = DateTime.Now;
                            
                            var data = sr.ReadLine();
                            string[] splitted = {"","","", ""};
                            
                            if (data != null)
                                splitted = data.Split(";");
                            string MadeBy = "";

                            if (splitted.Length > 3)
                                MadeBy = splitted[3];

                            IsItParseSuccess = false;
                            IsItParseSuccess2 = false;
                            IsItParseSuccess3 = false;

                            IsItParseSuccess = int.TryParse(splitted[0], out parsedNumber);
                            if(splitted.Length > 1)
                                IsItParseSuccess2 = double.TryParse(splitted[1], out parsedSeconds);
                            if(splitted.Length > 2)
                                IsItParseSuccess3 = DateTime.TryParse(splitted[2], out parsedDateTime);


                            if (IsItParseSuccess)
                            {
                                if (i == 1)
                                {
                                    index = parsedNumber;
                                }

                                else if (i != 0)
                                {                                  
                                    list[index][i - 2].WireStatus = parsedNumber;
                                    if (IsItParseSuccess2)
                                        list[index][i - 2].Seconds = parsedSeconds;
                                    if (IsItParseSuccess3)
                                        list[index][i - 2].DateOfFinish = parsedDateTime;
                                    list[index][i - 2].MadeBy = MadeBy;
                                    if (parsedNumber == 1 || parsedNumber == 2)
                                        countOfProgress += 1;
                                    else if (parsedNumber == 3)
                                        countOfProgress += 2;
                                }

                            }
                            else
                                MessageBox.Show("Parse Error", "Błąd odczytu pamięci!");
                        }
                        i++;


                    }
                    sr.Close();

                    var index2 = index;
                    list[index2].ForEach(x => x.Progress = Math.Round(  (countOfProgress / (list[index2].Count * 2) * 100), 2));
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                //  ListOfScannedBarcodes.Clear();
                return 0;
            }
            return i;
        }
        public static void SaveSingleLog(string NameOfCabinet, Wire wire)
        {
            try
            {

                //  string sciezka = $"{AppDomain.CurrentDomain.BaseDirectory}/logi/{NameOfCabinet}/{Data.SetNumber}/";      //definiowanieścieżki do której zapisywane logi
                //string sciezka = @$"\\KWIPUBV04\General$\Enercon\Shared\LOGI ENERCON\LOGI_SW_SM\logi\{NameOfCabinet}\{Data.SetNumber}\";
                string sciezka = @$"\\KWIPUBV01\Procesy$\Enercon\Wiring\LOGI_SW_SM\logi\{NameOfCabinet}\{Data.SetNumber}\";
                DateTime stop = DateTime.Now;
                if (Directory.Exists(sciezka))       //sprawdzanie czy sciezka istnieje
                {
                    ;
                }
                else
                    System.IO.Directory.CreateDirectory(sciezka); //jeśli nie to ją tworzy


                using (StreamWriter sw = new StreamWriter($"{sciezka}log.txt", true))
                {
                    var computerName = System.Environment.MachineName.ToUpper();

                    sw.WriteLine($"W;{computerName};{wire.Start.ToString("yyyy-MM-dd HH:mm:ss")};status:{wire.WireStatus};numer:{wire.Number};{wire.DtSource} <> {wire.DtTarget};czash:{Math.Round(wire.HandlingTime,2)};czasn:{Math.Round(wire.Seconds,2)}s;data_zakonczenia:{wire.DateOfFinish.ToString("yyyy-MM-dd HH:mm:ss")};{wire.MadeBy};{wire.Progress}%");

                }
            }
            catch (IOException iox)
            {
                MessageBox.Show(iox.Message);
            }
        }

        public static void SaveLog(string NameOfCabinet, List<Wire> list)
        {
            try
            {
                string sciezka = "C:/tars/";      //definiowanieścieżki do której zapisywane logi
                DateTime stop = DateTime.Now;
                if (Directory.Exists(sciezka))       //sprawdzanie czy sciezka istnieje
                {
                    ;
                }
                else
                    System.IO.Directory.CreateDirectory(sciezka); //jeśli nie to ją tworzy


                using (StreamWriter sw = new StreamWriter("C:/tars/" + Data.SetNumber + "-" + NameOfCabinet + "-" + "(" + stop.Day + "-" + stop.Month + "-" + stop.Year + " " + stop.Hour + "-" + stop.Minute + "-" + stop.Second + ")" + ".Tars"))
                {
                    var computerName = System.Environment.MachineName.ToUpper();

                    //   sw.WriteLine($"S{serial}");
                    sw.WriteLine($"Numer seta:{Data.SetNumber}");
                    sw.WriteLine($"Szafa:{NameOfCabinet}");
                    sw.WriteLine($"N{computerName}");
                    foreach (var item in list)
                    {
                        sw.WriteLine($"{item.Number};{item.DtSource} <> {item.DtTarget};{item.Seconds};{item.DateOfFinish};{item.MadeBy}");
                    }
                    sw.WriteLine("[" + stop.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }
            catch (IOException iox)
            {
                MessageBox.Show(iox.Message);
            }
        }
        public static void SaveComment(string NameOfCabinet, Wire wire)
        {
            try
            {
                //           \\KWIPUBV04\Procesy$\Enercon\Wiring\LOGI_SW_SM
                //    string sciezka = $"{AppDomain.CurrentDomain.BaseDirectory}/komentarze/{NameOfCabinet}/{Data.SetNumber}/";      //definiowanieścieżki do której zapisywane logi
                //string sciezka = @$"\\KWIPUBV04\General$\Enercon\Shared\LOGI ENERCON\UWAGI_PROD\{NameOfCabinet}\{Data.SetNumber}\";
                string sciezka = @$"\\KWIPUBV01\Procesy$\Enercon\Wiring\UWAGI_PROD\{NameOfCabinet}\{Data.SetNumber}\";
                DateTime stop = DateTime.Now;
                if (Directory.Exists(sciezka))       //sprawdzanie czy sciezka istnieje
                {
                    ;
                }
                else
                    System.IO.Directory.CreateDirectory(sciezka); //jeśli nie to ją tworzy


                using (StreamWriter sw = new StreamWriter($"{sciezka}uwagi.txt", true))
                {
                    var computerName = System.Environment.MachineName.ToUpper();

                    sw.WriteLine($"W;data_zakonczenia:{stop.ToString("yyyy-MM-dd HH:mm:ss")};{wire.MadeBy};{computerName};numer:{wire.Number};{wire.DtSource} <> {wire.DtTarget};komentarz:{wire.Addnotations}");

                }
            }
            catch (IOException iox)
            {
                MessageBox.Show(iox.Message);
            }
        }
        //private static readonly string Password = "TwojeSilneHaslo123"; // Ustaw hasło do szyfrowania
        //private static readonly byte[] Salt = Encoding.UTF8.GetBytes("UnikalnySalt1234"); // Salt do zabezpieczenia klucza

        //public static void SaveSingleLogCrypto(string NameOfCabinet, Wire wire)
        //{
        //    try
        //    {
        //        string sciezka = @$"\\KWIPUBV01\Procesy$\Enercon\Wiring\LOGI_SW_SM\logi\{NameOfCabinet}\{Data.SetNumber}\";
        //        DateTime stop = DateTime.Now;

        //        if (!Directory.Exists(sciezka))
        //            Directory.CreateDirectory(sciezka); // Tworzy folder, jeśli nie istnieje

        //        string plik = $"{sciezka}log.txt";
        //        string plik2 = $"{sciezka}log2.txt";

        //        // Tworzenie zawartości logu
        //        var computerName = Environment.MachineName.ToUpper();
        //        string log = $"W;{computerName};{stop:yyyy-MM-dd HH:mm:ss};status:{wire.WireStatus};numer:{wire.Number};{wire.DtSource} <> {wire.DtTarget};czas:{wire.Seconds};data_zakonczenia:{wire.DateOfFinish};{wire.MadeBy}";

        //        // Zapis zaszyfrowanego logu
        //        EncryptAndWriteToFile(plik, log);

        //        DecryptAndSaveToFile(plik, plik2);
        //    }
        //    catch (IOException iox)
        //    {
        //        MessageBox.Show(iox.Message);
        //    }
        //}

        //private static void EncryptAndWriteToFile(string filePath, string data)
        //{
        //    using (Aes aes = Aes.Create())
        //    {
        //        var key = DeriveKeyFromPassword(Password, Salt);

        //        aes.Key = key;
        //        aes.IV = GenerateIV(aes.BlockSize);

        //        using (FileStream fileStream = new FileStream(filePath, FileMode.Append))
        //        {
        //            // Zapisuje IV na początku pliku (jednorazowy blok)
        //            if (fileStream.Length == 0)
        //            {
        //                fileStream.Write(aes.IV, 0, aes.IV.Length);
        //            }

        //            using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
        //            using (StreamWriter sw = new StreamWriter(cryptoStream))
        //            {
        //                sw.WriteLine(data);
        //            }
        //        }
        //    }
        //}

        //private static byte[] DeriveKeyFromPassword(string password, byte[] salt)
        //{
        //    using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, 10000))
        //    {
        //        return rfc2898.GetBytes(32); // 256-bitowy klucz dla AES
        //    }
        //}

        //private static byte[] GenerateIV(int blockSize)
        //{
        //    byte[] iv = new byte[blockSize / 8];
        //    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        //    {
        //        rng.GetBytes(iv);
        //    }
        //    return iv;
        //}
        ////public static void DecryptAndSaveToFile(string encryptedFilePath, string outputFilePath)
        //{
        //    try
        //    {
        //        using (Aes aes = Aes.Create())
        //        {
        //            var key = DeriveKeyFromPassword(Password, Salt);

        //            using (FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Open))
        //            {
        //                // Odczytujemy IV z początku zaszyfrowanego pliku
        //                byte[] iv = new byte[16];
        //                int bytesRead = encryptedFileStream.Read(iv, 0, iv.Length);

        //                if (bytesRead != iv.Length)
        //                {
        //                    throw new CryptographicException("Nie udało się odczytać wektora IV z pliku.");
        //                }

        //                aes.Key = key;
        //                aes.IV = iv;

        //                // Tworzymy strumień do odszyfrowywania
        //                using (CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
        //                using (StreamReader sr = new StreamReader(cryptoStream))
        //                {
        //                    // Odszyfrowujemy całą zawartość pliku
        //                    string decryptedContent = sr.ReadToEnd();

        //                    // Zapisujemy odszyfrowaną zawartość do pliku log2.txt
        //                    File.WriteAllText(outputFilePath, decryptedContent);
        //                }
        //            }
        //        }
        //    }
        //    catch (CryptographicException ce)
        //    {
        //        MessageBox.Show($"Błąd podczas odszyfrowywania pliku: {ce.Message}");
        //    }
        //    catch (IOException iox)
        //    {
        //        MessageBox.Show($"Błąd dostępu do pliku: {iox.Message}");
        //    }
        //}


    }
}
