using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedisRx.Publisher;

namespace RedisRx
{
    class JsonNetSerializer : ISerializer
    {
        public string Serialize(object t)
        {
            return JsonConvert.SerializeObject(t);
        }
    }
}
