using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampBot
{
    public class JsonTypeHandler : SqlMapper.ITypeHandler
    {
        public void SetValue(IDbDataParameter parameter, object value)
        {
            parameter.Value = JsonConvert.SerializeObject(value);
        }

        public object Parse(Type destinationType, object value)
        {
            return JsonConvert.DeserializeObject(value as string, destinationType);
        }
    }
}
