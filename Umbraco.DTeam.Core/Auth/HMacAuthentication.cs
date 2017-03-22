﻿using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Umbraco.DTeam.Core.Auth
{
    public class HmacAuthentication
    {
        public static string GetSignature(string requestUri, DateTime timestamp, Guid nonce, string secret)
        {
            return GetSignature(requestUri, timestamp.ToUnixTimestamp().ToString(CultureInfo.InvariantCulture), nonce.ToString(), secret);
        }

        public static string GetSignature(string requestUri, string timestamp, string nonce, string secret)
        {
            var secretBytes = Encoding.UTF8.GetBytes(secret);

            using (var hmac = new HMACSHA256(secretBytes))
            {
                var signatureString = $"{requestUri}{timestamp}{nonce}";
                var signatureBytes = Encoding.UTF8.GetBytes(signatureString);
                var computedHashBytes = hmac.ComputeHash(signatureBytes);
                var computedString = Convert.ToBase64String(computedHashBytes);
                return computedString;
            }
        }

        /// <summary>
        /// Returns the token authorization header value as a base64 encoded string
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="nonce"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string GenerateAuthorizationHeader(string signature, Guid nonce, DateTime timestamp)
        {
            return
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        $"{signature}:{nonce}:{timestamp.ToUnixTimestamp().ToString(CultureInfo.InvariantCulture)}"));
        }
    }
}
