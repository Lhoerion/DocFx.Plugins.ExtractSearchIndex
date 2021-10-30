using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class FieldVectorsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t = JToken.FromObject(value);

            if (t.Type != JTokenType.Array)
            {
                t.WriteTo(writer);
            }
            else
            {
                var arr = (JArray)t;

                var elements = (JArray)arr[1];

                var newElements = new List<dynamic>();

                foreach (var element in elements)
                {
                    var val = element.Value<double>();
                    if (Math.Abs(val - Convert.ToInt32(val)) == 0)
                    {
                        newElements.Add(Convert.ToInt32(val));
                    }
                    else
                    {
                        newElements.Add(val);
                    }
                }

                arr[1] = JArray.FromObject(newElements);

                arr.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead => false;

        public override bool CanConvert(Type objectType) => true;
    }
}