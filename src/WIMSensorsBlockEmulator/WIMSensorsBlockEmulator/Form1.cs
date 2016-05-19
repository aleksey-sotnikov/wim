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
using System.IO;

namespace WIMSensorsBlockEmulator
{

    enum BSFlag
    {
        GET_DATA = 0x01,
        SYNC_TIME = 0x02
    }

    public partial class Form1 : Form
    {
        public static readonly string FILE_NAME = "d:\\!PROJECTS\\WIM\\AUTO\\DATA\\Kisler_1_198.wix";

        // Создаем сокет Tcp/Ip
        private Socket sListener;
        private Thread socetThread;
        private bool status = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            host.Text = "127.0.0.1";
            port.Text = "7654";

            
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
/*
        private void sendData()
        {
            String data = "testData";

            NetworkStream serverStream = clientSocket.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(data + "$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }
*/
        private void startClient(IPAddress host, int port)
        {
            msg("Server starting. Host: " + host + " port: " + port + ".");

            IPEndPoint ipEndPoint = new IPEndPoint(host, port);

            // Создаем сокет Tcp/Ip
            sListener = new Socket(host.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sListener.Bind(ipEndPoint);
            sListener.Listen(10);


            startBtn.Text = "Stop";
                status = true;
                statusLbl.Text = "Status: started";

                socetThread = new Thread(startTread);
                socetThread.Start();
            
        }

        private void startTread()
        {
            this.Invoke(new Action(() => { msg("start socet client thread"); }));
        try
        {
              
                while (status)
            {
                this.Invoke(new Action(() => { msg("waiting for connect... "); }));
                Socket handler = sListener.Accept();

                // Дождались клиента, получаем данные
                string data = null;
                byte[] bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);

                    // Получение управляющего флага(?)
                    BSFlag flag = (BSFlag)bytes[0];

                    data += flag;//Encoding.Default.GetString(bytes, 0, bytesRec);

                    // Показываем данные
                    this.Invoke(new Action(() => { msg("Recived data: " + data + "\n"); }));

                    // Отправляем ответ клиенту
                    if(BSFlag.GET_DATA == flag) {
                        
                        byte[] response = Encoding.UTF8.GetBytes("serv resp");
                        //byte[] response = getPreparedData();
                        this.Invoke(new Action(() => { msg("Send data " + response.Length + " bytes ...\n"); }));

                        handler.Send(response);

                        this.Invoke(new Action(() => { msg("Data sent complete.\n"); }));
                    }

                   
                //

                /* Пример управления сервером с клента. 
                if (data.IndexOf("<TheEnd>") > -1)
                {
                    Console.WriteLine("Сервер завершил соединение с клиентом.");
                    break;
                }
                */

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
        }
        catch (SocketException r)
        {
                this.Invoke(new Action(() => { msg("Connection Error: " + r.Message);  }));
                //this.Invoke(new Action(() => { msg("stop socet client thread" + Environment.NewLine); }));
                if (sListener != null)
                sListener.Close();
            stopClient();
        }
            this.Invoke(new Action(() => { msg("stop socet client thread" + Environment.NewLine); }));
            }

        private byte[] getPreparedData()
        {
            byte[] fileBytes = File.ReadAllBytes(FILE_NAME);
            
            return fileBytes;
        }

        private void stopClient()
        {
            if (sListener != null)
                sListener.Close(0);

            //if (socetThread != null)
                //socetThread.Join();

            startBtn.Text = "Start";

            this.Invoke(new Action(() =>
            {
                msg("Client stoped");
            }));

            statusLbl.Text = "Status: stoped";
           
        }
       
        public void msg(string msg)
        {
            DateTime d = DateTime.Now;
            log.Text += Environment.NewLine +d.ToLongTimeString() + "." + d.Millisecond + " >> " + msg;
            log.SelectionStart = log.TextLength;
            log.ScrollToCaret();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(log.Lines.Length > 0)
                log.Text = log.Lines[log.Lines.Length - 1];
        }
    }
}


/*** Client socket version ***

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


*** Client socket version ***/
