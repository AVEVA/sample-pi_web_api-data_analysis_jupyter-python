using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UploadUtility
{
    public class PIWebAPIClient : IDisposable
    {
        private readonly HttpClient _client;

        public PIWebAPIClient()
        {
            using var handler = new HttpClientHandler() { UseDefaultCredentials = true };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Add("X-Requested-With", "xhr");
        }

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

        public async Task<JObject> GetAsync(string path)
        {
            if (!Uri.TryCreate(_client.BaseAddress, path, out Uri newUri))
                throw new Exception($"Invalid input for uri {path}");

            if (!_client.BaseAddress.IsBaseOf(newUri))
                throw new Exception($"Base uri has been modified!");

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

        public async Task PostAsync(string path, string data)
        {
            if (!Uri.TryCreate(_client.BaseAddress, path, out Uri newUri))
                throw new Exception($"Invalid input for uri {path}");

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

        public async Task PostXmlAsync(string path, string data)
        {
            if (!Uri.TryCreate(_client.BaseAddress, path, out Uri newUri))
                throw new Exception($"Invalid input for uri {path}");

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

        public async Task DeleteAsync(string path)
        {
            if (!Uri.TryCreate(_client.BaseAddress, path, out Uri newUri))
                throw new Exception($"Invalid input for uri {path}");

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

        public JObject GetRequest(string path)
        {
            Task<JObject> t = GetAsync(path);
            t.Wait();
            return t.Result;
        }

        public void PostRequest(string path, string data, bool isXML = false)
        {
            if (isXML)
            {
                Task t = PostXmlAsync(path, data);
                t.Wait();
            }
            else
            {
                Task t = PostAsync(path, data);
                t.Wait();
            }
        }

        public void DeleteRequest(string path)
        {
            Task t = DeleteAsync(path);
            t.Wait();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }
    }
}
