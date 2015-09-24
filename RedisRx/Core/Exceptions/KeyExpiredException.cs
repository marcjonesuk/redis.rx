using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class KeyExpiredException : Exception
    {
        public string Key { get; private set; }
        public KeyExpiredException(string key)
        {
            Key = key;
        }
    }
}
