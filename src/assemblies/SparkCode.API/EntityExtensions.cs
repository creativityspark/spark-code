using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System.Text.Json;

namespace SparkCode
{
    public static class EntityExtensions
    {
        public static string ToJson(this EntityCollection entities)
        {
            using (var stringWriter = new System.IO.StringWriter())
            {
                var jw = new JsonTextWriter(stringWriter);
                jw.WriteStartArray();

                foreach (var entity in entities.Entities)
                {
                    jw.WriteStartObject();
                    WriteEntity(jw, stringWriter, entity);
                    jw.WriteEndObject();
                }

                jw.WriteEndArray();
                return stringWriter.ToString();
            }
        }

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
                else if (entity.Attributes[key] is AliasedValue)
                {
                    var aliasedValue = (AliasedValue)entity.Attributes[key];
                    writer.WriteValue(aliasedValue.Value);
                }
                else if (entity.Attributes[key] is OptionSetValue)
                {
                    var optionSetValue = (OptionSetValue)entity.Attributes[key];
                    writer.WriteValue(optionSetValue.Value);
                }
                else
                {
                    writer.WriteValue(entity.Attributes[key]);
                }
            }
        }
    }
}
