using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace DistributeOrderImportTool.Utils
{
    public static class JsonSerializer
    {
        public static string Serialize(object obj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(obj);
        }

        public static T Deserialize<T>(string serializedString)
        {
            try {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                return serializer.Deserialize<T>(serializedString);
            } catch {
                return default(T);
            }
        }
    }
}
