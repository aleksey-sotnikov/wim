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
            BSData bsData = new BSData();

            /***

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

            // parse start code
            byte[] startCodeArr = new byte[4];
            Array.Copy(rawData, 0, startCodeArr, 0, 4);
            String startCode = Encoding.Default.GetString(startCodeArr);

            Console.WriteLine("dataString={0}", startCode);
            //
            byte[] packageNumArr = new byte[3];
            Array.Copy(rawData, 4, packageNumArr, 0, 3);

            int packageNum  = packageNumArr[0] << 16 + packageNumArr[1] << 8 + packageNumArr[2];

            Console.WriteLine("packageNum={0}", packageNum);

            //TODO: parse code here

            return bsData;
        }
    }
}
