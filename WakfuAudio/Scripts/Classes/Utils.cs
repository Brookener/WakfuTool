using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WakfuAudio.Scripts.Classes
{
    class Utils
    {
        public static bool StringContains(string s, string pattern, int index)
        {
            return s.IndexOf(pattern, index, StringComparison.OrdinalIgnoreCase) > -1;
        }
        public static bool StringContains(string s, string pattern)
        {
            return StringContains(s, pattern, 0);
        }
        public static string GetStringFromPaterns(string text, string startPatern, string endPatern)
        {
            var start = text.IndexOf(startPatern) + startPatern.Length;
            var end = text.IndexOf(endPatern, start);
            if (start == -1 || end == -1)
                return "";
            return text.Substring(start, end - start);
        }
        public static string GetStringFromPaterns(string text, string[] startPatern, string[] endPatern)
        {
            var start = startPatern.Select(x => text.IndexOf(x) + x.Length).Min();
            var end = endPatern.Select(x => text.IndexOf(x, start)).Min();
            if (start == -1 || end == -1)
                return "";
            return text.Substring(start, end - start);
        }
        public static string ReplaceBetweenPaterns(string text, string newText, string startPatern, string endPatern)
        {
            var start = text.IndexOf(startPatern) + startPatern.Length;
            var end = text.IndexOf(endPatern, start);
            text = text.Remove(start, end - start);
            text = text.Insert(start, newText);
            return text;
        }
        public static string CompleteWithZeros(string s, int total)
        {
            while (s.Length < total)
                s = s.Insert(0, "0");
            return s;
        }

        public static System.Drawing.Image GetImage(string url)
        {
            WebRequest request = WebRequest.Create(url);

            try
            {
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(responseStream);
                responseStream.Dispose();
                return bmp;
            }
            catch
            {
                return null;
            }


        }
        public static ImageSource ConvertImage(System.Drawing.Image image)
        {
            try
            {
                if (image != null)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    MemoryStream memoryStream = new System.IO.MemoryStream();
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch { }
            return null;
        }
        public static ImageSource GetSourceImage(string url)
        {
            return ConvertImage(GetImage(url));
        }

        public static bool FileInUse(string file)
        {
            try
            {
                using (FileStream stream = new FileInfo(file).Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }
        
    }
}
