using Ganss.XSS;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CustomBindings.Bindings
{
    public class BindingExtensionProvider : IExtensionConfigProvider
    {
        private readonly ILogger logger;
        public BindingExtensionProvider(ILogger<Startup> logger)
        {
            this.logger = logger;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            context.AddBindingRule<HttpRequestBodyAttribute>()
                .Bind(new HttpRequestBodyBindingProvider(this.logger));

            context.AddBindingRule<AntiXSSHttpRequestAttribute>()
                .Bind(new AntiXSSHttpRequestBindingProvider(this.logger));
        }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class HttpRequestBodyAttribute : Attribute
    {

    }

    public class HttpRequestBodyBindingProvider : IBindingProvider
    {
        private readonly ILogger logger;
        public HttpRequestBodyBindingProvider(ILogger logger)
        {
            this.logger = logger;
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            IBinding binding = CreateBodyBinding(logger, context.Parameter.ParameterType);
            return Task.FromResult(binding);
        }

        private IBinding CreateBodyBinding(ILogger log, Type T)
        {
            var type = typeof(HttpRequestBodyBinding<>).MakeGenericType(T);
            var a_Context = Activator.CreateInstance(type, new object[] { log });
            return (IBinding)a_Context;
        }
    }

    public class HttpRequestBodyBinding<T> : IBinding
    {
        private readonly ILogger logger;
        public HttpRequestBodyBinding(ILogger logger)
        {
            this.logger = logger;
        }

        public bool FromAttribute => true;

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            return null;
        }

        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            var request = context.BindingData["req"] as HttpRequest;
            return Task.FromResult<IValueProvider>(new HttpRequestBodyValueProvider<T>(request, logger));
        }

        public ParameterDescriptor ToParameterDescriptor() => new ParameterDescriptor();
    }

    public class HttpRequestBodyValueProvider<T> : IValueProvider
    {
        private HttpRequest request;
        private ILogger logger;

        public HttpRequestBodyValueProvider(HttpRequest request, ILogger logger)
        {
            this.request = request;
            this.logger = logger;
        }

        public Type Type => typeof(object);
        public string ToInvokeString() => string.Empty;

        public async Task<object> GetValueAsync()
        {
            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            try
            {
                requestBody = new HtmlSanitizer().Sanitize(requestBody);
                T result = JsonConvert.DeserializeObject<T>(requestBody);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Error deserializing object from body: {requestBody}");
                throw;
            }
        }
    }
}
