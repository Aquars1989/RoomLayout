using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RoomLayout
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]) && Path.GetExtension(args[0]).ToLower() == ".rld")
            {
                Global.FilePath = args[0];
            }
            Application.Run(new MainForm());
        }
    }
}
