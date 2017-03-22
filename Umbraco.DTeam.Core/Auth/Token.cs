using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using Umbraco.Core.IO;

namespace Umbraco.DTeam.Core.Auth
{
    public class Token
    {
        private static string _apiKey;

        private static string ApiKey
        {
            get
            {
                if (_apiKey == null) ReadConfig();
                return _apiKey;
            }
        }

        private static void ReadConfig()
        {
            using (var stream = File.OpenRead(IOHelper.MapPath("~/App_Data/api.cfg")))
            using (var reader = new StreamReader(stream))
            {
                _apiKey = reader.ReadLine();
            }
        }

        internal static void ValidateToken(string token, string requestUrl, out DateTime timestamp)
        {
            var decodedToken = GetDecodedToken(token, requestUrl);

            //assign MinValue to expire immediately if the TryParse were to fail
            timestamp = DateTime.MinValue;
            double timeStampDouble;

            if (double.TryParse(decodedToken.Timestamp, NumberStyles.Any, CultureInfo.InvariantCulture, out timeStampDouble))
            {
                timestamp = timeStampDouble.FromUnixTime();
            }

            //generate a signature and verify it matches the token signature
            var validationSignature = HmacAuthentication.GetSignature(requestUrl, decodedToken.Timestamp, decodedToken.Nonce, ApiKey);
            if (validationSignature != decodedToken.RequestSignature)
                throw new UnauthorizedClientException("Authorization token signature is invalid");
        }

        private static DecodedToken GetDecodedToken(string token, string requestUrl)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            if (string.IsNullOrWhiteSpace(requestUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(requestUrl));

            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));

            //try getting the authorization header
            var headerParts = decoded.Split(':');
            if (headerParts.Length != 3)
                throw new UnauthorizedClientException("Authorization header is invalid");

            var decodedToken = new DecodedToken
            {
                RequestSignature = headerParts[0],
                Nonce = headerParts[1],
                Timestamp = headerParts[2]
            };

            return decodedToken;
        }

        internal class DecodedToken
        {
            public string RequestSignature { get; set; }
            public string Nonce { get; set; }
            public string Timestamp { get; set; }
        }
    }
}
