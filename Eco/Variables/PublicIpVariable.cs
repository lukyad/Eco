using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Eco.Variables
{
    public class PublicIpVariable : IVariableProvider
    {
        string _lastIp;
        DateTime _lastUpdate;

        public Dictionary<string, Func<string>> GetVariables()
        {
            var now = DateTime.Now;
            if (now -  _lastUpdate > TimeSpan.FromMinutes(1))
            {
                _lastIp = new WebClient().DownloadString("http://icanhazip.com").Trim();
                _lastUpdate = now;
            }
            return new Dictionary<string, Func<string>>()
            {
                { "publicIp", () => _lastIp }
            }; 
        }
    }
}
