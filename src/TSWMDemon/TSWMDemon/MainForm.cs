using System;
using System.Threading;
using System.Windows.Forms;

namespace TSWMDemon
{
    public partial class MainForm : Form
    {
        private Thread demonThread;
        private Demon demon;

        public MainForm(Thread demonThread, Demon demon)
        {
            InitializeComponent();
            this.demonThread = demonThread;
            this.demon = demon;

            
            
            System.Console.WriteLine("MainForm constructor " + demonThread.ManagedThreadId);

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
           
            //this.demonThread.Start();
            //while (!this.demon.Enabled) { System.Console.Write("."); Thread.Sleep(100); }
            //System.Console.Write("\n Demon thread started");
            this.label1.Text = "Endpoint: " + this.demon.BSEndPoint;
            //this.button1.Text = "started";
            
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Console.WriteLine("MainForm_FormClosing. Abort demon bg thread...");
            demonThread.Abort();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!this.demon.Enabled)
            {
                demonThread = new Thread(() => demon.start());
                this.button1.Text = "started";
            }
            else
            {
                this.demon.stop();
                this.button1.Text = "stopped";
            }

        }

        private String checkResp(byte[] resp)
        {
            string msg;
            if (resp != null)
            {
                msg = "response size = " + resp.Length + ", data: " + BitConverter.ToString(resp);
            }
            else
            {
                msg = "no response";
            }
            return msg;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] resp = demon.Ping();
            this.label2.Text = checkResp(resp);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] resp = this.demon.SetParams(0x04, 0x01, 0x18, 0x00);
            this.label3.Text = checkResp(resp);
        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.label2.Text = "...";
        }

        private void label3_Click(object sender, EventArgs e)
        {
            this.label3.Text = "...";
        }
    }
}
