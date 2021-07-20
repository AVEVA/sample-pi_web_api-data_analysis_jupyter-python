using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UploadUtility
{
    /// <summary>
    /// PI Web API Client
    /// </summary>
    public class PIWebAPIClient : IDisposable
    {
        private readonly HttpClient _client;

        /// <summary>
        /// Basic constructor for PI Web API Client using default credentials
        /// </summary>
        public PIWebAPIClient()
        {
            using var handler = new HttpClientHandler() { UseDefaultCredentials = true };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Add("X-Requested-With", "xhr");
        }

        /// <summary>
        /// Constructor for PI Web API Client using specific user credentials
        /// </summary>
        public PIWebAPIClient(string baseAddress, string username, string password)
        {
            _client = new HttpClient();

            // Base address must end with a '/'
            if (baseAddress[^1] != '/')
            {
                baseAddress += "/";
            }

            _client.BaseAddress = new Uri(baseAddress);
            string creds = Convert.ToBase64String(
                Encoding.ASCII.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}:{1}", username, password)));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", creds);
            _client.DefaultRequestHeaders.Add("X-Requested-With", "xhr");
        }

        /// <summary>
        /// Runs an async GET request at the specified url endpoint
        /// </summary>
        public async Task<JObject> GetAsync(Uri uri)
        {
            if (!Uri.TryCreate(_client.BaseAddress, uri, out Uri newUri))
                throw new Exception($"Invalid input for uri {uri}");

            if (!_client.BaseAddress.IsBaseOf(newUri))
                throw new Exception($"Base uri has been modified!");

            if (uri != null)
            {
                Console.WriteLine(uri.ToString());
            }

            Console.WriteLine(newUri.ToString());
            HttpResponseMessage response = await _client.GetAsync(newUri).ConfigureAwait(false);

            Console.WriteLine("GET response code " + response.StatusCode);
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var responseMessage = "Response status code does not indicate success: " + (int)response.StatusCode + " (" + response.StatusCode + " ). ";
                throw new HttpRequestException(responseMessage + Environment.NewLine + content);
            }

            return JObject.Parse(content);
        }

        /// <summary>
        /// Runs an async POST request at the specified url endpoint
        /// </summary>
        public async Task PostAsync(Uri uri, string data)
        {
            if (!Uri.TryCreate(_client.BaseAddress, uri, out Uri newUri))
                throw new Exception($"Invalid input for uri {uri}");

            if (!_client.BaseAddress.IsBaseOf(newUri))
                throw new Exception($"Base uri has been modified!");

            using var postContent = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(
                newUri, postContent).ConfigureAwait(false);

            Console.WriteLine("POST response code " + response.StatusCode);
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var responseMessage = "Response status code does not indicate success: " + (int)response.StatusCode + " (" + response.StatusCode + " ). ";
                throw new HttpRequestException(responseMessage + Environment.NewLine + content);
            }
        }

        /// <summary>
        /// Runs an async POST request using XML data at the specified url endpoint
        /// </summary>
        public async Task PostXmlAsync(Uri uri, string data)
        {
            if (!Uri.TryCreate(_client.BaseAddress, uri, out Uri newUri))
                throw new Exception($"Invalid input for uri {uri}");

            if (!_client.BaseAddress.IsBaseOf(newUri))
                throw new Exception($"Base uri has been modified!");

            using var postContent = new StringContent(data, Encoding.UTF8, "text/xml");
            HttpResponseMessage response = await _client.PostAsync(
                newUri, postContent).ConfigureAwait(false);

            Console.WriteLine("GET response code " + response.StatusCode);
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var responseMessage = "Response status code does not indicate success: " + (int)response.StatusCode + " (" + response.StatusCode + " ). ";
                throw new HttpRequestException(responseMessage + Environment.NewLine + content);
            }
        }

        /// <summary>
        /// Runs an async DELETE request at the specified url endpoint
        /// </summary>
        public async Task DeleteAsync(Uri uri)
        {
            if (!Uri.TryCreate(_client.BaseAddress, uri, out Uri newUri))
                throw new Exception($"Invalid input for uri {uri}");

            if (!_client.BaseAddress.IsBaseOf(newUri))
                throw new Exception($"Base uri has been modified!");

            HttpResponseMessage response = await _client.DeleteAsync(newUri).ConfigureAwait(false);
            Console.WriteLine("DELETE response code " + response.StatusCode);
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var responseMessage = "Response status code does not indicate success: " + (int)response.StatusCode + " (" + response.StatusCode + " ). ";
                throw new HttpRequestException(responseMessage + Environment.NewLine + content);
            }
        }

        /// <summary>
        /// Run a GET request at the specified url endpoint
        /// </summary>
        public JObject GetRequest(Uri uri)
        {
            Task<JObject> t = GetAsync(uri);
            t.Wait();
            return t.Result;
        }

        /// <summary>
        /// Run a POST request at the specified url endpoint
        /// </summary>
        public void PostRequest(Uri uri, string data, bool isXML = false)
        {
            if (isXML)
            {
                Task t = PostXmlAsync(uri, data);
                t.Wait();
            }
            else
            {
                Task t = PostAsync(uri, data);
                t.Wait();
            }
        }

        /// <summary>
        /// Run a DELETE request at the specified url endpoint
        /// </summary>
        public void DeleteRequest(Uri uri)
        {
            Task t = DeleteAsync(uri);
            t.Wait();
        }

        /// <summary>
        /// Clean up disposable resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up disposable resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }
    }
}
