using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
namespace WebAccessLog
{
    class File
    {
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
            a = a.Substring(a.IndexOf('.')+1, a.Length - a.IndexOf('.') - 1);
            if (new string[] { "html", "htm", "shtml", "map" }.Contains(a)) return "HTML";
            else if (new string[] { "gif", "jpeg", "jpg", "xbm", "bmp", "rgb", "xpm" }.Contains(a)) return "Images";
            else if (new string[] { "au", "snd", "wav", "mid", "midi", "lha", "aif", "aiff" }.Contains(a)) return "Sound";
            else if (new string[] { "mov", "movie", "avi", "qt", "mpeg", "mpg" }.Contains(a)) return "Video";
            else if (new string[] { "ps", "eps", "doc", "dvi", "txt" }.Contains(a)) return "Formatted";
            else if (new string[] { "cgi", "pl", "cgi-bin" }.Contains(a)) return "Dynamic";
            else return "Others";
        }
        static void Main(string[] args)
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
            foreach (String line in System.IO.File.ReadLines(args[0]))
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
                    dayOfTheWeek[(int) oDate.DayOfWeek] = dayOfTheWeek[(int) oDate.DayOfWeek] + 1;
                    monthOfTheYear[oDate.Month - 1] = monthOfTheYear[oDate.Month - 1] + 1;
                }
            }
            Console.WriteLine("Requests made per day on average:");
            Console.WriteLine("{0:N2}\n", request_count / 353);
            Console.WriteLine("Bytes transferred during the entire log duration:");
            Console.WriteLine("{0:N2} MB\n", totalBytes / 1000000);
            Console.WriteLine("Average number of bytes transferred per day:");
            Console.WriteLine("{0:N2} MB\n", totalBytes / 1000000 * 353);

            Console.WriteLine("Breakdown of server response codes:");
            for (int i = 0; i < 4; i++) Console.WriteLine("{0}: {1:N2}%", responseLabels[i], 100 * responses[i] / totalCount);

            Console.WriteLine("Breakdown of requests by each client category:");
            Console.WriteLine("Local: {0:N2}%", 100 * local_count / totalCount);
            Console.WriteLine("Remote: {0:N2}%\n", 100 * remote_count / totalCount);
            Console.WriteLine("Breakdown of bytes transferred by each client category:");
            Console.WriteLine("Local: {0:N2}%", 100 * local_byte / totalBytes);
            Console.WriteLine("Remote: {0:N2}%\n", 100 * remote_byte / totalBytes);

            Console.WriteLine("Breakdown of requests by file type category:");
            for (int i = 0; i < 7; i++) Console.WriteLine("{0}: {1:N2}%", typeLabels[i], 100 * typeCount[i] / totalCount);
            Console.WriteLine("\nBreakdown of bytes transferred by each file category:");
            for (int i = 0; i < 7; i++) Console.WriteLine("{0}: {1:N2}%", typeLabels[i], 100 * typeBytes[i] / totalBytes);
            Console.WriteLine("\nAverage transfer sizes of each file category:");
            for (int i = 0; i < 7; i++) Console.WriteLine("{0}: {1:N2} B", typeLabels[i], typeBytes[i] / typeCount[i]);

            List<ResultLine> result = object_list
                .GroupBy(l => l.Item1)
                .OrderBy(x => x.Count())
                .Select(cl => new ResultLine
                {
                    k = cl.First().Item1,
                    t = cl.Count().ToString(),
                    v = (cl.Sum(x => x.Item2)/cl.Count())
                }).ToList();

            Console.WriteLine("\nPercentage of unique objects and bytes in the log:");
            Console.WriteLine("Unique Objects: {0:N2}%", 100 * result.Where(x => x.t == "1").Count() / result.Count());
            Console.WriteLine("Unique Bytes: {0:N2}%", 100 * result.Where(x => x.t == "1").Sum(x => x.v) / result.Sum(x => x.v));

            Console.WriteLine("\nNumber of objects accessed by hour of the day");
            for (int i = 0; i < 24; i++) Console.WriteLine("{0}: {1}", i, hourOfTheDay[i]);
            Console.WriteLine("\nNumber of objects accessed by day of the week");
            for (int i = 0; i < 7; i++) Console.WriteLine("{0}: {1}", CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName((DayOfWeek) i), dayOfTheWeek[i]);
            Console.WriteLine("\nNumber of objects accessed by month of the year");
            for (int i = 0; i < 12; i++) Console.WriteLine("{0}: {1}", CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i+1), monthOfTheYear[i]);
        }
    }
}