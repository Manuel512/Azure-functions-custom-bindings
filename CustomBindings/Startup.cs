using CustomBindings;
using CustomBindings.Bindings;
using Ganss.XSS;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
                    new AntiXSSOuputPropertiesConverter()
                }
            };
            JsonConvert.DefaultSettings = () => jsonSerializerSettings;

            builder.Services.AddSingleton<IAntiXSSActionResult, AntiXSSActionResult>();
        }
    }

    public class AntiXSSOuputPropertiesConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var htmlSanitizer = new HtmlSanitizer();
            writer.WriteValue(htmlSanitizer.Sanitize(value.ToString()));
        }
    }
}
