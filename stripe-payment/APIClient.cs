using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StripeExample
{
    public class APIClient
    {
        #region Variables
        private IHttpContextAccessor _httpContextAccessor;
        private HttpClient _httpClient;
        private string _bearerAccessToken;
        private List<string> _knownHeaderKeys = new List<string>
        {
            "ActionsGUIDs",
            "Accept-Language"
        };
        #endregion

        #region Properties
        public string BaseAddress { get; set; }
        #endregion

        #region Constructors
        public APIClient(string baseAddress)
        {
            this.BaseAddress = baseAddress;
        }
        public APIClient(string baseAddress, IHttpContextAccessor httpContextAccessor)
        {
            this.BaseAddress = baseAddress;
            this._httpContextAccessor = httpContextAccessor;
        }

        public APIClient(HttpClient httpClient)
        {
            this.BaseAddress = string.Empty;
            this._httpClient = httpClient;
        }

        public APIClient(string baseAddress, HttpClient httpClient)
        {
            this.BaseAddress = baseAddress;
            this._httpClient = httpClient;
        }
        #endregion

        #region Methods
        public Task<T> GetAsync<T>()
        {
            return this.GetAsync<T>(string.Empty);
        }

        public Task<T> GetAsync<T>(string uri, List<KeyValuePair<string, string>> headers = null)
        {
            return this.GetAsync<T>(uri, TimeSpan.FromSeconds(100), headers);
        }

        public Task<T> GetAsync<T>(string uri, TimeSpan timeout, List<KeyValuePair<string, string>> headers = null)
        {
            Func<HttpResponseMessage, Task<T>> continueFunc = (response) =>
            {
                if (response.Content.Headers.ContentType != null && response.Content.Headers.ContentType.MediaType == "application/octet-stream")
                {
                    return response.Content.ReadAsByteArrayAsync().ContinueWith<T>(c =>
                    {
                        return (T)(object)c.Result;
                    });
                }

                return response.Content.ReadAsStringAsync().ContinueWith<T>(c =>
                {
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(c.Result as object);
                    }

                    return JsonConvert.DeserializeObject<T>(c.Result);
                });
            };

            return this.InvokeAsync<T>(client =>
            {
                if (headers != null)
                {
                    headers.ForEach(h =>
                    {
                        client.DefaultRequestHeaders.Add(h.Key, h.Value);
                    });
                }

                client.Timeout = timeout;
                return client.GetAsync(uri);
            }, continueFunc);
        }

        public Task<byte[]> GetAsByteArrayAsync(string uri)
        {
            return this.GetAsByteArrayAsync(uri, TimeSpan.FromSeconds(100));
        }

        public Task<byte[]> GetAsByteArrayAsync(string uri, TimeSpan timeout)
        {
            Func<HttpResponseMessage, Task<byte[]>> continueFunc = (response) => response.Content.ReadAsByteArrayAsync();

            return this.InvokeAsync(client =>
            {
                client.Timeout = timeout;
                return client.GetAsync(uri);
            }, continueFunc);
        }

        public Task<string> GetAsStringAsync(string uri)
        {
            return this.InvokeAsync(client => client.GetAsync(uri),
                response => response.Content.ReadAsStringAsync());
        }

        public Task<T> PostAsJsonAsync<T>(object data, string uri, TimeSpan timeout)
        {
            return this.InvokeAsync(client =>
            {
                client.Timeout = timeout;
                return client.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
            },
                response => response.Content.ReadAsStringAsync().ContinueWith<T>(c =>
                {
                    return JsonConvert.DeserializeObject<T>(c.Result);
                })
            );
        }

        public Task<T> PostAsJsonAsync<T>(object data, string uri)
        {
            return this.InvokeAsync(client =>
            {
                return client.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
            },
                response => response.Content.ReadAsStringAsync().ContinueWith<T>(c =>
                {
                    return JsonConvert.DeserializeObject<T>(c.Result);
                })
            );
        }

        public Task PostAsJsonAsync(object data, string uri)
        {
            return this.InvokeAsync<object>(client => client.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")));
        }

        public Task<string> PostAsStringAsync(string data, string uri, string contentType)
        {
            return this.InvokeAsync(client =>
            {
                return client.PostAsync(uri, new StringContent(data, Encoding.UTF8, contentType));
            },
                response => response.Content.ReadAsStringAsync()
            );
        }


        public Task PutAsJsonAsync(object data, string uri)
        {
            return this.InvokeAsync<object>(client => client.PutAsync(uri, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")));
        }

        public Task<T> PutAsJsonAsync<T>(object data, string uri)
        {
            return this.InvokeAsync(client =>
            {
                return client.PutAsync(uri, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
            },
                response => response.Content.ReadAsStringAsync()
                    .ContinueWith<T>(c =>
                    {
                        return JsonConvert.DeserializeObject<T>(c.Result);
                    }));
        }

        public Task<T> PutAsJsonAsync<T>(object data, string uri, TimeSpan timeout)
        {
            return this.InvokeAsync(client =>
            {
                client.Timeout = timeout;
                return client.PutAsync(uri, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
            },
                response => response.Content.ReadAsStringAsync()
                    .ContinueWith<T>(c =>
                    {
                        return JsonConvert.DeserializeObject<T>(c.Result);
                    }));
        }
        public Task<string> PutAsStringAsync(string data, string uri, string contentType)
        {
            return this.InvokeAsync(client =>
            {
                return client.PutAsync(uri, new StringContent(data, Encoding.UTF8, contentType));
            },
                response => response.Content.ReadAsStringAsync()
            );
        }

        public Task<T> DeleteAsJsonAsync<T>(string uri)
        {
            return this.InvokeAsync(client => client.DeleteAsync(uri),
                r => r.Content.ReadAsStringAsync().ContinueWith<T>(c =>
                {
                    return JsonConvert.DeserializeObject<T>(c.Result);
                }));
        }

        public Task<string> DeleteAsStringAsync(string uri)
        {
            return this.InvokeAsync(client =>
            {
                return client.DeleteAsync(uri);
            },
                response => response.Content.ReadAsStringAsync()
            );
        }

        public Task DeleteAsync(string uri)
        {
            return this.InvokeAsync<object>(client => client.DeleteAsync(uri));
        }


        public Task PatchAsync(object data, string uri)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = new HttpMethod("PATCH"),
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json-patch+json")
            };

            return this.InvokeAsync<object>(client => client.SendAsync(request));
        }

        public Task<T> DeleteAsJsonAsync<T>(object data, string uri)
        {
            return this.InvokeAsync(client => client.SendAsync(
                new HttpRequestMessage(HttpMethod.Delete, uri)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
                }),
                response => response.Content.ReadAsStringAsync().ContinueWith<T>(c =>
                {
                    return JsonConvert.DeserializeObject<T>(c.Result);
                }));
        }

        public APIClient UseAccessToken(string accessToken)
        {
            this._bearerAccessToken = accessToken;
            return this;
        }
        #endregion

        #region Helpers

        private void AppendKnownHeaders(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            this._knownHeaderKeys.ForEach(ch =>
            {
                string value = httpContextAccessor?.HttpContext?.Request?.Headers[ch];
                if (!string.IsNullOrEmpty(value))
                {
                    client.DefaultRequestHeaders.Add(ch, value);
                }
            });
        }

        private async Task<T> InvokeAsync<T>(Func<HttpClient, Task<HttpResponseMessage>> operation,
            Func<HttpResponseMessage, Task<T>> actionOnResponse = null)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            HttpClient client;

            if (this._httpClient == null)
            {
                client = new HttpClient();
                client.BaseAddress = new Uri(this.BaseAddress);
            }
            else
            {
                client = this._httpClient;
            }

            try
            {
                // Pass the token if exists
                string token = this._bearerAccessToken;
                //if (string.IsNullOrWhiteSpace(token))
                //{
                //    token = (this._httpContextAccessor != null && this._httpContextAccessor.HttpContext != null) ? this.getTokenFromRequest(this._httpContextAccessor.HttpContext.Request) : string.Empty;
                //}
                //if (!string.IsNullOrEmpty(token))
                //{
                //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                //}


                //this.AppendKnownHeaders(client, this._httpContextAccessor);

                HttpResponseMessage response = await operation(client);

                if (!response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    throw new InvalidOperationException($"Request failed : {response.StatusCode} - Content: {content}");
                }
                if (actionOnResponse != null)
                {
                    return await actionOnResponse(response);
                }

                return default(T);
            }
            finally
            {
                if (this._httpClient == null)
                {
                    client.Dispose();
                }
            }
        }
        #endregion
    }
}
