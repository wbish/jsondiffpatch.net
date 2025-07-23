using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonDiffPatchDotNet
{
    public abstract class ItemMatch
    {
        internal Func<JToken, object> ObjectHash;

        protected ItemMatch()
        {
            
        }

        protected ItemMatch(Func<JToken, object> objectHash)
        {
            ObjectHash = objectHash;
        }

        public virtual bool Match(JToken object1, JToken object2)
        {
            return Match(object1, object2, ObjectHash);
        }

		public virtual bool MatchArrayElement(JToken object1, int index1, JToken object2, int index2)
		{
			if(ObjectHash != null)
			{
				return Match(object1, object2, ObjectHash);
			}

			if(object1.Type != JTokenType.Object && object1.Type != JTokenType.Array)
			{
				return JToken.DeepEquals(object1, object2);
			}

			return index1 == index2;
		}

        public virtual bool Match(JToken object1, JToken object2, Func<JToken, object> objectHash)
        {
			if(objectHash == null || object1.Type != JTokenType.Object)
			{
				return JToken.DeepEquals(object1, object2);
			}

			var hash1 = objectHash.Invoke(object1);
            if(hash1 == null)
            {
                return false;
            }
            var hash2 = objectHash.Invoke(object2);
            if(hash2 == null)
            {
                return false;
            }

            return hash1.Equals(hash2);
        }
    }
}
