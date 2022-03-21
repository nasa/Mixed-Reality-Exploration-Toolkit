#if MRET_2021_OR_LATER
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDE.Communication
{
    public class GMSECMessage : GSFC.ARVR.GMSEC.GMSECMessage
    {
        public Message MESSAGE;
        [Serializable]
        public struct Message
        {
            public Massage MESSAGE { get; set; }

            public static implicit operator Message(string jsonMessage)
            {
                if (
                    !(jsonMessage is null) &&
                    jsonMessage.Length > 0 &&
                    jsonMessage.Contains('[') &&
                    jsonMessage.Contains(']')
                    )
                {
                    try
                    {
                        Message mess = JsonConvert.DeserializeObject<Message>(jsonMessage);
                        return mess;
                    }
                    catch (JsonSerializationException exe)
                    {
                        throw new Assets.VDE.Communication.Message()
                        {
                            Fatal = Assets.VDE.Communication.Message.Fatals.ErrorDecodingJson,
                            message = exe.Message
                        };
                    }
                    catch (Exception exe)
                    {
                        throw new Assets.VDE.Communication.Message()
                        {
                            Fatal = Assets.VDE.Communication.Message.Fatals.ErrorDecodingJson,
                            message = exe.Message
                        };
                    }
                }
                return new Message();
            }

            [Serializable]
            public struct Massage
            {
                public string SUBJECT { get; set; }
                public string KIND { get; set; }
                public List<rawFIELD> FIELD { get; set; }

                public struct rawFIELD
                {
                    public string NAME { get; set; }
                    public string TYPE { get; set; }
                    public string VALUE { get; set; }
                }
            }
        }
        public GMSECMessage(string _subject, MessageKind _kind) : base(_subject, _kind)
        {
            subject = _subject;
            kind = _kind;
        }
        public GMSECMessage(string jsonMessage) : base(jsonMessage)
        {
            MESSAGE = jsonMessage;
            subject = MESSAGE.MESSAGE.SUBJECT;
            kind = StringToMessageKind(MESSAGE.MESSAGE.KIND);
            foreach (Message.Massage.rawFIELD rawFIELD in MESSAGE.MESSAGE.FIELD)
            {
                if (!fields.Where(field => field.name == rawFIELD.NAME).Any())
                {
                    fields.Add(new Field(rawFIELD.NAME, rawFIELD.TYPE, rawFIELD.VALUE));
                }
            }
        }

        internal object TryGet(Field.FieldType type, string toGet)
        {
            object enter = null;
            bool set = false;
            if (MESSAGE.MESSAGE.FIELD.Exists(field => field.NAME == toGet && field.TYPE == type.ToString()))
            {
                enter = GetField(toGet).GetValue();
                set = true;
            }
            switch (type)
            {
                case Field.FieldType.UNSET:
                    break;
                case Field.FieldType.BIN:
                    break;
                case Field.FieldType.BOOL:
                    break;
                case Field.FieldType.CHAR:
                    break;
                case Field.FieldType.F32:
                    return (float)(set ? enter : 0.0F);
                case Field.FieldType.F64:
                    break;
                case Field.FieldType.I8:
                    break;
                case Field.FieldType.I16:
                    return (short)(set ? enter : 0);
                case Field.FieldType.I32:
                    return (int)(set ? enter : 0);
                case Field.FieldType.I64:
                    break;
                case Field.FieldType.STRING:
                    return (string)(set ? enter : "");
                case Field.FieldType.U8:
                    break;
                case Field.FieldType.U16:
                    return (ushort)(set ? enter : 0);
                case Field.FieldType.U32:
                    break;
                case Field.FieldType.U64:
                    break;
                default:
                    break;
            }
            return (int) 0;
        }
    }
}
#endif