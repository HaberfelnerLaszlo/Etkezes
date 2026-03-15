using Etkezes_Models;

using Newtonsoft.Json;

using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Etkezes_Ellenor.Services
{
    public class ApiHelper
    {
        public string ErrorMessage { get; set; } = String.Empty;
        private static readonly HttpClient client = new();
        //private static readonly string[] scopes = ["api://6091ad40-f274-4e3a-813b-a9498817fd69/access_as_user"];

        //protected static string URI = "https://localhost:7192/";
        protected static string URI = "http://192.168.10.66:5000/";
        //private string? _token;
        //private readonly DateTime _tokenExpiration;
        //private async Task CreateAuthorizationHeaderForUserAsync()
        //{
        //    _token = await authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);

        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _token.Substring(6));
        //}
        public event EventHandler<ErrorMessageEventArg>? OnErrorMessage;
        public async Task<Tv?> Get<Tv>(string uri)
        {
            try
            {
                client.BaseAddress ??= new Uri(URI);
                //if (string.IsNullOrEmpty(_token) && !uri.Contains("Math"))
                //{
                //    await CreateAuthorizationHeaderForUserAsync();
                //}
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                using HttpResponseMessage response = await client.GetAsync(uri);
                string? valasz;
                if (response.IsSuccessStatusCode)
                {
                    valasz = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(valasz) && valasz.Contains("Message"))
                    {
                        var deserilizeResponse = JsonConvert.DeserializeObject<MainResponse>(valasz);
                        if (deserilizeResponse != null)
                        {
                            if (deserilizeResponse.Success)
                            {
                                return JsonConvert.DeserializeObject<Tv>(deserilizeResponse.Data?.ToString() ?? string.Empty);
                            }
                        }
                    }
                    return JsonConvert.DeserializeObject<Tv>(valasz);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    valasz = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(valasz) && valasz.Contains("Message"))
                    {
                        var r = JsonConvert.DeserializeObject<MainResponse>(valasz);
                        if (r?.Success ?? false)
                        {
                            ErrorMessage = r.Message;
                            OnErrorMessage?.Invoke(this, new(response.StatusCode.ToString(), r.Message));
                        }
                        else ErrorMessage = r?.Message ?? "Null eredmény lett a hiba üzenet";
                    }
                    else ErrorMessage = "Nem elérhető a szerver!";
                    return default;
                }
                else
                {
                    ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
                    OnErrorMessage?.Invoke(this, new(response.StatusCode.ToString(), ErrorMessage));
                    return default;
                }
            }
            catch (HttpRequestException e)
            {
                //client.CancelPendingRequests();
                //client.Dispose();
                ErrorMessage = "Nem elérhető a szerver! Hibaüzenet: " + e.Message;
                OnErrorMessage?.Invoke(this, new(e.StatusCode.ToString(), e.Message));
                return default;
            }
            catch (Exception e)
            {
                client.CancelPendingRequests();
                //client.Dispose();
                ErrorMessage = "Hiba történt! Az üzenet: " + e.Message;
                OnErrorMessage?.Invoke(this, new(e.HResult.ToString(), e.Message));
                return default;
            }
        }

        public async Task<T?> Post<T>(string uri, T t)
        {
            var valasz = string.Empty;
            //var token = await authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);

            //if (string.IsNullOrEmpty(_token) && !uri.Contains("Math"))
            //{
            //    await CreateAuthorizationHeaderForUserAsync();
            //}
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.PostAsJsonAsync<T>(uri, t);
            if (response.IsSuccessStatusCode)
            {
                valasz = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(valasz) && valasz.Contains("Message", StringComparison.OrdinalIgnoreCase))
                {
                    var deserilizeResponse = JsonConvert.DeserializeObject<MainResponse>(valasz);
                    if (deserilizeResponse != null)
                    {
                        if (deserilizeResponse.Success)
                        {
                            return JsonConvert.DeserializeObject<T>(deserilizeResponse.Data?.ToString() ?? string.Empty);
                        }
                    }
                }
                return JsonConvert.DeserializeObject<T?>(valasz);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                valasz = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(valasz) && valasz.Contains("Message"))
                {
                    var r = JsonConvert.DeserializeObject<MainResponse>(valasz);
                    ErrorMessage = r?.Message ?? "Null eredmény lett a hiba üzenet";
                }
                else ErrorMessage += "Nem elérhető a szerver!";
                return default;
            }
            else
            {
                ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
                var message = await response.Content.ReadAsStringAsync();
                ErrorMessage += message;
            }
            return default;
        }

        public async Task<T?> Put<T>(string uri, T t)
        {
            //var token = await authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);
            //if (string.IsNullOrEmpty(_token) && !uri.Contains("Math"))
            //{
            //    await CreateAuthorizationHeaderForUserAsync();
            //}
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.PutAsJsonAsync<T>(uri, t);
            string valasz;
            if (response.IsSuccessStatusCode)
            {
                valasz = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(valasz) && valasz.Contains("Message"))
                {
                    var deserilizeResponse = JsonConvert.DeserializeObject<MainResponse>(valasz);
                    if (deserilizeResponse != null)
                    {
                        if (deserilizeResponse.Success)
                        {
                            return JsonConvert.DeserializeObject<T>(deserilizeResponse.Data?.ToString()??string.Empty);
                        }
                    }
                }
                return JsonConvert.DeserializeObject<T>(valasz);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                valasz = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(valasz) && valasz.Contains("Message"))
                {
                    var r = JsonConvert.DeserializeObject<MainResponse>(valasz);
                    ErrorMessage = r?.Message ?? "Null eredmény lett a hiba üzenet";
                }
                else ErrorMessage += "Nem elérhető a szerver!";
                return default;
            }
            else
            {
                ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
                var message = await response.Content.ReadAsStringAsync();
                ErrorMessage += message;
            }
            return default;
        }

        public async Task<bool> Delete(string uri)
        {
            bool valasz = false;
            //var token = await authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);
            //if (string.IsNullOrEmpty(_token) && !uri.Contains("Math"))
            //{
            //    await CreateAuthorizationHeaderForUserAsync();
            //}
            client.BaseAddress ??= new Uri(URI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.DeleteAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                valasz = true;
            }
            else ErrorMessage = response.StatusCode.ToString() + " : " + response.RequestMessage;
            return valasz;
        }
    }
    //public class TokenResponse
    //{
    //    public string? Token { get; set; }
    //    public int ExpiresIn { get; set; }
    //}
    public class ErrorMessageEventArg : EventArgs
    {
        public string? ErrorCode { get; set; }
        public string? Message { get; set; }
        public ErrorMessageEventArg(string? errorCode, string? message)
        {
            ErrorCode = errorCode;
            Message = message;
        }
    }
}
