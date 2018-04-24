using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ROBO
{
    static class Program
    {
        public static int contador = 3;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
