using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace q_limits
{
    public class Credential
    {
        public string Key;
        public string Value;

        public Credential(string key = null, string value = null)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
