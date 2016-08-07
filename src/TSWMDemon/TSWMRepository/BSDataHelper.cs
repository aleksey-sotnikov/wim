﻿using System;

using System.Text;
using TSWMRepository.domain;

namespace TSWMRepository
{
    enum BSCommand
    {
        CMD_DEV_INFO	= 0x80, //	Запрос дескриптора
        CMD_PING        = 0x81, //	Ping
        CMD_HW_RESET    = 0x82, //	Аппаратный сброс
        CMD_SW_RESET    = 0x83, //	Программный сброс
        CMD_GET_ERROR	= 0x84, //	Запрос ошибок
        CMD_ZERO_ERROR	= 0x85, //	Обнуления ошибок
        CMD_SET_PARAM	= 0x86, //	Установка параметров
        CMD_GET_PARAM	= 0x87  //	Считывание данных
    }
    

    class BSDataHelper
    {
        public const byte START_BYTE = 0x55;

        public byte[] dataRequest(byte sensorType)
        {
            byte[] request = new byte[8]; 
            byte[] header = prepareHeader(BSCommand.CMD_GET_PARAM);

            header.CopyTo(request, 0);
            request[7] = sensorType;
            return request;
        }

        public byte[] pingRequest(){
            return prepareHeader(BSCommand.CMD_PING);
        }

        public byte[] lastError()
        {
            return prepareHeader(BSCommand.CMD_GET_ERROR);
        }

        public byte[] errorsReset()
        {
            return prepareHeader(BSCommand.CMD_ZERO_ERROR);
        }

        public byte[] hwReset()
        {
            return prepareHeader(BSCommand.CMD_HW_RESET);
        }

        public byte[] swReset()
        {
            return prepareHeader(BSCommand.CMD_SW_RESET);
        }

        private byte[] prepareHeader(BSCommand cmd)
        {
            byte[] header = new byte[7];
            header[0] = START_BYTE;
            header[1] = 0x01; //ADR TODO: set real address
            header[2] = 0x00; //PACKET_ID TODO: set real packet id
            header[3] = (byte)cmd; //Command
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
            header[4] = dataCount; //DATA_COUNT 2B
            header[5] = 0x00; //DATA_COUNT 2B
            header[6] = Crc8.ComputeChecksum(new byte[]{ header[1], header[2],header[3],header[4],header[5] }); //CRC8

            return header;
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
            return crc;
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