#if MRET_2021_OR_LATER
using Assets.VDE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VDE.Communication;
using static GSFC.ARVR.GMSEC.GMSECMessage.Field;

namespace Assets.VDE.Communication
{
    internal class GMSECConnection
    {
        Log log;
        Data data;
        private MonoGMSEC gmsec;
        private GMSECMessage teleREQfromServer;
        private Thread requestThread;
        private Thread telemetryThread;
        internal System.Collections.Concurrent.ConcurrentQueue<Telemetry> telewall = new System.Collections.Concurrent.ConcurrentQueue<Telemetry> { };

        internal GMSECConnection(Data data)
        {
            this.data = data;
            log = new Log("GMSECConnection", data.messenger);
            log.Entry("Starting up");
            Connect();
        }
        private void Connect()
        {
            log.Entry("Initializing GMSEC Object");
            gmsec = new MonoGMSEC();
            gmsec.Initialize();
            log.Entry("GMSEC Initialized, Setting up Config");
            gmsec.CreateConfig();
            gmsec.AddToConfig("connectionType", "gmsec_bolt");
            gmsec.AddToConfig("server", data.VDE.serverAddress);
            log.Entry("Connecting to " + data.VDE.serverAddress, Log.Event.HUD);
            gmsec.Connect();
            log.Entry("Should be connected to: " + data.VDE.serverAddress, Log.Event.HUD);
            telemetryThread = new System.Threading.Thread(new System.Threading.ThreadStart(TeleChannel));
            telemetryThread.Start();
            requestThread = new System.Threading.Thread(new System.Threading.ThreadStart(Request));
            requestThread.Start();
        }
        private async void TeleChannel()
        {
            bool stillWaiting = true;
            gmsec.Subscribe("GMSEC.VDE.SERVER.REQ.TELEMETRY");
            log.Entry("Waiting for response: GMSEC.VDE.SERVER.REQ.TELEMETRY");

            while (stillWaiting)
            {
                GMSECMessage msg = gmsec.Receive(10000);
                if (msg != null && msg.GetSubject() == "GMSEC.VDE.SERVER.REQ.TELEMETRY")
                {
                    log.Entry("[TeleChannel] Message Received:\n" + msg.ToString());
                    teleREQfromServer = msg;
                    stillWaiting = false;
                }
            }

            while (data.forrestIsRunning)
            {
                if(telewall.TryDequeue(out Telemetry teler))
                {
                    SendTelemetry(teler);
                }
                await Task.Delay(1000);
            }
        }

        internal void EnqueueTelemetry(Telemetry telemetry)
        {
            telewall.Enqueue(telemetry);
        }
        internal void SendTelemetry(Telemetry telemetry)
        {
            string telestring = Newtonsoft.Json.JsonConvert.SerializeObject(telemetry);

            /*
             * this here wants to be a 2016.00.GMSEC.MSG.TLM.CCSDSPKT message. 
             * as MIST is unavailable in MonoGMSEC, we're bulding it here with hard labour.
             * 
             * and because the ConnectionManager is unable to hande multiple REQUEST-RESPONSE 
             * threads simultaneously, this hack has to do. for now.
             */
            gmsec.CreateNewMessage("GMSEC.VDE.CLIENT.MSG.TLM");

            gmsec.AddF32FieldToMessage("HEADER-VERSION", 2010);
            gmsec.AddF32FieldToMessage("CONTENT-VERSION", 2016);

            gmsec.AddI16FieldToMessage("REQUEST-TYPE", 1);
            gmsec.AddI16FieldToMessage("SEVERITY", 1);
            gmsec.AddI16FieldToMessage("VCID", 1);

            gmsec.AddBoolFieldToMessage("FINAL-MESSAGE", false);

            gmsec.AddStringFieldToMessage("MESSAGE-TYPE", "MSG");
            gmsec.AddStringFieldToMessage("MESSAGE-SUBTYPE", "TLM");
            gmsec.AddStringFieldToMessage("MISSION-ID", "VDE");
            gmsec.AddStringFieldToMessage("FACILITY", "VDEC");
            gmsec.AddStringFieldToMessage("COMPONENT", "CLIENT");

            gmsec.AddStringFieldToMessage("FORMAT", "CCSDSPKT");
            gmsec.AddStringFieldToMessage("COLLECTION-POINT", "TWO TOWELS");
            gmsec.AddStringFieldToMessage("STREAM-MODE", "RT");
            gmsec.AddStringFieldToMessage("PHY-CHAN", "VOGONS VOICE");

            gmsec.AddBinaryFieldToMessage("DATA", 0, 1);

            gmsec.AddStringFieldToMessage("DAATA", telestring);
            gmsec.PublishMessage();
        }
        private void Request()
        {
            GMSECMessage req = ComposeMessage("REGISTER", "GMSEC.VDE.CLIENT.MSG.ANNOUNCE"); 
            GMSECMessage rpl = gmsec.Request(req, 10000);

            if (rpl.kind == GSFC.ARVR.GMSEC.GMSECMessage.MessageKind.REPLY && rpl.subject == "GMSEC.VDE.SERVER.MSG.RESPONSE")
            {
                log.Entry("Got response from server: " + rpl.GetStringField("UNIQUE-ID").GetValueAsString());
                data.config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(rpl.GetStringField("MNEMONIC.1.VALUE").GetValueAsString());

                data.messenger.Post(new Message()
                {
                    LayoutEvent = Layouts.Layouts.LayoutEvent.InitializeAll
                });
                RequestEntitiesAndRelations();
            }
            else
            {
                log.Entry("Invalid response from GMSEC server: " + rpl.ToString(), Log.Event.HUD);
            }
        }

        private void RequestEntitiesAndRelations()
        {
            GMSECMessage
            req = ComposeMessage("Send Entities");

            GMSECMessage
            rpl = gmsec.Request(req, 10000);

            if (rpl.kind == GSFC.ARVR.GMSEC.GMSECMessage.MessageKind.REPLY && rpl.subject == "GMSEC.VDE.SERVER.MSG.RESPONSE")
            {
                ImportEntities(rpl);
            }

            req = ComposeMessage("Send Relations");
            rpl = gmsec.Request(req, 10000);

            if (rpl.kind == GSFC.ARVR.GMSEC.GMSECMessage.MessageKind.REPLY && rpl.subject == "GMSEC.VDE.SERVER.MSG.RESPONSE")
            {
                ImportRelations(rpl);
            }
        }

        private void ImportEntities(GMSECMessage message)
        {
            ushort count = (ushort) message.GetU16Field("NUM-OF-MNEMONICS").GetValue();
            log.Entry("Importing " + count + " entities.", Log.Event.HUD);
            _ = Task.Run(() => data.entities.MonitorEntitiesCreationProgress(data, count));
            if (
                count > 0 &&
                message.fields.Where(field => field.name == "MNEMONIC.1.NAME").Any()
                )
            {
                for (int incoming = 1; incoming <= count; incoming++)
                {
                    string prefix = "MNEMONIC." + incoming + ".";
                    try
                    {
                        if (message.fields.Where(field => field.name.StartsWith("MNEMONIC." + incoming)).Count() > 5)
                        {
                            Entity entity = new Entity();
                            entity.a = (float)message.TryGet(FieldType.F32, prefix + "A");
                            entity.id = (int)message.TryGet(FieldType.I32, prefix + "ID");
                            entity.gm = (int)message.TryGet(FieldType.I32, prefix + "GM");
                            entity.md = (int)message.TryGet(FieldType.I32, prefix + "MD");
                            entity.pos = (int)message.TryGet(FieldType.I32, prefix + "POS");
                            entity.type = (Entity.Type)message.TryGet(FieldType.I32, prefix + "TYPE");
                            entity.count = (int)message.TryGet(FieldType.I32, prefix + "COUNT");
                            string tmpC = (string)message.TryGet(FieldType.STRING, prefix + "C");
                            // otherwise ParseColour will parse 255 white from empty string.
                            if (!(tmpC is null) && tmpC.Length > 0)
                            {
                                entity.c = (string)message.TryGet(FieldType.STRING, prefix + "C");
                            }
                            entity.v = (string)message.TryGet(FieldType.STRING, prefix + "V");
                            entity.r = (string)message.TryGet(FieldType.STRING, prefix + "R");
                            entity.g = (string)message.TryGet(FieldType.STRING, prefix + "G");
                            entity.info = (string)message.TryGet(FieldType.STRING, prefix + "INFO");
                            entity.name = (string)message.TryGet(FieldType.STRING, prefix + "NAME");
                            entity.uuid = (string)message.TryGet(FieldType.STRING, prefix + "UUID");
                            data.entities.AddOrUpdate(entity);
                        }
                        else
                        {
                            log.Entry("NOT done (" + incoming + " / " + count + "): " + message.fields.Where(field => field.name.StartsWith("MNEMONIC." + incoming)).Count());
                        }
                    }
                    catch (Exception exe)
                    {
                        log.Entry("Error while parsing incoming entity ("+ message.GetStringField(prefix + "NAME").GetValueAsString() + "): " + exe.Message + "\n" + exe.StackTrace);
                    }
                }
            }
            log.Entry("Imported " + count + " entities.", Log.Event.HUD);
        }
        private void ImportRelations(GMSECMessage message)
        {
            ushort count = (ushort) message.GetU16Field("NUM-OF-MNEMONICS").GetValue();
            if (count > 0)
            {
                List<UI.ImportLink> rareImports = new List<UI.ImportLink> { };
                for (int incoming = 1; incoming <= count; incoming++)
                {
                    string prefix = "MNEMONIC." + incoming + ".";
                    try
                    {
                        rareImports.Add(new UI.ImportLink()
                        {
                            s = (int)message.TryGet(FieldType.I32, prefix + "SRC"), //(int)message.GetI32Field(prefix + "SRC").GetValue(),
                            d = (int)message.TryGet(FieldType.I32, prefix + "DST"),//; (int)message.GetI32Field(prefix + "DST").GetValue(),
                            w = (int)message.TryGet(FieldType.I32, prefix + "W")//; (int)message.GetI32Field(prefix + "W").GetValue(),
                        });
                    }
                    catch (Exception exe)
                    {
                        log.Entry("Error while parsing incoming relations (" + incoming + "/" + count + "): " + exe.Message + "\n" + exe.StackTrace);
                    }
                }
                if (rareImports.Count > 0)
                {
                    // not manipulating links directly.. yet. 
                    log.Entry("Sending relations to be imported: " + data.entities.Count() + " & " + rareImports.Count);
                    data.links.Import(JsonSerializer.Serialize(rareImports));
                }
            }
        }

        private static GMSECMessage ComposeMessage(string topic, string subject = "GMSEC.VDE.CLIENT.MSG.REQUEST")
        {
            GMSECMessage req = new GMSECMessage(subject, GSFC.ARVR.GMSEC.GMSECMessage.MessageKind.REQUEST);
            req.AddField("HEADER-VERSION", (float)2010);
            req.AddField("CONTENT-VERSION", (float)2016);
            req.AddField("FACILITY", "VDEC");
            req.AddField("MESSAGE-SUBTYPE", "MVAL");
            req.AddField("MESSAGE-TYPE", "REQ");
            req.AddField("MISSION-ID", "VDE");
            req.AddField("COMPONENT", "CLIENT");
            req.AddField("NUM-OF-MNEMONICS", (ushort)1);
            req.AddField("REQUEST-TYPE", (short)1);
            req.AddField("SEVERITY", (short)1);
            req.AddField("MNEMONIC.1.NAME", "Virtual Data Explorer Client");
            req.AddField("MNEMONIC.1.VALUE", topic);
            req.AddField("OCCURRENCE-TYPE", "SYS");
            req.AddField("SUBCLASS", "AST");
            req.AddField("EVENT-TIME", GMSEC.API.TimeUtil.FormatTime(GMSEC.API.TimeUtil.GetCurrentTime()));
            return req;
        }

    }
}
#endif