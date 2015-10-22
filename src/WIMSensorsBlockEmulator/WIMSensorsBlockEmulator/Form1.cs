using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;

namespace WIMSensorsBlockEmulator
{
    public partial class Form1 : Form
    {
        private System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        private bool status = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            host.Text = "127.0.0.1";
            port.Text = "7777";

        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (!status)
            {
                IPAddress hostValue;
                if (IPAddress.TryParse(host.Text, out hostValue))
                {
                    string portText = port.Text;

                    int portValue = 0;
                    if (Int32.TryParse(portText, out portValue))
                    {
                        startClient(hostValue, portValue);
                    }
                    else
                    {
                        msg("Invalid port");
                    }
                }
                else
                {
                    msg("Invalid host ip address");
                }
            }
            else
            {
                status = false;
                stopClient();
            }
        }

        private void sendData()
        {
            String data = "testData";

            NetworkStream serverStream = clientSocket.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(data + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }

        private void startClient(IPAddress host, int port)
        {
            msg("Client started. Host: " + host + " port: " + port + ". Try to connect...");

            try
            {
                clientSocket.Connect(host, port);

                startBtn.Text = "Stop";
                status = true;
                statusLbl.Text = "Status: started";

                Thread myThread = new Thread(startTread);
                myThread.Start();
                    
                
                }
            catch (SocketException r)
            {
                msg("Connection Error: "+r.Message);

                clientSocket.Close();
                stopClient();
            }
        }

        private void startTread()
        {
            this.Invoke(new Action(() => { msg("start socet client thread"); })); 
            while (status)
            {
                this.Invoke(new Action(() => { msg("send data "); }));       
                //TODO: do send data
                    
                //random sleep
                Random rnd = new Random();
                Thread.Sleep(rnd.Next(2000, 10000));    
            }
            this.Invoke(new Action(() => { msg("stop socet client thread" + Environment.NewLine); })); 
        }

        private void stopClient()
        {
            startBtn.Text = "Start";
            
            msg("Client stoped");

            statusLbl.Text = "Status: stoped";
           
        }
       
        public void msg(string msg)
        {
            DateTime d = DateTime.Now;
            log.Text += Environment.NewLine +d.ToLongTimeString() + "." + d.Millisecond + " >> " + msg;
            log.SelectionStart = log.TextLength;
            log.ScrollToCaret();
        }     
    }
}
