﻿using System;
using System.Threading.Tasks;

namespace LightningPay
{
    /// <summary>
    /// Every clients of the LightningPay package implements the interface ILightningClient
    /// </summary>
    public interface ILightningClient : IDisposable
    {
        /// <summary>Gets the wallet balance in satoshis.</summary>
        /// <returns>
        ///   Balance is satoshis
        /// </returns>
        Task<long> GetBalance();

        /// <summary>Creates the invoice.</summary>
        /// <param name="satoshis">The amount in satoshis.</param>
        /// <param name="description">The description will be appears in the invoice.</param>
        /// <param name="options">Invoice creation options.</param>
        /// <returns>
        ///   The lightning invoice just created
        /// </returns>
        Task<LightningInvoice> CreateInvoice(long satoshis, string description, CreateInvoiceOptions options = null);

        /// <summary>Checks the payment of an invoice.</summary>
        /// <param name="invoiceId">The invoice identifier.</param>
        /// <returns>
        ///  True of the invoice is paid, false otherwise 
        /// </returns>
        Task<bool> CheckPayment(string invoiceId);

        /// <summary>Pay.</summary>
        /// <param name="paymentRequest">The payment request (aka bolt11).</param>
        /// <returns>
        ///    True on the payment success, false otherwise
        /// </returns>
        Task<bool> Pay(string paymentRequest);
    }
}