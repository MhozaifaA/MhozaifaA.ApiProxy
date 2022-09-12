using Meteors;
using MhozaifaA.ApiProxy.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MhozaifaA.ApiProxy
{
    public class HttpProxy : IHttpProxy
    {
        private readonly HttpProxyOptions options;
        private readonly HttpClient httpClient;
        private readonly IHttpContextAccessor context;

        public HttpProxy(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor context, IOptions<HttpProxyOptions> options)
        {
            if (options?.Value is null)
                throw new ArgumentNullException("HttpProxyOptions options, pass options or use HttpProxy section in appsetings");

            if (options!.Value.BaseAddress is null)
                throw new ArgumentNullException("HttpProxyOptions options, pass options or use HttpProxy section in appsetings should contain BaseAddress");

            this.options = options!.Value;

        
            this.httpClient = httpClient;
            this.context = context;
            this.httpClient.BaseAddress = this.options.BaseAddress;
        }
        /// <summary>
        /// this work as middleware can be replace with <code>app.Use() </code> as global but this use in side action to more relaibel and flixble update the apis.
        /// <para>Actting as proxy middle layer</para>
        /// </summary>
        public Task<JsonResult> Call => _Call().ToJsonResultAsync();

        public async Task<OperationResult<byte>> _Call()
        {
            var uriApi = string.Concat(options!.BaseAddress!.OriginalString, context!.HttpContext!.Request.GetEncodedPathAndQuery());

            var createProxy = CreateProxyHttpRequest(context.HttpContext, new Uri(uriApi));
            HttpResponseMessage? response = await httpClient.SendAsync(createProxy, HttpCompletionOption.ResponseContentRead, context.HttpContext.RequestAborted);

            await CopyProxyHttpResponse(context.HttpContext, response);

            return byte.MinValue.ToOperationResult();
        }


        private HttpRequestMessage CreateProxyHttpRequest(HttpContext context, Uri uri)
        {
            var request = context.Request;

            var requestMessage = new HttpRequestMessage();
            var requestMethod = request.Method;
            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(request.Body);
                requestMessage.Content = streamContent;
            }

            // Copy the request headers
            foreach (var header in request.Headers)
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());

            requestMessage.Headers.Host = uri.Authority;
            requestMessage.RequestUri = uri;
            requestMessage.Method = new HttpMethod(request.Method);

            return requestMessage;
        }


        private async Task CopyProxyHttpResponse(HttpContext context, HttpResponseMessage responseMessage)
        {
            if (responseMessage == null)
                throw new ArgumentNullException(nameof(responseMessage));

            var response = context.Response;

            response.StatusCode = (int)responseMessage.StatusCode;
            foreach (var header in responseMessage.Headers)
                response.Headers[header.Key] = header.Value.ToArray();

            foreach (var header in responseMessage.Content.Headers)
                response.Headers[header.Key] = header.Value.ToArray();

            // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
            response.Headers.Remove("transfer-encoding");

            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
            {
                await responseStream.CopyToAsync(response.Body, context.RequestAborted);
            }
        }

    }
}
