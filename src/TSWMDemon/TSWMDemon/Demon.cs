﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TSWMRepository;
using TSWMRepository.domain;

namespace TSWMDemon
{
    enum BSFlag
    {
        // Флаг запроса готовых данных
        GET_DATA = 0x01,
        // Флаг запроса синхронизации времени блока сенсоров с сервером времени
        SYNC_TIME = 0x02
    }

    /// <summary>
    /// Основной поток, который получает данные от блока датчиков.
    /// </summary>
    class Demon
    {
        //статус подключения к блоку сенсоров(?)
        private bool enabled = false;

        //константа названия модуля для логгера
        private static readonly string MODULE = "Demon";
        //название ключа IP-адреса блока сенсоров в файле настроек
        private static readonly string KEY_SBHOST = "SBhost";
        //название ключа порта блока сенсоров в файле настроек
        private static readonly string KEY_SBPORT = "SBport";
        //название ключа интервала опроса блока сенсоров в файле настроек
        private static readonly string KEY_INTERVAL = "SBinterval";
        //интервал опроса блока сенсоров
        private int INTERVAL = Convert.ToInt32(ConfigurationManager.AppSettings[KEY_INTERVAL]);

        //private System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        
        // запуск демона
        public void start()
        {
            Logger.Info("Start Demon...", MODULE);
            
            // создаение соеднинения с репозиторем хранения данных(?)
            IRepository rep = Repository.Instance;

            //Получение конечнной точки блока сенсоров
            EndPoint bsEndPoint = prepareBSEndPoint();

            enabled = true;

            // цикл получения данных от блока сенсоров (?)
            while (enabled)
            {
                // соединение с блоком сенсоров
                Socket socket = startClient(bsEndPoint);
                if(socket == null)
                {
                    Logger.Error("Socket not created!", MODULE);
                    continue;
                }
                Console.WriteLine("new socked started");
                try
                {
                    // получение данных от блока сенсоров
                    byte[] rawResponse = fetchSBData(socket);

                    // TEST
                    // Console.WriteLine("\nОтвет от сервера размером: {0}\n\n", rawResponse.Length);
                    //File.WriteAllBytes("d:\\!PROJECTS\\WIM\\AUTO\\DATA\\Kisler_1_198.wix_1", rawResponse);

                    parseSBData(rawResponse);


                    // освобождение сокета
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    Console.WriteLine("socket closed");

                    //Задержка потока. TODO: Переделать на таймер. Либо асинхронный вызов.
                    Thread.Sleep(INTERVAL);
                    //Console.WriteLine("thread weakup ok\n================================");
                }
                catch (SocketException r)
                {
                    Logger.Error("Socket Error: " + r.Message, MODULE);
                    Console.WriteLine(r.ToString());
                }
                
            }
            stop();
        }

        private void parseSBData(byte[] rawData)
        {
            IRepository rep = Repository.Instance;

            BSData bsData = rep.parseSBData(rawData);
            if (bsData.IsValidData())
            {
                Console.WriteLine("Recived data is valid");
                Logger.Info("Recived data is valid", MODULE);
            }
            else
            {
                Console.WriteLine("Data validation error");
                Logger.Warning("Data validation error", MODULE);
            }
        }

        // соединение с блоком сенсоров
        private EndPoint prepareBSEndPoint()
        {
            // получение параметров соединения
            string host = ConfigurationManager.AppSettings[KEY_SBHOST];
            string port = ConfigurationManager.AppSettings[KEY_SBPORT];
            System.Console.WriteLine("Client started. Host: " + host + " port: " + port + ". Try to connect...");
            IPAddress hostValue;
            // парсинг адреса хоста
            if (IPAddress.TryParse(host, out hostValue))
            {
                string portText = port;

                int portValue = 0;
                // парсинг порта хоста
                if (Int32.TryParse(portText, out portValue))
                {
                    EndPoint ep = new System.Net.IPEndPoint(hostValue, portValue);
                    return ep;
                }
                else
                {
                    Logger.Error("Invalid port", MODULE);
                }
            }
            else
            {
                Logger.Error("Invalid host ip address", MODULE);
            }
            return null;
        }

        // создание соединения с сервером блока сенсоров
        private Socket startClient(EndPoint ep)
        {

            Logger.Info("Client started. End Point: " + ep + ". Try to connect...", MODULE);
            Socket clientSocket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                
                clientSocket.Connect(ep);
                if (clientSocket.Connected)
                {
                    
                    Logger.Info("Connected.", MODULE); 
                }
            }
            catch (SocketException r)
            {
                Logger.Error("Connection Error: " + r.Message, MODULE);

                clientSocket.Close();
                return null;
            }

            return clientSocket;
        }

        private byte[] fetchSBData(Socket socket)
        {
            // TODO: do fetch data 
            // Буфер для входящих данных
            byte[] responseBuffer = new byte[1024];
            // Массив для данных, полученных от блока сенсоров
            byte[] response = new byte[0];

            // создание запроса готовых данных
            // byte[] request = Encoding.UTF8.GetBytes(message);
            byte[] request = { (byte)BSFlag.GET_DATA };
            // Отправляем данные через сокет
            int bytesSent = socket.Send(request);
            System.Console.WriteLine("sent {0} bytes ", bytesSent);
            
            // получение ответа
            do
            {
                int bytesRecived = socket.Receive(responseBuffer, responseBuffer.Length, 0);

                // изначальная длина полученых данных
                int respSize = response.Length;
                
                // увеличение размера массива полученных данных
                Array.Resize(ref response, respSize + bytesRecived);

                // проверка для отрезания хвоста. Нужно замеить на что-то более подходящее.
                byte[] responseData;
                if (bytesRecived < responseBuffer.Length)
                {
                    responseData = new byte[bytesRecived];
                    Array.Copy(responseBuffer, 0, responseData, 0, bytesRecived);
                }
                else
                {
                    responseData = responseBuffer;
                }

                // добавление даннхы их буфера к общем получнным данным
                responseData.CopyTo(response, respSize);
                
                // for string data: 
                // builder.Append(Encoding.UTF8.GetString(responseBuffer, 0, bytesRecived));
                // Console.WriteLine("CURRRR ответ сервера: " + builder.ToString());
            }
            while (socket.Available > 0);

            //Console.WriteLine("Ответ от сервера {0} byte : {1}\n", response.Capacity, Encoding.Default.GetString(response.ToArray()));

            return response;
        }

        // отключение от блока сенсоров(?)
        public bool stop()
        {
            Logger.Info("Stop Demon...", MODULE);
            //TODO: disconnect impl here
            return enabled = false;
        }

        // текущий статус соединения с блоком сенсоров
        public bool getServerStatus()
        {
            return enabled;
        }

    }
}
