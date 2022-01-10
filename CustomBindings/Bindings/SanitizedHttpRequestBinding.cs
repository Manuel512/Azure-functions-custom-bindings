using Ganss.XSS;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomBindings.Bindings
{
    public class AntiXSSHttpRequestBindingProvider : IBindingProvider
    {
        private readonly ILogger logger;
        public AntiXSSHttpRequestBindingProvider(ILogger logger)
        {
            this.logger = logger;
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            var paramType = context.Parameter.ParameterType;
            var genericParamType = paramType.GetGenericArguments().FirstOrDefault();
            IBinding binding = CreateBodyBinding(logger, genericParamType ?? paramType);
            return Task.FromResult(binding);
        }

        private IBinding CreateBodyBinding(ILogger log, Type T)
        {
            var type = typeof(AntiXSSHttpRequestBinding<>).MakeGenericType(T);
            var a_Context = Activator.CreateInstance(type, new object[] { log });
            return (IBinding)a_Context;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class AntiXSSHttpRequestAttribute : Attribute
    {

    }

    public class AntiXSSHttpRequestBinding<T> : IBinding
    {
        private readonly ILogger logger;
        public AntiXSSHttpRequestBinding(ILogger logger)
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
            return Task.FromResult<IValueProvider>(new AntiXSSHttpRequestValueProvider<T>(request, logger));
        }

        public ParameterDescriptor ToParameterDescriptor() => new ParameterDescriptor();
    }

    public class AntiXSSHttpRequestValueProvider<T> : IValueProvider
    {
        private HttpRequest _request;
        private ILogger _logger;
        private readonly HtmlSanitizer _htmlSanitizer;

        public AntiXSSHttpRequestValueProvider(HttpRequest request, ILogger logger)
        {
            _request = request;
            _logger = logger;
            _htmlSanitizer = new HtmlSanitizer();
        }

        public Type Type => typeof(object);
        public string ToInvokeString() => string.Empty;

        public async Task<object> GetValueAsync()
        {
            string requestBody = await new StreamReader(_request.Body).ReadToEndAsync();
            try
            {
                SanitizedHttpRequest httpRequest = null;

                if (typeof(T) != typeof(SanitizedHttpRequest))
                {
                    httpRequest = new SanitizedHttpRequest<T>
                    {
                        Body = GetTypedRequestBody(requestBody),
                    };
                }

                httpRequest ??= new SanitizedHttpRequest();

                httpRequest.Query = _request.Query.ToDictionary(x => x.Key, x => _htmlSanitizer.Sanitize(x.Value));

                return httpRequest;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Error deserializing object from body: {requestBody}");
                throw;
            }
        }

        private T GetTypedRequestBody(string requestBody)
        {
            if (string.IsNullOrEmpty(requestBody))
                return default;

            return JsonConvert.DeserializeObject<T>(requestBody);
        }
    }
}
