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

}
