using System;
using System.Collections.Generic;
using System.Text;

namespace MobileShareLibrary.Utility
{
    class NetworkEventArgs:EventArgs
    {
        public object Data { get; set; }
        public bool isSucceed { get; set; }
    }
}
