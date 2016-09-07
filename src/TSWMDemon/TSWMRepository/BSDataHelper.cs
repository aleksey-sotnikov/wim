using System;
using System.Text;
using TSWMRepository.domain;

namespace TSWMRepository
{
    /// <summary>
    /// Список команд запроса
    /// </summary>
    enum BSCommand
    {
        /// <summary>
        /// Запрос дескриптора
        /// </summary>
        CMD_DEV_INFO = 0x80,
        /// <summary>
        /// Ping
        /// </summary>
        CMD_PING = 0x81,
        /// <summary>
        /// Аппаратный сброс
        /// </summary>
        CMD_HW_RESET = 0x82,
        /// <summary>
        /// Программный сброс
        /// </summary>
        CMD_SW_RESET = 0x83,
        /// <summary>
        /// Запрос ошибок
        /// </summary>
        CMD_GET_ERROR = 0x84,
        /// <summary>
        /// Обнуление ошибок
        /// </summary>
        CMD_ZERO_ERROR = 0x85,
        /// <summary>
        /// Установка параметров
        /// </summary>
        CMD_SET_PARAM = 0x86,
        /// <summary>
        /// Считывание данных
        /// </summary>
        CMD_GET_PARAM = 0x87
    }
    
    /// <summary>
    /// Подготовка запросов и парсинг ответов от БОД
    /// </summary>
    public class BSDataHelper
    {

        // Стартовый байт
        public const byte START_BYTE = 0x55;

        // Адрес устройства TODO: set real address
        public const byte DEVICE_ADDRESS = 0xC1;

        //Рассположение данных в массиве запроса
        /// <summary>
        /// Стартовый байт
        /// </summary>
        public const int REQ_POS_START_BYTE = 0;
        /// <summary>
        /// Адрес устройства 
        /// </summary>
        public const int REQ_POS_DEV_ADDR = 1;
        /// <summary>
        /// Номер пакета
        /// </summary>
        public const int REQ_POS_PACKAGE_NUM = 2;
        /// <summary>
        /// Команда запроса
        /// </summary>
        public const int REQ_POS_CMD = 3;
        /// <summary>
        /// Кол-во данных в теле запроса (1 of 2 bytes) 
        /// </summary>
        public const int REQ_POS_DATA_COUNT_1 = 4;
        /// <summary>
        /// Кол-во данных в теле запроса (2 of 2 bytes) 
        /// </summary>
        public const int REQ_POS_DATA_COUNT_2 = 5;
        /// <summary>
        /// Контрольная сумма данных заголовка запроса
        /// </summary>
        public const int REQ_POS_HEADER_CRC = 6;
        /// <summary>
        /// Поле тела OP1
        /// </summary>
        public const int REQ_POS_OP1 = 7;
        /// <summary>
        /// Поле тела OP2
        /// </summary>
        public const int REQ_POS_OP2 = 8;
        /// <summary>
        /// Поле тела OP3
        /// </summary>
        public const int REQ_POS_OP3 = 9;
        /// <summary>
        /// Поле тела OP4
        /// </summary>
        public const int REQ_POS_OP4 = 10;


        /// <summary>
        /// Создание запроса данных на основе типа сенсора
        /// </summary>
        /// <param name="sensorType">Тип сенсора: 1-2 Петли, 5-12 Кислер</param>
        /// <returns>Подготовленный массив запроса</returns>
        public byte[] DataRequest(byte sensorType)
        {
            byte[] header = prepareHeader(BSCommand.CMD_GET_PARAM);
            int requestSize = header.Length + header[REQ_POS_DATA_COUNT_2];
            byte[] request = new byte[requestSize];
            header.CopyTo(request, 0);
            request[REQ_POS_OP1] = sensorType;
            request[requestSize - 1] = Crc8.ComputeChecksum(new byte[] { request[REQ_POS_OP1] }); //CRC8 Body hash 

            return request;
        }

        /// <summary>
        /// Создание запроса установки параметров БОД
        /// </summary>
        /// <param name="sensorType">Тип сенсора: 1-2 Петли, 5-12 Кислер ???</param>
        /// <param name="freq">Частота в петле /или/ Множ.прореж.данных 5-8</param>
        /// <param name="coeff">Напряжение возбужд(1-4) /или/ Коэффиц.деления</param>
        /// <param name="threshold">Порог регистрации</param>
        /// <returns>Подготовленный массив запроса</returns>
        public byte[] SetParams(byte sensorType, byte freq, byte coeff, byte threshold)
        {
            byte[] header = prepareHeader(BSCommand.CMD_SET_PARAM);

            int requestSize = header.Length + header[REQ_POS_DATA_COUNT_2];
            System.Console.WriteLine("requestSize"+ requestSize);
            byte[] request = new byte[requestSize];
            header.CopyTo(request, 0);
            request[REQ_POS_OP1] = sensorType;
            request[REQ_POS_OP2] = freq;
            request[REQ_POS_OP3] = coeff;
            request[REQ_POS_OP4] = threshold;
            request[requestSize - 1] = Crc8.ComputeChecksum(new byte[] { request[REQ_POS_OP1], request[REQ_POS_OP2], request[REQ_POS_OP3], request[REQ_POS_OP4] }); //CRC8 Body hash 
            System.Console.WriteLine("header crc:{0:X}", request[requestSize-1]);
            return request;
        }

        /// <summary>
        /// Создание запроса Ping
        /// </summary>
        /// <returns>Подготовленный массив запроса</returns>
        public byte[] PingRequest(){
            return prepareHeader(BSCommand.CMD_PING);
        }

        /// <summary>
        /// Создание запроса Ping
        /// </summary>
        /// <returns>Подготовленный массив запроса</returns>
        public byte[] LastError()
        {
            return prepareHeader(BSCommand.CMD_GET_ERROR);
        }

        /// <summary>
        /// Создание запроса на сброс ошибок 
        /// </summary>
        /// <returns>Подготовленный массив запроса</returns>
        public byte[] ErrorsReset()
        {
            return prepareHeader(BSCommand.CMD_ZERO_ERROR);
        }

        /// <summary>
        /// Создание запроса на аппаратный сброс 
        /// </summary>
        /// <returns>Подготовленный массив запроса</returns>
        public byte[] HwReset()
        {
            return prepareHeader(BSCommand.CMD_HW_RESET);
        }

        /// <summary>
        /// Создание запроса на программный сброс
        /// </summary>
        /// <returns>Подготовленный массив запроса</returns>
        public byte[] SwReset()
        {
            return prepareHeader(BSCommand.CMD_SW_RESET);
        }

        /// <summary>
        /// Подгатовка массива запроса в соответствии с командой
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private byte[] prepareHeader(BSCommand cmd)
        {
            //Определение кол-ва доп. данных в зависимости от типа запроса
            //Установка параметров БОД (CMD_SET_PARAM) - 4
            //Синхронизация времени (???)              - 4 
            //Запрос данных (CMD_GET_PARAM)            - 1
            byte dataCount = 0x00;
            switch (cmd)
            {
                case BSCommand.CMD_SET_PARAM:
                    dataCount = 0x04;
                    break;
                case BSCommand.CMD_GET_PARAM:
                    dataCount = 0x01;
                    break;
            }

            byte[] header = new byte[7];
            header[REQ_POS_START_BYTE] = START_BYTE;
            header[REQ_POS_DEV_ADDR] = DEVICE_ADDRESS; 
            header[REQ_POS_PACKAGE_NUM] = 0x00; //PACKET_ID TODO: set real packet id
            header[REQ_POS_CMD] = (byte) cmd; //Command
            header[REQ_POS_DATA_COUNT_1] = 0x00; //DATA_COUNT (2 Byte)
            header[REQ_POS_DATA_COUNT_2] = dataCount; //DATA_COUNT (2 Byte)
            header[REQ_POS_HEADER_CRC] = Crc8.ComputeChecksum(new byte[]{ header[REQ_POS_DEV_ADDR], header[REQ_POS_PACKAGE_NUM],header[REQ_POS_CMD],header[REQ_POS_DATA_COUNT_1],header[REQ_POS_DATA_COUNT_2] }); //CRC8 header hash

            return header;
        }

    }

    public static class CRC
    {
        // CRC-8 for Dallas iButton products from Maxim/Dallas AP Note 27
        static readonly byte[] crc8Table = new byte[]
        {
        0x00, 0x5E, 0xBC, 0xE2, 0x61, 0x3F, 0xDD, 0x83,
        0xC2, 0x9C, 0x7E, 0x20, 0xA3, 0xFD, 0x1F, 0x41,
        0x9D, 0xC3, 0x21, 0x7F, 0xFC, 0xA2, 0x40, 0x1E,
        0x5F, 0x01, 0xE3, 0xBD, 0x3E, 0x60, 0x82, 0xDC,
        0x23, 0x7D, 0x9F, 0xC1, 0x42, 0x1C, 0xFE, 0xA0,
        0xE1, 0xBF, 0x5D, 0x03, 0x80, 0xDE, 0x3C, 0x62,
        0xBE, 0xE0, 0x02, 0x5C, 0xDF, 0x81, 0x63, 0x3D,
        0x7C, 0x22, 0xC0, 0x9E, 0x1D, 0x43, 0xA1, 0xFF,
        0x46, 0x18, 0xFA, 0xA4, 0x27, 0x79, 0x9B, 0xC5,
        0x84, 0xDA, 0x38, 0x66, 0xE5, 0xBB, 0x59, 0x07,
        0xDB, 0x85, 0x67, 0x39, 0xBA, 0xE4, 0x06, 0x58,
        0x19, 0x47, 0xA5, 0xFB, 0x78, 0x26, 0xC4, 0x9A,
        0x65, 0x3B, 0xD9, 0x87, 0x04, 0x5A, 0xB8, 0xE6,
        0xA7, 0xF9, 0x1B, 0x45, 0xC6, 0x98, 0x7A, 0x24,
        0xF8, 0xA6, 0x44, 0x1A, 0x99, 0xC7, 0x25, 0x7B,
        0x3A, 0x64, 0x86, 0xD8, 0x5B, 0x05, 0xE7, 0xB9,
        0x8C, 0xD2, 0x30, 0x6E, 0xED, 0xB3, 0x51, 0x0F,
        0x4E, 0x10, 0xF2, 0xAC, 0x2F, 0x71, 0x93, 0xCD,
        0x11, 0x4F, 0xAD, 0xF3, 0x70, 0x2E, 0xCC, 0x92,
        0xD3, 0x8D, 0x6F, 0x31, 0xB2, 0xEC, 0x0E, 0x50,
        0xAF, 0xF1, 0x13, 0x4D, 0xCE, 0x90, 0x72, 0x2C,
        0x6D, 0x33, 0xD1, 0x8F, 0x0C, 0x52, 0xB0, 0xEE,
        0x32, 0x6C, 0x8E, 0xD0, 0x53, 0x0D, 0xEF, 0xB1,
        0xF0, 0xAE, 0x4C, 0x12, 0x91, 0xCF, 0x2D, 0x73,
        0xCA, 0x94, 0x76, 0x28, 0xAB, 0xF5, 0x17, 0x49,
        0x08, 0x56, 0xB4, 0xEA, 0x69, 0x37, 0xD5, 0x8B,
        0x57, 0x09, 0xEB, 0xB5, 0x36, 0x68, 0x8A, 0xD4,
        0x95, 0xCB, 0x29, 0x77, 0xF4, 0xAA, 0x48, 0x16,
        0xE9, 0xB7, 0x55, 0x0B, 0x88, 0xD6, 0x34, 0x6A,
        0x2B, 0x75, 0x97, 0xC9, 0x4A, 0x14, 0xF6, 0xA8,
        0x74, 0x2A, 0xC8, 0x96, 0x15, 0x4B, 0xA9, 0xF7,
        0xB6, 0xE8, 0x0A, 0x54, 0xD7, 0x89, 0x6B, 0x35
        };

        public static byte CRC8(byte[] bytes, int len)
        {
            byte crc = 0;
            for (var i = 0; i < len; i++)
                crc = crc8Table[crc ^ bytes[i]];
            return crc;
        }
    }

    static class BSDataParser
    {

        static readonly int BYTE_LENGTH_4 = 4, BYTE_LENGTH_3 = 3, BYTE_LENGTH_1 = 1;

        public static BSData parseSBData(byte[] rawData)
        {
            BSData bsData;
            byte[] threeByteArr = new byte[3], fourByteArr = new byte[4];
            string sensorType;
            int sourceIndex = 0;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////                        PACKAGE HEADER                             ///////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            /// parse start code 4B ///                                                            - 1) start code
            Array.Copy(rawData, sourceIndex, fourByteArr, 0, BYTE_LENGTH_4);
            string startCode = ByteArrayToString(fourByteArr);
            sourceIndex += BYTE_LENGTH_4;
            /// END OF parse start code ///

            sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

            /// parse package number 3B ///                                                        - 2) package number
            Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

            int packageNum = ByteArrayToInt24(threeByteArr);
            sourceIndex += BYTE_LENGTH_3;
            /// END OF parse package number ///

            sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

            /// parse package transfer time 3B///                                                 - 3) package transfer time
            Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

            int packageTransferTime = ByteArrayToInt24(threeByteArr);
            sourceIndex += BYTE_LENGTH_3;

            TimeSpan packageTransferTimeSpan = TimeSpan.FromMilliseconds(packageTransferTime * 10);

            /// END OF parse package transfer time ///

            sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

            /// parse MAC 4B ///                                                                   - 4) MAC
            Array.Copy(rawData, sourceIndex, fourByteArr, 0, BYTE_LENGTH_4);

            string deviceMAC = ByteArrayToString(fourByteArr);

            // sensorType = half byte of sernsor MAC                                               -   sensor type
            sensorType = ByteArrayToString(new byte[] { fourByteArr[2], fourByteArr[3] });

            sourceIndex += BYTE_LENGTH_4;
            /// END OF parse MAC ///

            sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

            /// parse event time 3B ///                                                            - 5) event time
            Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

            int eventTime = ByteArrayToInt24(threeByteArr);

            sourceIndex += BYTE_LENGTH_3;
            /// END OF parse event time ///

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////                    END OF PACKAGE HEADER                          ///////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////


            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////                         PACKAGE BODY                              ///////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////

            sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

            /// parse flags 1B ///                                                                 - 6) flag
            int flags = rawData[sourceIndex];

            Console.WriteLine("flags bits=" + Pad(rawData[sourceIndex]));

            sourceIndex += BYTE_LENGTH_1;
            /// END OF parse flags ///

            sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

            /// parse sensor sleep 3B ///                                                          - 7) sensor sleep
            Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

            int sensorSleep = ByteArrayToInt24(threeByteArr);
            sourceIndex += BYTE_LENGTH_3;
            /// END OF parse sensor sleep ///

            sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

            /// parse sensor max value 3B ///                                                      - 8) sensor max value
            Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

            int sensorMaxValue = ByteArrayToInt24(threeByteArr);
            sourceIndex += BYTE_LENGTH_3;
            /// END OF parse sensor max value ///


            // IF SENSOR IS KISLER                                                                 -- IF KISLER
            int axesValue = 0;
            if (BSData.SENSOR_KISLER.Equals(sensorType))
            {
                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse axes value 3B ///                                                        - 8b) axes value
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                axesValue = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse axes value ///
            }

            sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift   

            /// parse sensor noise level 3B ///                                                    - 9) sensor noise level
            Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

            int noiseLevel = ByteArrayToInt24(threeByteArr);
            sourceIndex += BYTE_LENGTH_3;
            /// END OF parse noise level ///

            sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

            /// parse discrete frequency 1B ///                                                    - 10 discrete frequency  
            int freqDiscrete = rawData[sourceIndex];
            Console.WriteLine("discr freq bits=" + Pad(rawData[sourceIndex]));
            sourceIndex += BYTE_LENGTH_1;
            /// END OF parse discrete frequency ///

            //sourceIndex += Environment.NewLine.Length;

            /// parse dataBlockCount 1B ///                                                        - data block count
            int dataBlockCount = rawData[sourceIndex];
            Console.WriteLine("dataBlockCount bits=" + Pad(rawData[sourceIndex]));
            sourceIndex += BYTE_LENGTH_1;
            /// END OF parse dataBlockCount ///

            sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift


            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //FIXME
            if (BSData.SENSOR_KISLER.Equals(sensorType))
                dataBlockCount = 32;
            else
                dataBlockCount = 31;
            //FIXME
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


            /// parse data blocks 3B * dataBlockCount ///                                          - data Blocks
            int[] dataBlocks = new int[dataBlockCount];
            for (int i = 0; i < dataBlockCount; i++)
            {
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);
                dataBlocks[i] = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift
            }
            /// END OF parse data blocks ///


            // IF SENSOR IS KISLER                                                                  -- KISLER BRANCH
            if (BSData.SENSOR_KISLER.Equals(sensorType))
            {
                //FFFFCC skip                                                                       - 29) free
                sourceIndex += 6;

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                //TODO: TIME Imp

                /// parse sensor AmplImp 3B ///                                                     - 30) Ampl Imp
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int amplImp = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse AmplImp ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor WidthImp 3B ///                                                     - 31) Width Imp
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int widthImp = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse WidthImp ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor SquareImp 3B ///                                                     - 32) Square Imp
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int squareImp = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse SquareImp ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor Road Temprature 3B ///                                               - 33) Road Tempr
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int roadTempr = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse Road Tempr ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor Vibration 3B ///                                                     - 34) Vibration
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int vibration = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse Vibration ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor checksum 3B ///                                                     - 35) checksum
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int checksum = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse checksum ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse end code 4B ///                                                           - 36) end code
                Array.Copy(rawData, sourceIndex, fourByteArr, 0, BYTE_LENGTH_4);
                string endCode = ByteArrayToString(fourByteArr);
                sourceIndex += BYTE_LENGTH_4;
                /// END OF parse end code ///

                BSKislerData bsKislerData = new BSKislerData();

                bsKislerData.PackageNumber = packageNum;
                bsKislerData.TimeStampFrom = packageTransferTimeSpan;
                bsKislerData.TimeFromStart = eventTime;
                bsKislerData.DeviceMAC = deviceMAC;
                bsKislerData.Flag = flags;
                bsKislerData.SensorSleepValue = sensorSleep;
                bsKislerData.SensorMaxValue = sensorMaxValue;
                bsKislerData.SensorAxesValue = axesValue;           //??
                bsKislerData.NoiseLevel = noiseLevel;
                bsKislerData.Decimation = freqDiscrete;
                bsKislerData.DataBlockCount = dataBlockCount;
                bsKislerData.DataBlocks = dataBlocks;
                bsKislerData.ImpulseAmplitude = amplImp;
                bsKislerData.ImpulseWidth = widthImp;
                bsKislerData.ImpulseSquare = squareImp;
                bsKislerData.RoadTemperature = roadTempr;
                bsKislerData.Vibration = vibration;
                bsKislerData.Checksum = checksum;

                bsData = bsKislerData;


                Console.WriteLine("PARSED DATA: Sensor type KISLER; start code={0}; packageNum={1}; packageTransferTime={2}, MAC={3},\n" +
                                "eventTime={4}, falgs={5}, sensorSleep={6}, sensorMaxValue={7},axesValue={19}\n" +
                                "noiseLevel={8}, freqDiscrete={9}, dataBlockCount={10},\ndataBlocks[{10}]=[{11}]\n" +
                                "amplImp={12}, widthImp={13}, squareImp={14}, roadTempr={15}, vibration={16},\nchecksum={17},endCode={18}",
                                startCode, packageNum, packageTransferTimeSpan, deviceMAC, eventTime, flags, sensorSleep,
                                sensorMaxValue, noiseLevel, freqDiscrete, dataBlockCount, string.Join(", ", dataBlocks),
                                amplImp, widthImp, squareImp, roadTempr, vibration, checksum, endCode, axesValue);
            }

            // IF SENSOR IS LOOP                                                                    -- LOOP BRANCH
            else if (BSData.SENSOR_LOOP.Equals(sensorType))
            {
                /// parse sensor NumberOfLocalMax 3B ///                                            - 29) Number Of Local Max
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int numberOfLocalMax = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse NumberOfLocalMax ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor Time Of Local Max 1 3B ///                                         - 30) Time Of Local Max 1
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int timeLocalMax1 = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse Time Of Local Max 1 ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor Ampl Local Max 1 3B ///                                            - 31) Ampl Of Local Max 1 
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int amplLocalMax1 = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse Ampl Local Max 1 ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor Time Of Local Max 2 3B ///                                         - 32) Time Of Local Max 2
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int timeLocalMax2 = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse Time Of Local Max 2 ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor Ampl Local Max 2 3B ///                                            - 33) Ampl Of Local Max 2 
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int amplLocalMax2 = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse Ampl Local Max 2 ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor Time Of Local Max 3 3B ///                                         - 34) Time Of Local Max 3
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int timeLocalMax3 = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse Time Of Local Max 3 ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor Ampl Local Max 3 3B ///                                            - 35) Ampl Of Local Max 3 
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int amplLocalMax3 = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse Ampl Local Max 3 ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse sensor checksum 3B ///                                                    - 36) checksum
                Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

                int checksum = ByteArrayToInt24(threeByteArr);
                sourceIndex += BYTE_LENGTH_3;
                /// END OF parse checksum ///

                sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift

                /// parse end code 4B ///                                                           - 37) end code
                Array.Copy(rawData, sourceIndex, fourByteArr, 0, BYTE_LENGTH_4);
                string endCode = ByteArrayToString(fourByteArr);
                sourceIndex += BYTE_LENGTH_4;
                /// END OF parse end code ///

                BSLoopData bsLoopData = new BSLoopData();

                bsLoopData.PackageNumber = packageNum;
                bsLoopData.TimeStampFrom = packageTransferTimeSpan;
                bsLoopData.TimeFromStart = eventTime;
                bsLoopData.DeviceMAC = deviceMAC;
                bsLoopData.Flag = flags;
                bsLoopData.SensorSleepValue = sensorSleep;
                bsLoopData.SensorMaxValue = sensorMaxValue;
                bsLoopData.NoiseLevel = noiseLevel;
                bsLoopData.DescreteFrequency = freqDiscrete;
                bsLoopData.DataBlockCount = dataBlockCount;
                bsLoopData.DataBlocks = dataBlocks;
                bsLoopData.LocalMaxCount = numberOfLocalMax;
                bsLoopData.LocalMax1Time = timeLocalMax1;
                bsLoopData.LocalMax1Amplitude = amplLocalMax1;
                bsLoopData.LocalMax2Time = timeLocalMax2;
                bsLoopData.LocalMax2Amplitude = amplLocalMax2;
                bsLoopData.LocalMax3Time = timeLocalMax3;
                bsLoopData.LocalMax3Amplitude = amplLocalMax3;
                bsLoopData.Checksum = checksum;

                bsData = bsLoopData;


                Console.WriteLine("PARSED DATA: Sensor type LOOP;  startCode={0}; packageNum={1}; packageTransferTime={2}, MAC={3},\n" +
                                "eventTime={4},falgs={5},sensorSleep={6}, sensorMaxValue={7},\n" +
                                "noiseLevel={8},freqDiscrete={9},dataBlockCount={10},\ndataBlocks[{10}]=[{11}]\n" +
                                "numberOfLocalMax={12},timeLocalMax1={13}, amplLocalMax2={14}, timeLocalMax2={15},\n" +
                                "amplLocalMax2={16}, timeLocalMax3={17}, amplLocalMax3={18},\nchecksum={19},endCode={20}",
                                startCode, packageNum, packageTransferTimeSpan, deviceMAC, eventTime, flags, sensorSleep,
                                sensorMaxValue, noiseLevel, freqDiscrete, dataBlockCount, string.Join(", ", dataBlocks), numberOfLocalMax,
                                timeLocalMax1, amplLocalMax1, timeLocalMax2, amplLocalMax2, timeLocalMax3, amplLocalMax3, checksum, endCode);
            }
            else
            {
                throw new NotSupportedException("Sensor not supported: " + deviceMAC);
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////                     END OF PACKAGE BODY                           ///////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////






            /*** Parse examples

                byte[] bytes = { 130, 200, 234, 23 }; // A byte array contains non-ASCII (or non-readable) characters

                string s1 = Encoding.UTF8.GetString(bytes); // ���
                byte[] decBytes1 = Encoding.UTF8.GetBytes(s1);  // decBytes1.Length == 10 !!
                // decBytes1 not same as bytes
                // Using UTF-8 or other Encoding object will get similar results

                string s2 = BitConverter.ToString(bytes);   // 82-C8-EA-17
                String[] tempAry = s2.Split('-');
                byte[] decBytes2 = new byte[tempAry.Length];
                for (int i = 0; i < tempAry.Length; i++)
                    decBytes2[i] = Convert.ToByte(tempAry[i], 16);
                // decBytes2 same as bytes

                string s3 = Convert.ToBase64String(bytes);  // gsjqFw==
                byte[] decByte3 = Convert.FromBase64String(s3);
                // decByte3 same as bytes

                ***/

            // var startCode = Convert.ToInt64(rawData[0]);
            // var startCode2 = Convert.ToInt64(rawData[1]);
            // string hexOutput = String.Format("{0:X}", startCode) + String.Format("{0:X}", startCode2) ;
            // string strHex = Convert.ToInt32(strBinary, 2).ToString("X");


            return bsData;
        }

        private static int ByteArrayToInt24(byte[] arr)
        {
            byte b1 = arr[0], b2 = arr[1], b3 = arr[2];
            int r = 0;
            byte b0 = 0xff;

            if ((b1 & 0x80) != 0) r |= b0 << 24;
            r |= b1 << 16;
            r |= b2 << 8;
            r |= b3;

            return r;

            // simple version
            // return arr[0] << 16 + arr[1] << 8 + arr[2];
        }

        static string Pad(byte b)
        {
            return Convert.ToString(b, 2).PadLeft(8, '0');
        }

        public static string ToHex(this byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];

            byte b;

            for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
            {
                b = ((byte)(bytes[bx] >> 4));
                c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

                b = ((byte)(bytes[bx] & 0x0F));
                c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }

            return new string(c);
        }

        private static string ByteArrayToString(byte[] arr)
        {
            return Encoding.Default.GetString(arr);
        }
    }

    /// <summary>
    /// Вычисление контрольной суммы CRC-8 байтового массива
    /// </summary>
    public static class Crc8
    {
        static byte[] table = new byte[256];
        // x8 + x7 + x6 + x4 + x2 + 1
        const byte poly = 0xd5;

        public static byte ComputeChecksum(params byte[] bytes)
        {
            
            byte crc = 0;
            if (bytes != null && bytes.Length > 0)
            {
                foreach (byte b in bytes)
                {
                    crc = table[crc ^ b];
                }
            }

            //return CS(bytes);
            return crc;  
        }


        static byte RotLeft(byte val, byte lShift, byte rShift)
        {
            return (byte)((val << lShift) | (val >> rShift));
        }

        static byte CS(byte[] buffer)
        {
            byte bits = 8;
            byte lShift = 2;
            byte rShift = (byte)(bits - lShift);
            byte res = 0;
            byte index = 0;
            int count = buffer.Length;

            while (count-- > 0)
                res = (byte)(RotLeft(res, lShift, rShift) ^ buffer[index++]);

            return RotLeft(res, lShift, rShift);
        }



        static Crc8()
        {
            for (int i = 0; i < 256; ++i)
            {
                int temp = i;
                for (int j = 0; j < 8; ++j)
                {
                    if ((temp & 0x80) != 0)
                    {
                        temp = (temp << 1) ^ poly;
                    }
                    else
                    {
                        temp <<= 1;
                    }
                }
                table[i] = (byte)temp;
            }
        }
    }
}
