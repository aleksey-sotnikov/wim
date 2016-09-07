using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSWMRepository.domain;

namespace TSWMRepository
{
    public interface IRepository
    {
        //FIXME: Test method
        void WriteLine();

        BSData parseSBData(byte[] rawData);

        BSDataHelper getBSDataHelper();
    }
}
