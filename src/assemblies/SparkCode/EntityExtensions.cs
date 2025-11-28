using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System.Text.Json;

namespace SparkCode
{
    public static class EntityExtensions
    {
        public static string ToJson(this Entity entity)
        {
            using (var stringWriter = new System.IO.StringWriter())
            {
                var jw = new JsonTextWriter(stringWriter);
                jw.WriteStartObject();
                WriteEntity(jw, stringWriter, entity);
                jw.WriteEndObject();
                return stringWriter.ToString();
            }
        }

        private static void WriteEntity(JsonTextWriter writer, System.IO.StringWriter sw, Entity entity)
        {
            foreach (var key in entity.Attributes.Keys)
            {
                writer.WritePropertyName(key);
                if (entity.Attributes[key] is Entity)
                {
                    writer.WriteStartObject();
                    WriteEntity(writer, sw, (Entity)entity.Attributes[key]);
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteValue(entity.Attributes[key]);
                }
            }
        }
    }
}
