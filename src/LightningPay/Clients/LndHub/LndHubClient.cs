﻿using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

using LightningPay.Infrastructure.Api;

namespace LightningPay.Clients.LndHub
{
    public class LndHubClient : ApiServiceBase, ILightningClient
    {
        private readonly string baseUri;

        private bool clientInternalBuilt = false;

        public LndHubClient(HttpClient client, 
            LndHubOptions options) : base(client,
            BuildAuthentication(options))
        {
            this.baseUri = options.Address.ToBaseUrl();
        }


        public async Task<LightningInvoice> CreateInvoice(long satoshis, 
            string description, 
            CreateInvoiceOptions options = null)
        {
            var strAmount = satoshis.ToString(CultureInfo.InvariantCulture);
            var strExpiry = options.ToExpiryString();


            var request = new AddInvoiceRequest
            {
                Amount = strAmount,
                Memo = description,
                Expiry = strExpiry
            };

            var response = await this.SendAsync<AddInvoiceResponse>(HttpMethod.Post,
                $"{baseUri}/addinvoice",
                request);

            if (string.IsNullOrEmpty(response.PaymentRequest)
                || response.R_hash == null)
            {
                throw new ApiException("Cannot retrieve Payment request or request hash in the LNDHub api response",
                    System.Net.HttpStatusCode.BadRequest);
            }

            return response.ToLightningInvoice(satoshis, description, options);
        }

        public async Task<bool> CheckPayment(string invoiceId)
        {
            var response = await this.SendAsync<CheckPaymentResponse>(HttpMethod.Get,
                $"{baseUri}/checkpayment/{invoiceId}");

            return response.Paid;
        }

        internal static AuthenticationBase BuildAuthentication(LndHubOptions options)
        {
            if(string.IsNullOrEmpty(options.Login)
                || string.IsNullOrEmpty(options.Password))
            {
                throw new ArgumentException("Login and Password are mandatory for lndhub authentication");
            }

            return new LndHubAuthentication(options);
        }

        public static LndHubClient New(string address,
           string login,
           string password,
           HttpClient httpClient = null)
        {
            bool clientInternalBuilt = false;

            if (httpClient == null)
            {
                httpClient = new HttpClient();
                clientInternalBuilt = true;
            }

            LndHubClient client = new LndHubClient(httpClient, new LndHubOptions()
            {
                Address = new Uri(address),
                Login = login,
                Password = password
            });

            client.clientInternalBuilt = clientInternalBuilt;

            return client;
        }

        public void Dispose()
        {
            if (this.clientInternalBuilt)
            {
                this.httpClient?.Dispose();
            }
        }
    }
}
