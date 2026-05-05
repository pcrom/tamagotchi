using System;
using System.Windows.Forms;

namespace HanaJotchi
{
    public static class Program
    {
        public static VPScreen VirtualPet;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            VirtualPet = new VPScreen();

            Application.Run(VirtualPet);
        }
    }
}
