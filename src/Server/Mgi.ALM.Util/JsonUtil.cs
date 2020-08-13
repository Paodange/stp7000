using Newtonsoft.Json;

namespace Mgi.ALM.Util
{
    public class JsonUtil
    {

        public static string Serialize(object o, bool prettify = false)
        {
            return JsonConvert.SerializeObject(o, prettify ? Formatting.Indented : Formatting.None);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
        public static T ConvertTo<T>(object o)
        {
            return Deserialize<T>(Serialize(o));
        }
    }
}
