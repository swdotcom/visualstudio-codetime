using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeTime
{
    class HttpManager
    {
        public static bool IsOk(HttpResponseMessage response)
        {
            return (response != null && response.StatusCode == HttpStatusCode.OK);
        }

        public static async Task<HttpResponseMessage> AppRequest(HttpMethod httpMethod, string api, string optionalPayload = null)
        {
            return await RequestIt(httpMethod, Constants.app_endpoint + "" + api, optionalPayload);
        }

        public static async Task<HttpResponseMessage> MetricsRequest(HttpMethod httpMethod, string api, string optionalPayload = null, string override_jwt = null)
        {
            return await RequestIt(httpMethod, Constants.metrics_endpoint + "" + api, optionalPayload, override_jwt);
        }

        public static async Task<HttpResponseMessage> RequestIt
            (HttpMethod httpMethod, string api, string optionalPayload = null, string override_jwt = null)
        {

            HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            var cts = new CancellationTokenSource();
            HttpResponseMessage response = null;
            AddAuthorization(client, override_jwt);
            HttpContent contentPost = null;
            try
            {
                if (optionalPayload != null)
                {
                    contentPost = new StringContent(optionalPayload, Encoding.UTF8, "application/json");
                }
            }
            catch (Exception e)
            {
                NotifyPostException(e);
            }
            bool isPost = (httpMethod.Equals(HttpMethod.Post));
            try
            {
                if (httpMethod.Equals(HttpMethod.Post))
                {
                    response = await client.PostAsync(api, contentPost, cts.Token);
                }
                else if (httpMethod.Equals(HttpMethod.Get))
                {
                    response = await client.GetAsync(api, cts.Token);
                }
                else if (httpMethod.Equals(HttpMethod.Delete))
                {
                    response = await client.DeleteAsync(api, cts.Token);
                }
                else
                {
                    response = await client.PutAsync(api, contentPost, cts.Token);
                }
            }
            catch (HttpRequestException e)
            {
                if (isPost)
                {
                    NotifyPostException(e);
                }
            }
            catch (TaskCanceledException e)
            {
                if (e.CancellationToken == cts.Token)
                {
                    // triggered by the caller
                    if (isPost)
                    {
                        NotifyPostException(e);
                    }
                }
                else
                {
                    // a web request timeout (possibly other things!?)
                    LogManager.Info("We are having trouble receiving a response from Software.com");
                }
            }
            catch (Exception e)
            {
                if (isPost)
                {
                    NotifyPostException(e);
                }
            }
            finally
            {
            }
            return response;
        }

        private static void NotifyPostException(Exception e)
        {
            LogManager.Error("We are having trouble sending data to Software.com, reason: " + e.Message);
        }

        private static void AddAuthorization(HttpClient client, string override_jwt = null)
        {
            if (override_jwt == null)
            {
                string jwt = FileManager.getItemAsString("jwt");
                if (!String.IsNullOrEmpty(jwt))
                {
                    if (jwt.Contains("JWT "))
                    {
                        jwt = "Bearer " + jwt.Substring("JWT ".Length);
                    }
                    client.DefaultRequestHeaders.Add("Authorization", jwt);
                }
            } else
            {
                client.DefaultRequestHeaders.Add("Authorization", override_jwt);
            }
        }
    }
}
