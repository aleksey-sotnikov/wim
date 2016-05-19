using System;
using System.Threading;
using System.Windows.Forms;

namespace TSWMDemon
{
    public partial class MainForm : Form
    {
        private Thread demon;

        public MainForm(Thread demon)
        {
            InitializeComponent();
            this.demon = demon;
            System.Console.WriteLine("MainForm constructor " + demon.ManagedThreadId);
        }

        private void Form1_FormClosing(object sender, EventArgs e)
        {
            demon.Join();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.demon.Start();
            System.Console.WriteLine("MainForm_Load");
        }
    }
}
