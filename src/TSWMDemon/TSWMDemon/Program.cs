using System;
using System.Threading;
using System.Windows.Forms;
using TSWMRepository;

namespace TSWMDemon
{

    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            IRepository rep = Repository.Instance;

            rep.WriteLine();
            
            Demon demon = new Demon();

            Thread demonThread = new Thread(() => demon.start());

            //demonThread.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(demonThread, demon));
        }

    }
    
}