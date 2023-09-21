using CodeCodeChallenge.Tests.Integration.Helpers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CodeChallenge.Tests.Integration.Extensions
{
    internal static class HttpClientExtensions
    {
        /// <summary>
        /// PUT's a payload to a Uri, by converting to JSON.
        /// </summary>
        public static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient httpClient, string requestUri, T payload)
        {
            var requestContent = new JsonSerialization().ToJson(payload);
            return httpClient.PutAsync(requestUri,
                new StringContent(requestContent, Encoding.UTF8, "application/json"));
        }

        /// <summary>
        /// Reads the content as a string, synchronously.
        /// </summary>
        public static string ReadAsString(this HttpContent content)
            => content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}
