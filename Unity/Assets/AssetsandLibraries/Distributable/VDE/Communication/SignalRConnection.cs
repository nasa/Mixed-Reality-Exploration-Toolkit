/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */

/*
 * This file has been modified from the standalone VDE to work in MRET while also living together 
 * with ROS https://github.com/siemens/ros-sharp (and UMP).
 * 
 * ROS depends on slightly newer .net DLL-s, that in itself do NOT rely on System.Runtime.dll. 
 * However, the .net DLL-s that VDE relies on AND that would be compatible with the newer .net 
 * version (that ROS depends on) DO require a newer System.Runtime.dll than the one Unity itself 
 * relies on (4.1.2.0 (which is mono, not MS!) vs 4.2.1.0).
 * 
 * Newer version System.Runtime.dll might work if IL2CPP backend could be used instead of mono, 
 * alas: UMP (universal media player) will not like that and UMP is required to function in MRET.
 * 
 * Hence the only way to get VDE, ROS and UMP to live together in MRET at the same time / build is 
 * to use https://assetstore.unity.com/packages/tools/network/best-http-2-155981 instead of MS .net DLL-s.
 * 
 * if you know of a better solution, please DO let me know.
 * 
 * 20211202 KK
 * 
 * as of MRET 2021.3 release the default VDE com method while running in MRET is GMSEC (4.9).
 * 
 * 20220213 KK
 */

#if MRET_2021_OR_LATER || DOTNETWINRT_PRESENT
#define USE_FAKE_SIGNALR
#endif

#if USE_FAKE_SIGNALR
using Assets.VDE.UI;
#if MRET_EXTENSION_BESTHTTP
//using BestHTTP.SignalR.JsonEncoders;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
#endif
#else
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
#endif
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
    class SignalRConnection
    {
        Log log;
        Data data;
#if (USE_FAKE_SIGNALR && MRET_EXTENSION_BESTHTTP) || !USE_FAKE_SIGNALR
        HubConnection connection;
#endif
        bool stayConnected = true;
        CancellationTokenSource cancellationToken;
        public SignalRConnection(Data data)
        {
            this.data = data;
            log = new Log("Connection",data.messenger);
            ServicePointManager.ServerCertificateValidationCallback = ValidateCert;
            Connect();
        }
        async void Connect()
        {
#if (USE_FAKE_SIGNALR && MRET_EXTENSION_BESTHTTP) || !USE_FAKE_SIGNALR
            try
            {
#if USE_FAKE_SIGNALR
                HubOptions options = new HubOptions();
                options.ConnectTimeout = new TimeSpan(0, 10, 0);
                options.PreferedTransport = TransportTypes.WebSocket;
                if (!(connection is null) && connection.State != ConnectionStates.Negotiating && connection.State != ConnectionStates.Connected)
                {
                    data.messenger.Post(new Message()
                    {
                        LogingEvent = Log.Event.HUD,
                        message = "Old connection in state: " + connection.State + " shall be executed" 
                    });

                    await connection.CloseAsync();
                    connection = null;
                }
                if (connection is null)
                {
                    connection = new HubConnection(new Uri(data.VDE.serverAddress), new JsonProtocol(new LitJsonEncoder()), options);
                    connection.StartConnect();
                    cancellationToken = new CancellationTokenSource();

                    connection.OnConnected += async e =>
                    {
                        data.messenger.Post(new Message()
                        {
                            LogingEvent = Log.Event.HUD,
                            message = "SignalR Connection established to: " + data.VDE.serverAddress //e.NegotiationResult
                        });
                        await OnConnect(connection);
                    };
                    connection.OnError += Connection_OnError;
                    connection.OnTransportEvent += Connection_OnTransportEvent;
                    connection.OnClosed += e =>
                    {
                        if (stayConnected && data.forrestIsRunning)
                        {
                            data.messenger.Post(new Message()
                            {
                                LogingEvent = Log.Event.HUD,
                                message = "Connection closed with error: " + e.NegotiationResult
                            });
                            Connect();
                        }
                    };
                }
#else
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
                await OnConnect();
#endif

                _ = Task.Run(() => TickerAsync());
            }
            catch (Exception exe)
            {
                if (stayConnected && data.forrestIsRunning && !(connection is null) && connection.State != ConnectionStates.Negotiating)
                {
                    log.Entry("Error connecting to " + data.VDE.serverAddress + ": " + exe.Message, Log.Event.HUD);
                    await Task.Delay(new TimeSpan(0, 0, 5));
                    Connect();
                }
                else
                {
                    log.Entry("Error connecting to " + data.VDE.serverAddress + ": " + exe.Message, Log.Event.HUD);
                }
            }
#endif
        }

#if (USE_FAKE_SIGNALR && MRET_EXTENSION_BESTHTTP) || !USE_FAKE_SIGNALR
        private async Task OnConnect(HubConnection connection)
        {
            await connection.InvokeAsync<string>("RegisterClient", System.Environment.MachineName);
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
#if USE_FAKE_SIGNALR
                await connection.InvokeAsync<string>("RequestConfiguration");
#else
                await connection.InvokeAsync("RequestConfiguration");
#endif
            }
            if (!data.VDE.backendWithBakedData)
            {

#if USE_FAKE_SIGNALR
                await connection.InvokeAsync<string>("GetEntities");
#else
                await connection.InvokeAsync("GetEntities");
#endif
            }
#if !USE_FAKE_SIGNALR
            StreamEntities();
            StreamLinks();
#endif
        }
#endif

#if (USE_FAKE_SIGNALR && MRET_EXTENSION_BESTHTTP) || !USE_FAKE_SIGNALR
        private void Connection_OnTransportEvent(HubConnection arg1, ITransport arg2, TransportEvents arg3)
        {
            if (arg3 == TransportEvents.ClosedWithError || arg3 == TransportEvents.FailedToConnect || arg3 == TransportEvents.Closed)
            {
                data.messenger.Post(new Message()
                {
                    LogingEvent = Log.Event.HUD,
                    message = "An event of a transport: " + arg1.NegotiationResult + " arg: " + arg2 + " arrrg: " + arg3
                });
            }
        }
#endif

#if (USE_FAKE_SIGNALR && MRET_EXTENSION_BESTHTTP) || !USE_FAKE_SIGNALR
        private void Connection_OnError(HubConnection arg1, string arg2)
        {
            data.messenger.Post(new Message()
            {
                LogingEvent = Log.Event.HUD,
                message = "Connection error: " + arg1.NegotiationResult + " arg: " + arg2
            });
        }
#endif

        private void PrepareForFreshInputFromServer()
        {
            data.links.UnHighlightLinks();
        }

#if (USE_FAKE_SIGNALR && MRET_EXTENSION_BESTHTTP) || !USE_FAKE_SIGNALR
        private async Task TickerAsync()
        {
            int ticker = 100;
#if USE_FAKE_SIGNALR
            while (!(connection is null) && connection.State != ConnectionStates.Closed)
#else
            while (!(connection is null) && connection.State != HubConnectionState.Disconnected)
#endif
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
#endif

        internal void Disconnect()
        {
#if !USE_FAKE_SIGNALR
            connection.DisposeAsync();
#endif
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
#if (USE_FAKE_SIGNALR && MRET_EXTENSION_BESTHTTP) || !USE_FAKE_SIGNALR
            try
            {
#if USE_FAKE_SIGNALR
                if(connection.State == ConnectionStates.Connected)
#else
                if (connection.State == HubConnectionState.Connected)
#endif
                {
                    await connection.InvokeAsync<string>("ReceiveTelemetryJson", JsonConvert.SerializeObject(message));
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
#endif
        }

        internal async void SendEntities(List<Entities.ExportEntity> ExportEntities)
        {
#if (USE_FAKE_SIGNALR && MRET_EXTENSION_BESTHTTP) || !USE_FAKE_SIGNALR
            try
            {

#if USE_FAKE_SIGNALR
                if (connection.State == ConnectionStates.Connected)
#else
                if (connection.State == HubConnectionState.Connected)
#endif
                {
                    await connection.InvokeAsync<string>("ReceiveEntityPositionJson", JsonConvert.SerializeObject(ExportEntities));
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
#endif
        }

        internal async void SendLinks(List<ExportLink> exportLinks)
        {
#if (USE_FAKE_SIGNALR && MRET_EXTENSION_BESTHTTP) || !USE_FAKE_SIGNALR
            try
            {

#if USE_FAKE_SIGNALR
                if (connection.State == ConnectionStates.Connected)
#else
                if (connection.State == HubConnectionState.Connected)
#endif
                {
                    await connection.InvokeAsync<string>("ReceiveRelationsJson", JsonConvert.SerializeObject(exportLinks));
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
#endif
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
#if (USE_FAKE_SIGNALR && MRET_EXTENSION_BESTHTTP) || !USE_FAKE_SIGNALR
#if USE_FAKE_SIGNALR
            await connection.InvokeAsync<string>("GetRelations");
#else
            await connection.InvokeAsync("GetRelations");
#endif
#endif
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
#if USE_FAKE_SIGNALR
            log.Entry("StreamEntities is disabled due to: Assembly 'System.Threading.Channels' with identity 'System.Threading.Channels, Version=4.0.2.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' uses 'System.Runtime, Version=4.2.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' which has a higher version than referenced assembly 'System.Runtime' with identity 'System.Runtime, Version=4.1.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' ", Log.Event.HUD);
#else
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
#endif
        }

        async void StreamLinks()
        {
#if USE_FAKE_SIGNALR
            log.Entry("StreamLinks is disabled due to: Assembly 'System.Threading.Channels' with identity 'System.Threading.Channels, Version=4.0.2.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' uses 'System.Runtime, Version=4.2.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' which has a higher version than referenced assembly 'System.Runtime' with identity 'System.Runtime, Version=4.1.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' ", Log.Event.HUD);
#else
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
#endif
        }
    }
}
