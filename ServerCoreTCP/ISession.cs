using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    internal interface ISession
    {
        public EndPoint ConnectedEndPoint { get; }
    }
}
