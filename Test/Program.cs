using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace Test
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            StringBuilder commandLine = new StringBuilder();
            foreach (string arg in args)
            {
                if (commandLine.Length != 0)
                {
                    commandLine.Append(" ");
                }
                commandLine.Append(arg);
            }

            string option = "c";
            string param = null;

            Regex commandLineRegex = new Regex(@"[-/]([a-zA-Z]*):? *(\d*)");
            Match match = commandLineRegex.Match(commandLine.ToString());
            if (match.Success)
            {
                if (match.Groups.Count > 1)
                {
                    option = match.Groups[1].Value;
                }
                if (match.Groups.Count > 2)
                {
                    param = match.Groups[2].Value;
                }
            }

            option = option.ToLower();

            if (option == "c")
            {
                ShowConfiguration(param);
            }
            else if (option == "p")
            {
                ShowPreview(param);
            }
            else if (option == "s")
            {
                ShowMainDisplay();
            }
            else if (option == "v")
            {
                ShowVersion();
            }
            else
            {
                MessageBox.Show(string.Format("Unexpected option [{0}], expected c, p, s or v command line [{1}].", option, commandLine));
                return;
            }

        }

        private static void ShowMainDisplay()
        {
            var bounds = Screen.AllScreens.Select(s => s.Bounds).Aggregate((a, b) => Rectangle.Union(a, b));
            MainDisplay mainDisplay = new MainDisplay(bounds);
            Application.Run(mainDisplay);
        }

        private static void ShowPreview(string param)
        {
            IntPtr handle = new IntPtr(long.Parse(param));
            MainDisplay mainDisplay = new MainDisplay(handle);
            Application.Run(mainDisplay);
        }

        private static void ShowConfiguration(string param)
        {
            MessageBox.Show(param);
            ConfigurationDisplay configurationDisplay = new ConfigurationDisplay();
            Application.Run(configurationDisplay);
        }

        private static void ShowVersion()
        {
            MessageBox.Show(string.Format("Version [{0}]", Application.ProductVersion));
        }
    }
}
