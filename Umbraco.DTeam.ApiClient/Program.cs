using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using Umbraco.DTeam.Core.Auth;

namespace Umbraco.DTeam.ApiClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage!");
                return;
            }

            var apiKey = args[0];
            var request = new HttpRequestMessage(HttpMethod.Post, args[1].TrimEnd('/') + "/Umbraco/BackOffice/DTeam/Api/CaptureProgress");

            var requestPath = request.RequestUri.CleanPathAndQuery();
            var timestamp = DateTime.UtcNow;
            var nonce = Guid.NewGuid();

            var signature = HmacAuthentication.GetSignature(requestPath, timestamp, nonce, apiKey);
            var headerToken = HmacAuthentication.GenerateAuthorizationHeader(signature, nonce, timestamp);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", headerToken);

            var httpClient = new HttpClient();

            var result = httpClient.SendAsync(request, CancellationToken.None).Result;
            Console.WriteLine(result.StatusCode);

            if (result.StatusCode != HttpStatusCode.OK) return;

            var formatter = new JsonMediaTypeFormatter();
            var formatters = new[] { formatter };
            var datetime = result.Content.ReadAsAsync<DateTime>(formatters, CancellationToken.None).Result;

            //Console.WriteLine(datetime.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss K"));
            Console.WriteLine(datetime.ToString("yyyy/MM/dd HH:mm:ss K"));
        }
    }
}
