using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WMSAPIS.Security
{
    public class Result<T>
    {
        public T Data { get; set; }

        public string Message { get; set; }

        public bool IsSuccess { get; set; }

        public int ReasonCode { get; set; }
        public string searchby { get; set; }
    }
}
