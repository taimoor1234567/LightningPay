﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

using LightningPay.Tools;

namespace LightningPay.Infrastructure.Api
{
    public abstract class ApiServiceBase
    {
        protected readonly HttpClient httpClient;

        private readonly AuthenticationBase authentication;

        public ApiServiceBase(HttpClient httpClient, 
            AuthenticationBase authentication)
        {
            this.httpClient = httpClient;
            this.authentication = authentication;
        }

        protected async Task<string> GetStringAsync(string url)
        {
            return await this.httpClient.GetStringAsync(url);
        }

        protected async Task<TResponse> SendAsync<TResponse>(HttpMethod method,
           string url,
           object body = null)
           where TResponse : class
        {
            HttpContent content = null;

            if (body != null)
            {
                content = new StringContent(Json.Serialize(body), Encoding.UTF8, "application/json");
            }

            var request = new HttpRequestMessage(method, url)
            {
                Content = content
            };
            authentication.AddAuthentication(request);

            try
            {
                var response = await this.httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(json))
                    {
                        var responseObject = Json.Deserialize<TResponse>(json, new JsonOptions() 
                        {
                            SerializationOptions = JsonSerializationOptions.ByteArrayAsBase64
                        }
                        );

                        return responseObject;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    throw new ApiException($"Http error with status code {response.StatusCode} and response data {errorContent}",
                        response.StatusCode,
                        responseData: errorContent);
                }

            }
            catch (Exception exc)
            {
                throw new ApiException($"Internal Error on request the url : {url} : {exc.Message}", 
                    HttpStatusCode.InternalServerError, 
                    innerException: exc);
            }

        }

    }
}
