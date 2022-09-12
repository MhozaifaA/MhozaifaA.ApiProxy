using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MhozaifaA.ApiProxy.Options
{
    public class HttpProxyOptions
    {
        public const string _SectionName = "HttpProxy";
        public Uri? BaseAddress { get; set; }
    }
}
