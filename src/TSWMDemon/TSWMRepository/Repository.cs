using System;
using System.Text;
using TSWMRepository.domain;

namespace TSWMRepository
{
    public class Repository : IRepository
    {
        private static volatile IRepository _instance;
        private static object syncRoot = new Object();

        public int Counter { get; set; }

        private Repository() { }

        public static IRepository Instance
        {
            // singleton ???
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new Repository();
                    }
                }

                return _instance;
            }
        }

        public void WriteLine()
        {
            Counter++;
            Console.WriteLine("Hello from repository " + Counter);
        }

        public BSData parseSBData(byte[] rawData)
        {
            return BSDataParser.parseSBData(rawData);
        }


    }

    static class BSDataParser
    {
        static readonly string SENSOR_KISLER = "0E";
        static readonly string SENSOR_LOOP = "2E";

        static readonly int BYTE_LENGTH_4 = 4, BYTE_LENGTH_3 = 3, BYTE_LENGTH_1 = 1;

        public static BSData parseSBData(byte[] rawData)
        {
            BSData bsData = new BSData();
            byte[] threeByteArr = new byte[3], fourByteArr = new byte[4];
            string sensorType;

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

            string s4 = HttpServerUtility.UrlTokenEncode(bytes);    // gsjqFw2
            byte[] decBytes4 = HttpServerUtility.UrlTokenDecode(s4);
            // decBytes4 same as bytes

            ***/

            // var startCode = Convert.ToInt64(rawData[0]);
            // var startCode2 = Convert.ToInt64(rawData[1]);
            // string hexOutput = String.Format("{0:X}", startCode) + String.Format("{0:X}", startCode2) ;
            // string strHex = Convert.ToInt32(strBinary, 2).ToString("X");

            int sourceIndex = 0;

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
            sensorType = ByteArrayToString(new byte[]{ fourByteArr[2], fourByteArr[3] });

            sourceIndex += BYTE_LENGTH_4;
            /// END OF parse MAC ///

            sourceIndex += Environment.NewLine.Length; //FIXME: remove car return shift
            
            /// parse event time 3B ///                                                            - 5) event time
            Array.Copy(rawData, sourceIndex, threeByteArr, 0, BYTE_LENGTH_3);

            int eventTime = ByteArrayToInt24(threeByteArr);

            sourceIndex += BYTE_LENGTH_3;
            /// END OF parse event time ///

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
            if (SENSOR_KISLER.Equals(sensorType))
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



                Console.WriteLine("PARSED DATA: Sensor type KISLER; start code={0}; packageNum={1}; packageTransferTime={2}, MAC={3},\n" +
                                "eventTime={4}, falgs={5}, sensorSleep={6}, sensorMaxValue={7},\n" +
                                "noiseLevel={8}, freqDiscrete={9}, dataBlockCount={10},\ndataBlocks[{10}]=[{11}]\n" +
                                "amplImp={12}, widthImp={13}, squareImp={14}, roadTempr={15}, vibration={16},\nchecksum={17},endCode={18}",
                                startCode, packageNum, packageTransferTimeSpan, deviceMAC, eventTime, flags, sensorSleep,
                                sensorMaxValue, noiseLevel, freqDiscrete, dataBlockCount, string.Join(", ", dataBlocks),
                                amplImp, widthImp, squareImp, roadTempr,vibration,checksum,endCode);
            }

            // IF SENSOR IS LOOP                                                                    -- LOOP BRANCH
            else if (SENSOR_LOOP.Equals(sensorType))
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


                Console.WriteLine("PARSED DATA: Sensor type LOOP;  startCode={0}; packageNum={1}; packageTransferTime={2}, MAC={3},\n" +
                                "eventTime={4},falgs={5},sensorSleep={6}, sensorMaxValue={7},\n" +
                                "noiseLevel={8},freqDiscrete={9},dataBlockCount={10},\ndataBlocks[{10}]=[{11}]\n" +
                                "numberOfLocalMax={12},timeLocalMax1={13}, amplLocalMax2={14}, timeLocalMax2={15},\n" +
                                "amplLocalMax2={16}, timeLocalMax3={17}, amplLocalMax3={18},\nchecksum={19},endCode={20}",
                                startCode, packageNum, packageTransferTimeSpan, deviceMAC, eventTime, flags, sensorSleep,
                                sensorMaxValue, noiseLevel, freqDiscrete, dataBlockCount, string.Join(", ", dataBlocks), numberOfLocalMax,
                                timeLocalMax1, amplLocalMax1, timeLocalMax2, amplLocalMax2, timeLocalMax3, amplLocalMax3, checksum,endCode);
            }






            

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
}
