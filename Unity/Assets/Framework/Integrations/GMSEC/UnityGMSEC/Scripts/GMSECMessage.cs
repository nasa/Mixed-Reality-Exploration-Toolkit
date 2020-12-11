using System.Collections.Generic;
using UnityEngine;
using System;

namespace GSFC.ARVR.GMSEC
{
    public class GMSECMessage
    {
        public enum MessageKind { UNSET, PUBLISH, REQUEST, REPLY }

        string subject = "UNSET";
        MessageKind kind = MessageKind.UNSET;
        List<Field> fields = new List<Field>();

#region FIELD
        public class Field
        {
            public enum FieldType { UNSET, BIN, BOOL, CHAR, F32, F64, I8, I16, I32, I64, STRING, U8, U16, U32, U64 }

            public string name = "UNSET";
            public FieldType type = FieldType.UNSET;

            private List<byte> byteVal; // Big-endian
            private bool boolVal;
            private char charVal;
            private float f32Val;
            private double f64Val;
            private sbyte i8Val;
            private Int16 i16Val;
            private Int32 i32Val;
            private Int64 i64Val;
            private string stringVal;
            private byte u8Val;
            private UInt16 u16Val;
            private UInt32 u32Val;
            private UInt64 u64Val;

            public Field(string _name, string _type, string _stringVal)
            {
                name = _name;
                type = StringToFieldType(_type);

                switch (type)
                {
                    case FieldType.BIN:
                        byteVal = new List<byte>();
                        for (int i = _stringVal.Length - 1; i > -1; i -= 2)
                        {
                            if (i > 0)
                            {
                                byteVal.Add(Convert.ToByte(_stringVal.Substring(i - 1, 2), 16));
                            }
                            else
                            {
                                byteVal.Add(Convert.ToByte(_stringVal.Substring(i - 1, 1), 16));
                            }
                        }
                        byteVal.Reverse();
                        break;

                    case FieldType.BOOL:
                        boolVal = Convert.ToBoolean(_stringVal);
                        break;

                    case FieldType.CHAR:
                        charVal = Convert.ToChar(_stringVal);
                        break;

                    case FieldType.F32:
                        f32Val = Convert.ToSingle(_stringVal);
                        break;

                    case FieldType.F64:
                        f64Val = Convert.ToDouble(_stringVal);
                        break;

                    case FieldType.I8:
                        i8Val = Convert.ToSByte(_stringVal);
                        break;

                    case FieldType.I16:
                        i16Val = Convert.ToInt16(_stringVal);
                        break;

                    case FieldType.I32:
                        i32Val = Convert.ToInt32(_stringVal);
                        break;

                    case FieldType.I64:
                        i64Val = Convert.ToInt64(_stringVal);
                        break;

                    case FieldType.STRING:
                        stringVal = _stringVal;
                        break;

                    case FieldType.U8:
                        u8Val = Convert.ToByte(_stringVal);
                        break;

                    case FieldType.U16:
                        u16Val = Convert.ToUInt16(_stringVal);
                        break;

                    case FieldType.U32:
                        u32Val = Convert.ToUInt32(_stringVal);
                        break;

                    case FieldType.U64:
                        u64Val = Convert.ToUInt64(_stringVal);
                        break;

                    case FieldType.UNSET:
                    default:
                        break;
                }
            }

            public Field(string _name, List<byte> _value)
            {
                name = _name;
                type = FieldType.BIN;
                byteVal = _value;
            }

            public Field(string _name, bool _value)
            {
                name = _name;
                type = FieldType.BOOL;
                boolVal = _value;
            }

            public Field(string _name, char _value)
            {
                name = _name;
                type = FieldType.CHAR;
                charVal = _value;
            }

            public Field(string _name, float _value)
            {
                name = _name;
                type = FieldType.F32;
                f32Val = _value;
            }

            public Field(string _name, double _value)
            {
                name = _name;
                type = FieldType.F64;
                f64Val = _value;
            }

            public Field(string _name, sbyte _value)
            {
                name = _name;
                type = FieldType.I8;
                i8Val = _value;
            }

            public Field(string _name, Int16 _value)
            {
                name = _name;
                type = FieldType.I16;
                i16Val = _value;
            }

            public Field(string _name, Int32 _value)
            {
                name = _name;
                type = FieldType.I32;
                i32Val = _value;
            }

            public Field(string _name, Int64 _value)
            {
                name = _name;
                type = FieldType.I64;
                i64Val = _value;
            }

            public Field(string _name, string _value)
            {
                name = _name;
                type = FieldType.STRING;
                stringVal = _value;
            }

            public Field(string _name, byte _value)
            {
                name = _name;
                type = FieldType.U8;
                u8Val = _value;
            }

            public Field(string _name, UInt16 _value)
            {
                name = _name;
                type = FieldType.U16;
                u16Val = _value;
            }

            public Field(string _name, UInt32 _value)
            {
                name = _name;
                type = FieldType.U32;
                u32Val = _value;
            }

            public Field(string _name, UInt64 _value)
            {
                name = _name;
                type = FieldType.U64;
                u64Val = _value;
            }

            public object GetValue()
            {
                switch (type)
                {
                    case FieldType.BIN:
                        return byteVal;

                    case FieldType.BOOL:
                        return boolVal;

                    case FieldType.CHAR:
                        return charVal;

                    case FieldType.F32:
                        return f32Val;

                    case FieldType.F64:
                        return f64Val;

                    case FieldType.I8:
                        return i8Val;

                    case FieldType.I16:
                        return i16Val;

                    case FieldType.I32:
                        return i32Val;

                    case FieldType.I64:
                        return i64Val;

                    case FieldType.STRING:
                        return stringVal;

                    case FieldType.U8:
                        return u8Val;

                    case FieldType.U16:
                        return u16Val;

                    case FieldType.U32:
                        return u32Val;

                    case FieldType.U64:
                        return u64Val;

                    case FieldType.UNSET:
                    default:
                        return "UNSET";
                }
            }

            public string GetValueAsString()
            {
                switch (type)
                {
                    case FieldType.BIN:
                        return BitConverter.ToString(byteVal.ToArray());

                    case FieldType.BOOL:
                        return boolVal.ToString();

                    case FieldType.CHAR:
                        return charVal.ToString();

                    case FieldType.F32:
                        return f32Val.ToString();

                    case FieldType.F64:
                        return f64Val.ToString();

                    case FieldType.I8:
                        return i8Val.ToString();

                    case FieldType.I16:
                        return i16Val.ToString();

                    case FieldType.I32:
                        return i32Val.ToString();

                    case FieldType.I64:
                        return i64Val.ToString();

                    case FieldType.STRING:
                        return stringVal;

                    case FieldType.U8:
                        return u8Val.ToString();

                    case FieldType.U16:
                        return u16Val.ToString();

                    case FieldType.U32:
                        return u32Val.ToString();

                    case FieldType.U64:
                        return u64Val.ToString();

                    case FieldType.UNSET:
                    default:
                        return "UNSET";
                }
            }

            public string ToJSON()
            {
                return "{\"FIELD\":{\"NAME\":\"" + name + "\",\"TYPE\":\""
                    + type.ToString() + "\",\"VALUE\":\"" + GetValueAsString() + "\"}}";
            }

            public override string ToString()
            {
                return "FIELD|NAME:" + name + " TYPE:" + type.ToString() + " VALUE:" + GetValueAsString();
            }

            private FieldType StringToFieldType(string type)
            {
                if (type.Equals("BIN"))
                {
                    return FieldType.BIN;
                }
                else if (type.Equals("BOOL"))
                {
                    return FieldType.BOOL;
                }
                else if (type.Equals("CHAR"))
                {
                    return FieldType.CHAR;
                }
                else if (type.Equals("F32"))
                {
                    return FieldType.F32;
                }
                else if (type.Equals("F64"))
                {
                    return FieldType.F64;
                }
                else if (type.Equals("I8"))
                {
                    return FieldType.I8;
                }
                else if (type.Equals("I16"))
                {
                    return FieldType.I16;
                }
                else if (type.Equals("I32"))
                {
                    return FieldType.I32;
                }
                else if (type.Equals("I64"))
                {
                    return FieldType.I64;
                }
                else if (type.Equals("STRING"))
                {
                    return FieldType.STRING;
                }
                else if (type.Equals("U8"))
                {
                    return FieldType.U8;
                }
                else if (type.Equals("U16"))
                {
                    return FieldType.U16;
                }
                else if (type.Equals("U32"))
                {
                    return FieldType.U32;
                }
                else if (type.Equals("U64"))
                {
                    return FieldType.U64;
                }
                else
                {
                    return FieldType.UNSET;
                }
            }
        }
#endregion

#region CONSTRUCTORS
        public GMSECMessage(string _subject, MessageKind _kind)
        {
            subject = _subject;
            kind = _kind;
        }

        public GMSECMessage(string jsonMessage)
        {
            List<string> rawElements = JSONMessageToList(jsonMessage);

            // Stop if this is not a message.
            if (!rawElements[0].Equals("MESSAGE"))
            {
                return;
            }

            for (int i = 1; i < rawElements.Count; i++)
            {
                if (rawElements[i].Equals("SUBJECT"))
                {
                    subject = rawElements[++i];
                }
                else if (rawElements[i].Equals("KIND"))
                {
                    kind = StringToMessageKind(rawElements[++i]);
                }
                else if (rawElements[i].Equals("FIELD"))
                {
                    bool inFields = true;
                    while (inFields && (i < rawElements.Count - 1))
                    {
                        if (rawElements[i + 1].Equals("NAME") && rawElements[i + 3].Equals("TYPE") && rawElements[i + 5].Equals("VALUE"))
                        {
                            fields.Add(new Field(rawElements[i + 2], rawElements[i + 4], rawElements[i + 6]));
                            i += 6;
                        }
                        else if (rawElements[i + 1].Equals("NAME") && rawElements[i + 3].Equals("TYPE") && rawElements[i + 7].Equals("VALUE"))
                        {
                            fields.Add(new Field(rawElements[i + 2], rawElements[i + 4], rawElements[i + 8]));
                            i += 8;
                        }
                        else
                        {
                            inFields = false;
                        }
                    }
                }
                else
                {
                    Debug.LogError("[GMSECMessage] Unidentified element \"" + rawElements[i] + "\" in message.");
                }
            }
        }
#endregion

#region ADDFIELD
        public void AddField(string _name, List<byte> _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, bool _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, char _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, float _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, double _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, sbyte _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, Int16 _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, Int32 _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, Int64 _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, string _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, byte _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, UInt16 _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, UInt32 _value)
        {
            fields.Add(new Field(_name, _value));
        }

        public void AddField(string _name, UInt64 _value)
        {
            fields.Add(new Field(_name, _value));
        }
        #endregion

#region GETFIELD
        public Field[] GetAllFields()
        {
            return fields.ToArray();
        }

        public Field GetField(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.name == _name)
                {
                    return field;
                }
            }
            return null;
        }

        public Field GetBinaryField(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.BIN)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetBooleanField(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.BOOL)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetCharField(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.CHAR)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetF32Field(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.F32)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetF64Field(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.F64)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetI8Field(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.I8)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetI16Field(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.I16)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetI32Field(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.I32)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetI64Field(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.I64)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetStringField(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.STRING)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetU8Field(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.U8)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetU16Field(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.U16)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetU32Field(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.U32)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        public Field GetU64Field(string _name)
        {
            foreach (Field field in fields)
            {
                if (field.type == Field.FieldType.U64)
                {
                    if (field.name == _name)
                    {
                        return field;
                    }
                }
            }
            return null;
        }
#endregion

        public string ToJSON()
        {
            bool insertComma = false;
            string messageString = "{\"MESSAGE\":{\"KIND\":\"" + kind.ToString()
                + "\",\"SUBJECT\":\"" + subject.ToString() + "\",\"FIELD\":[";

            foreach (Field field in fields)
            {
                if (insertComma)
                {
                    messageString = messageString + ",";
                }

                messageString = messageString + "{\"NAME\":\"" + field.name + "\",\"TYPE\":\""
                    + field.type.ToString() + "\",\"VALUE\":\"" + field.GetValueAsString() + "\"}";

                insertComma = true;
            }
            messageString = messageString + "]}}";

            return messageString;
        }

        public override string ToString()
        {
            string messageString = "MESSAGE|SUBJECT:" + subject + " KIND:" + kind.ToString();

            foreach (Field field in fields)
            {
                messageString += "\n " + field.ToString();
            }

            return messageString;
        }

        public string GetSubject()
        {
            return subject;
        }

        public MessageKind GetKind()
        {
            return kind;
        }

#region Helpers
        private List<string> JSONMessageToList(string jsonMessage)
        {
            string csvMessage = jsonMessage.Replace("}", "").Replace("]", "");
            string[] rawElements = csvMessage.Split(',', '"', '{', '[');

            List<string> result = new List<string>();
            foreach (string s in rawElements)
            {
                if (!string.IsNullOrEmpty(s) && !s.Equals(":") && (s[0] != 0))
                {
                    result.Add(s.Replace("\"", ""));
                }
            }

            return result;
        }

        private MessageKind StringToMessageKind(string kind)
        {
            if (kind.Equals("PUBLISH"))
            {
                return MessageKind.PUBLISH;
            }
            else if (kind.Equals("REQUEST"))
            {
                return MessageKind.REQUEST;
            }
            else if (kind.Equals("REPLY"))
            {
                return MessageKind.REPLY;
            }
            else
            {
                return MessageKind.UNSET;
            }
        }
#endregion
    }
}