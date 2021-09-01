/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.VDE.Communication
{
    class Connection
    {
        Log log;
        Data data;
        HubConnection connection;
        bool stayConnected = true;
        CancellationTokenSource cancellationToken;
        public Connection(Data data)
        {
            this.data = data;
            log = new Log("Connection",data.messenger);
            log.Entry("starting up");
            ServicePointManager.ServerCertificateValidationCallback = ValidateCert;
            Connect();
        }
        async void Connect()
        {
            try
            {
                connection = new HubConnectionBuilder()
                    .WithUrl(data.VDE.serverURL)
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(LogLevel.Warning);
                        logging.AddProvider(new UnityLogger());
                    })
                    .Build();

                connection.HandshakeTimeout = new TimeSpan(0,2,0);
                connection.ServerTimeout = new TimeSpan(0, 10, 0);
                await connection.StartAsync();
                cancellationToken = new CancellationTokenSource();
                connection.Closed += e => 
                {
                    if (stayConnected && data.forrestIsRunning)
                    {
                        data.messenger.Post(new Message()
                        {
                            LogingEvent = Log.Event.HUD,
                            message = "Connection closed with error: " + e.Message
                        });
                        Connect();
                    }
                    return Task.CompletedTask;
                };
                await connection.InvokeAsync("RegisterClient", System.Environment.MachineName);

                connection.On<string>("ReceiveConfiguration", ReceiveConfiguration);
                connection.On<string, string>("ReceiveJson", ReceiveJson);
                connection.On<string>("ReceiveEntities", ReceiveEntities);
                connection.On<string>("ReceiveRelations", ReceiveRelations);
                connection.On("ViewChanged", PrepareForFreshInputFromServer);

                connection.On<string, string>("NewHeadsetInTown", ReceiveMessage);
                connection.On<string>("NewHeadsetInTown", ReceiveMessage);
                connection.On("NewHeadsetInTown", ReceiveMessage);

                connection.On<string, string>("ReceiveMessage", ReceiveMessage);
                connection.On<string>("ReceiveMessage", ReceiveMessage);
                connection.On("ReceiveMessage", ReceiveMessage);

                if (data.config is null || data.config.VDE is null || data.config.VDE.Count < 1)
                {
                    await connection.InvokeAsync("RequestConfiguration");
                }
                if (!data.VDE.backendWithBakedData)
                {
                    await connection.InvokeAsync("GetEntities");
                }

                StreamEntities();
                StreamLinks();

                _ = Task.Run(() => TickerAsync());
            }
            catch (Exception exe)
            {
                log.Entry("Error connecting to " + data.VDE.serverURL + ": " + exe.Message, Log.Event.HUD);
                if (stayConnected && data.forrestIsRunning)
                {
                    await Task.Delay(new TimeSpan(0, 0, 5));
                    Connect();
                }
            }
        }

        private void PrepareForFreshInputFromServer()
        {
            data.links.UnHighlightLinks();
        }

        private async Task TickerAsync()
        {
            int ticker = 100;
            while (!(connection is null) && connection.State != HubConnectionState.Disconnected)
            {
                if (ticker == 0)
                {
                    ticker = 100;
                }
                SendTelemetry(new Telemetry()
                {
                    type = Telemetry.Type.progress,
                    progress = new List<Progress>
                {
                    new Progress()
                    {
                        name = "ticks",
                        description = "Ticker",
                        ints = new int[] { ticker--, 100 }
                    }
                }
                });
                await data.UI.Sleep(2345);
            }
        }

        internal void Disconnect()
        {
            connection.DisposeAsync();
        }

        /// <summary>
        /// 
        /// 20200106. temporary hacks are the most permanent ones, right. 
        /// headsets, browser and server should be identifying eachother based on CA maintained by the VDE server instance.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        static bool ValidateCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        internal async void SendTelemetry(Telemetry message)
        {
            try
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    await connection.InvokeAsync("ReceiveTelemetryJson", JsonConvert.SerializeObject(message));
                }
            }
            catch (Exception exe)
            {
                data.messenger.Post(new Message()
                {
                    LogingEvent = Log.Event.HUD,
                    message = "Unable to send telemetry: " + exe.Message
                });
            }
        }
        internal async void SendEntities(List<Entities.ExportEntity> message)
        {
            try
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    await connection.InvokeAsync("ReceiveEntityPositionJson", JsonConvert.SerializeObject(message));
                }
            }
            catch (Exception exe)
            {
                data.messenger.Post(new Message()
                {
                    LogingEvent = Log.Event.HUD,
                    message = "Unable to send entity positions: " + exe.Message
                });
            }
        }
        void ReceiveJson(string json, string name)
        {
            switch (name)
            {
                case "stats":
                    {
                        Dictionary<string,float> importStats = JsonConvert.DeserializeObject<Dictionary<string, float>>(json);
                        foreach (KeyValuePair<string,float> stat in importStats)
                        {
                            data.stats.AddOrUpdate(stat.Key, stat.Value, (key, existingVal) => {
                                existingVal = stat.Value;
                                return existingVal;
                            });
                        }
                    }
                    break;
                default:
                    log.Entry("dont know how to handle a " + name, Log.Event.ToServer);
                    break;
            }
        }
        void ReceiveConfiguration(string config)
        {
            if (data.VDE.cacheResponseFromServer)
            {
                System.IO.File.WriteAllText("c:\\data\\config.json", config);
            }
            data.config = JsonConvert.DeserializeObject<Config>(config);
            data.layouts.InitializeLayouts();
        }
        void ReceiveMessage(string from, string message)
        {
            log.Entry("Message from: " + from + " saying: " + message);
        }
        void ReceiveMessage(string message)
        {
            log.Entry("Anonymous message: " + message);
        }
        /// <summary>
        /// a stub, as SignalR defaults to this in some cases and will whine if that function doesnt exist.
        /// </summary>
        void ReceiveMessage() { }

        async void ReceiveEntities(string entities)
        {
            log.Entry("got " + entities.Length + " bytes of entities", Log.Event.ToServer);
            if (data.VDE.cacheResponseFromServer)
            {
                System.IO.File.WriteAllText("c:\\data\\entities.json", entities);
            }
            data.entities.Import(entities);
            //StreamEntities();
            await connection.InvokeAsync("GetRelations");
        }
        void ReceiveRelations(string links)
        {
            if (!(links is null) && links.Length > 0)
            {
                if (data.VDE.cacheResponseFromServer)
                {
                    System.IO.File.AppendAllText("c:\\data\\links.json", links);
                }
                log.Entry("got relations to be imported: " + links.Substring(0, Math.Min(links.Length, 666)), Log.Event.ToServer);
                data.links.Import(links);
            }
            else
            {
                log.Entry("server sent 0 relations", Log.Event.ToServer);
            }
        }
        async void StreamEntities()
        {
            var chanell = await connection.StreamAsChannelAsync<Entity>("StreamEntities");
            try
            {
                log.Entry("connection to the server for StreamEntities has been established. ", Log.Event.ToServer);
                while (!cancellationToken.IsCancellationRequested && data.forrestIsRunning && await chanell.WaitToReadAsync())
                {
                    while (chanell.TryRead(out Entity importedEntity))
                    {
                        data.entities.AddOrUpdate(importedEntity);
                    }
                }
            }
            catch (System.Net.WebSockets.WebSocketException exe)
            {
                log.Entry("connection to the server was severed because: " + exe.Message, Log.Event.HUD);
            }
            catch (OperationCanceledException exe)
            {
                log.Entry("connection to the server was severed because: " + exe.Message, Log.Event.HUD);
            }
        }
        async void StreamLinks()
        {
            var chanell = await connection.StreamAsChannelAsync<UI.ImportLink>("StreamLinks");
            try
            {
                log.Entry("connection to the server for StreamLinks has been established. ", Log.Event.ToServer);
                while (!cancellationToken.IsCancellationRequested && data.forrestIsRunning && await chanell.WaitToReadAsync())
                {
                    while (chanell.TryRead(out UI.ImportLink incomingLink))
                    {
                        _ = Task.Run(() => data.links.LinkEntities(incomingLink.s, incomingLink.d, incomingLink.w, data.links.links.OrderByDescending(imp => imp.weight).First().weight, true));
                    }
                }
            }
            catch (System.Net.WebSockets.WebSocketException exe)
            {
                log.Entry("connection to the server was severed because: " + exe.Message, Log.Event.HUD);
            }
            catch (OperationCanceledException exe)
            {
                log.Entry("connection to the server was severed because: " + exe.Message, Log.Event.HUD);
            }
        }
    }
}
