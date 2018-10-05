using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonDiffPatchDotNet
{
    public class DefaultItemMatch : ItemMatch
    {
        public DefaultItemMatch(Func<JToken, object> objectHash):base(objectHash)
        {
            
        }
    }
}
