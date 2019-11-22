using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static string textOutput = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        public class ResultLine
        {
            public ResultLine() { }
            public string k { get; set; }
            public string t { get; set; }
            public double v { get; set; }
        }
        static string getFileType(string a)
        {
            if (a.IndexOf('.') == -1) return "Others";
            a = a.Substring(a.IndexOf('.') + 1, a.Length - a.IndexOf('.') - 1);
            if (new string[] { "html", "htm", "shtml", "map" }.Contains(a)) return "HTML";
            else if (new string[] { "gif", "jpeg", "jpg", "xbm", "bmp", "rgb", "xpm" }.Contains(a)) return "Images";
            else if (new string[] { "au", "snd", "wav", "mid", "midi", "lha", "aif", "aiff" }.Contains(a)) return "Sound";
            else if (new string[] { "mov", "movie", "avi", "qt", "mpeg", "mpg" }.Contains(a)) return "Video";
            else if (new string[] { "ps", "eps", "doc", "dvi", "txt" }.Contains(a)) return "Formatted";
            else if (new string[] { "cgi", "pl", "cgi-bin" }.Contains(a)) return "Dynamic";
            else return "Others";
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) Compute(openFileDialog.FileName);
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            string filename;
            var sfd = new SaveFileDialog { Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*", };
            if (sfd.ShowDialog() == true)
            {
                filename = sfd.FileName;
                File.WriteAllText(filename, textOutput);
                MessageBox.Show(String.Format("Saved to {0}", filename));

            }

        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }




























        static void Compute(String filename)
        {
            double[] responses = { 0, 0, 0, 0 };
            string[] responseLabels = { "Successful", "Not Modified", "Found", "Unsuccessful" };
            double request_count = 0;
            double[] typeCount = { 0, 0, 0, 0, 0, 0, 0 };
            double[] typeBytes = { 0, 0, 0, 0, 0, 0, 0 };
            string[] typeLabels = { "HTML", "Images", "Sound", "Video", "Formatted", "Dynamic", "Others" };
            double local_count = 0;
            double local_byte = 0;
            double remote_count = 0;
            double remote_byte = 0;
            double totalCount = 0;
            double totalBytes = 0;
            double[] hourOfTheDay = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] dayOfTheWeek = { 0, 0, 0, 0, 0, 0, 0 };
            double[] monthOfTheYear = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            List<Tuple<string, double>> object_list = new List<Tuple<string, double>>();
            foreach (String line in System.IO.File.ReadLines(filename))
            {
                List<String> elements = line.Split(' ').ToList();
                if (line != "" && elements.Count <= 11 && elements.Count >= 9)
                {
                    request_count++;
                    if (elements.Count == 9 && elements[2] != "-") elements.Insert(2, "-");
                    string sourceAddress = elements[0];
                    string timeStr = elements[3].Replace("[", string.Empty);
                    string requestMethod = elements[5];
                    string requestFileName = elements[6].Replace("\"", string.Empty);
                    string responseCode = elements[elements.Count - 2];
                    string replySizeInBytes = elements[elements.Count - 1];
                    string fileType = getFileType(requestFileName);

                    if (responseCode == "200") responses[0]++;
                    else if (responseCode == "302") responses[2]++;
                    else if (responseCode == "304") responses[1]++;
                    else responses[3]++;
                    if (replySizeInBytes == "-") continue;

                    if (sourceAddress == "local")
                    {
                        local_count++;
                        local_byte += Int32.Parse(replySizeInBytes);
                    }
                    else if (sourceAddress == "remote")
                    {
                        remote_count++;
                        remote_byte += Int32.Parse(replySizeInBytes);
                    }

                    if (fileType == "HTML")
                    {
                        typeCount[0]++;
                        typeBytes[0] += Int32.Parse(replySizeInBytes);
                    }
                    else if (fileType == "Images")
                    {
                        typeCount[1]++;
                        typeBytes[1] += Int32.Parse(replySizeInBytes);
                    }
                    else if (fileType == "Sound")
                    {
                        typeCount[2]++;
                        typeBytes[2] += Int32.Parse(replySizeInBytes);
                    }
                    else if (fileType == "Video")
                    {
                        typeCount[3]++;
                        typeBytes[3] += Int32.Parse(replySizeInBytes);
                    }
                    else if (fileType == "Formatted")
                    {
                        typeCount[4]++;
                        typeBytes[4] += Int32.Parse(replySizeInBytes);
                    }
                    else if (fileType == "Dynamic")
                    {
                        typeCount[5]++;
                        typeBytes[5] += Int32.Parse(replySizeInBytes);
                    }
                    else if (fileType == "Others")
                    {
                        typeCount[6]++;
                        typeBytes[6] += Int32.Parse(replySizeInBytes);
                    }

                    totalCount++;
                    totalBytes += Convert.ToInt64(replySizeInBytes);
                    object_list.Add(new Tuple<string, double>(requestFileName, Convert.ToInt64(replySizeInBytes)));

                    DateTime oDate = DateTime.ParseExact(timeStr, "dd/MMM/yyyy:HH:mm:ss", null);
                    hourOfTheDay[oDate.Hour] = hourOfTheDay[oDate.Hour] + 1;
                    dayOfTheWeek[(int)oDate.DayOfWeek] = dayOfTheWeek[(int)oDate.DayOfWeek] + 1;
                    monthOfTheYear[oDate.Month - 1] = monthOfTheYear[oDate.Month - 1] + 1;
                }
            }
            textOutput += String.Format("Requests made per day on average:\n");
            textOutput += String.Format("{0:N2}\n\n", request_count / 353);
            textOutput += String.Format("Bytes transferred during the entire log duration:\n");
            textOutput += String.Format("{0:N2} MB\n\n", totalBytes / 1000000);
            textOutput += String.Format("Average number of bytes transferred per day:\n");
            textOutput += String.Format("{0:N2} MB\n\n", totalBytes / 1000000 * 353);

            textOutput += String.Format("Breakdown of server response codes:\n");
            for (int i = 0; i < 4; i++) textOutput += String.Format("{0}: {1:N2}%\n", responseLabels[i], 100 * responses[i] / totalCount);

            textOutput += String.Format("\nBreakdown of requests by each client category:\n");
            textOutput += String.Format("Local: {0:N2}%\n", 100 * local_count / totalCount);
            textOutput += String.Format("Remote: {0:N2}%\n\n", 100 * remote_count / totalCount);
            textOutput += String.Format("Breakdown of bytes transferred by each client category:\n");
            textOutput += String.Format("Local: {0:N2}%\n", 100 * local_byte / totalBytes);
            textOutput += String.Format("Remote: {0:N2}%\n\n", 100 * remote_byte / totalBytes);

            textOutput += String.Format("Breakdown of requests by file type category:\n");
            for (int i = 0; i < 7; i++) textOutput += String.Format("{0}: {1:N2}%\n", typeLabels[i], 100 * typeCount[i] / totalCount);
            textOutput += String.Format("\nBreakdown of bytes transferred by each file category:\n");
            for (int i = 0; i < 7; i++) textOutput += String.Format("{0}: {1:N2}%\n", typeLabels[i], 100 * typeBytes[i] / totalBytes);
            textOutput += String.Format("\nAverage transfer sizes of each file category:\n");
            for (int i = 0; i < 7; i++) textOutput += String.Format("{0}: {1:N2} B\n", typeLabels[i], typeBytes[i] / typeCount[i]);

            List<ResultLine> result = object_list
                .GroupBy(l => l.Item1)
                .OrderBy(x => x.Count())
                .Select(cl => new ResultLine
                {
                    k = cl.First().Item1,
                    t = cl.Count().ToString(),
                    v = (cl.Sum(x => x.Item2) / cl.Count())
                }).ToList();

            textOutput += String.Format("\nPercentage of unique objects and bytes in the log:\n");
            textOutput += String.Format("Unique Objects: {0:N2}%\n", 100 * result.Where(x => x.t == "1").Count() / result.Count());
            textOutput += String.Format("Unique Bytes: {0:N2}%\n", 100 * result.Where(x => x.t == "1").Sum(x => x.v) / result.Sum(x => x.v));

            textOutput += String.Format("\nNumber of objects accessed by hour of the day\n");
            for (int i = 0; i < 24; i++) textOutput += String.Format("{0}: {1}\n", i, hourOfTheDay[i]);
            textOutput += String.Format("\nNumber of objects accessed by day of the week\n");
            for (int i = 0; i < 7; i++) textOutput += String.Format("{0}: {1}\n", CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName((DayOfWeek)i), dayOfTheWeek[i]);
            textOutput += String.Format("\nNumber of objects accessed by month of the year\n");
            for (int i = 0; i < 12; i++) textOutput += String.Format("{0}: {1}\n", CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i + 1), monthOfTheYear[i]);
            MessageBox.Show(String.Format("Analysis of {0} complete!\n", filename));
        }





    }
}
