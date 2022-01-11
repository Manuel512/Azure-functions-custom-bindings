using CustomBindings;
using CustomBindings.Bindings;
using Ganss.XSS;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(Startup))]
namespace CustomBindings
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var wbBuilder = builder.Services.AddWebJobs(x => { return; });

            // And now you can use AddExtension
            wbBuilder.AddExtension<BindingExtensionProvider>();

            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new AntiXSSIOPropertiesConverter()
                }
            };
            JsonConvert.DefaultSettings = () => jsonSerializerSettings;

            builder.Services.AddSingleton<IAntiXSSActionResult, AntiXSSActionResult>();
        }
    }

    public class AntiXSSIOPropertiesConverter : JsonConverter
    {
        private readonly HtmlSanitizer _htmlSanitizer;
        public AntiXSSIOPropertiesConverter()
        {
            _htmlSanitizer = new HtmlSanitizer();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(_htmlSanitizer.Sanitize(value.ToString()));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jToken = null;
            object target;

            if (reader.TokenType == JsonToken.StartArray)
                jToken = JArray.Parse(_htmlSanitizer.Sanitize(reader.Value.ToString()));

            if (reader.TokenType == JsonToken.StartObject)
                jToken = JObject.Parse(_htmlSanitizer.Sanitize(reader.Value.ToString()));             

            if (jToken != null)
            {
                target = Activator.CreateInstance(objectType);
                serializer.Populate(jToken.CreateReader(), target);
            }

            if (reader.TokenType == JsonToken.Null)
                return null;
            else
            {
                jToken = JToken.Load(reader);
                target = _htmlSanitizer.Sanitize(((JValue)jToken).Value.ToString());
            }

            return target;
        }
    }


    public class AntiXSSInputPropertiesConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JToken jtoken = JToken.Load(reader);

            // Create target object based on JObject
            object target = Activator.CreateInstance(objectType);

            // Populate the object properties
            serializer.Populate(jtoken.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
