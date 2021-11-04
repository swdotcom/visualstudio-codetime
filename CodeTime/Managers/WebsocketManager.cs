﻿
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace CodeTime
{
    internal class WebsocketManager
    {
        private static readonly ManualResetEvent ExitEvent = new ManualResetEvent(false);
        private static bool initialized = false;
        private static int keepAliveSeconds = 60 * 3;

        public static void Initialize(bool re_initialize = false)
        {
            if (initialized && !re_initialize) {
                return;
            }
            initialized = true;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;

            var factory = new Func<ClientWebSocket>(() =>
            {
                var client = new ClientWebSocket
                {
                    Options =
                    {
                        KeepAliveInterval = TimeSpan.FromSeconds(keepAliveSeconds),
                    }
                };
                client.Options.SetRequestHeader("Authorization", FileManager.getItemAsString("jwt"));
                return client;
            });

            var url = new Uri("wss://api.software.com/websockets");

            using (IWebsocketClient client = new WebsocketClient(url, factory))
            {
                client.Name = "SoftwareClient";
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(type =>
                {
                    LogManager.Info($"Code Time: Reconnecting the websocket connection, url: {client.Url}");
                });
                client.DisconnectionHappened.Subscribe(info =>
                {
                    LogManager.Warning($"Code Time: Websocket connection was disconnected");
                });

                client.MessageReceived.Subscribe(msg =>
                {
                    LogManager.Info($"Message received: {msg}");
                    JObject jsonObj = JsonConvert.DeserializeObject<JObject>(msg.ToString());
                    JToken body = jsonObj.GetValue("body");
                    
                    switch (jsonObj.GetValue("type").ToString())
                    {
                        case "flow_state":
                            FlowStateHandler(body);
                            break;
                        case "flow_score":
                            FlowScoreHandler();
                            break;
                        case "authenticated_plugin_user":
                            AuthenticatedPluginUserHandler(body);
                            break;
                        case "current_day_stats_update":
                            CurrentDayStatsUpdateHandler(body);
                            break;
                        case "user_integration_connection":
                            UserIntegrationConnectionHandler(body);
                            break;
                    }
                });

                LogManager.Info("Starting Code Time websocket...");
                client.Start().Wait();
                LogManager.Info("Code Time websocket started.");

                Task.Run(() => StartSendingPing(client));

                ExitEvent.WaitOne();
            }

            LogManager.Info("Code Time websocket stopped.");
        }

        private static async Task StartSendingPing(IWebsocketClient client)
        {
            while (true)
            {
                await Task.Delay(1000 * 60);

                if (!client.IsRunning)
                {
                    continue;
                }

                client.Send("ping");
            }
        }

        private static void CurrentDomainOnProcessExit(object sender, EventArgs eventArgs)
        {
            LogManager.Warning("Exiting process");
            ExitEvent.Set();
        }

        private static void FlowScoreHandler()
        {
            FlowManager.EnableFlow(true);
        }

        private static void FlowStateHandler(JToken body)
        {
            try
            {
                bool enable_flow = (bool)body.ToObject<JObject>().GetValue("enable_flow");
                if (!enable_flow)
                {
                    FlowManager.DisableFlow();
                }
            } catch (Exception ex)
            {
                LogManager.Error("Error updating flow state", ex);
            }
        }

        private static void AuthenticatedPluginUserHandler(JToken body)
        {
            try
            {
                SoftwareUser user = JsonConvert.DeserializeObject<SoftwareUser>(body.ToString());
                UserManager.AuthenticationCompleteHandler(user);
            } catch (Exception ex)
            {
                LogManager.Error("Error handling authenticated user event", ex);
            }
        }

        private static void CurrentDayStatsUpdateHandler(JToken body)
        {
            try
            {
                SessionSummary sessionSummary = JsonConvert.DeserializeObject<SessionSummary>(body.ToObject<JObject>().GetValue("data").ToString());
                _ = SessionSummaryManager.UpdateStatusBarWithSummaryDataAsync(sessionSummary);
            } catch (Exception ex)
            {
                LogManager.Error("Error updating current day stats", ex);
            }
        }

        private static void UserIntegrationConnectionHandler(JToken body)
        {
            _ = UserManager.GetUser();
        }
    }
}