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
        public Dictionary<string, Func<string>> GetVariables()
        {
            return new Dictionary<string, Func<string>>()
            {
                { "publicIp", () => new WebClient().DownloadString("http://icanhazip.com").Trim() }
            }; 
        }
    }
}
