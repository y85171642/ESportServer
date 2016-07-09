using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Threading;

namespace Esports.space
{
    public static class LOG
    {
        static string output_path = System.AppDomain.CurrentDomain.BaseDirectory + "/log.txt";
        private static object lock_obj = new object();
        public static void Out(string text)
        {
            lock (lock_obj)
            {
                text += "\n";
                if (!File.Exists(output_path))
                    File.Create(output_path).Close();
                StreamWriter sw = new StreamWriter(output_path, true, System.Text.Encoding.UTF8);
                sw.WriteLine(text);
                sw.Close();
            }
        }
    }
}