﻿using System.IO;
using System.Linq;
using VVVV.Utils.OSC;


namespace VVVV.Packs.Messaging.Serializing
{
    using System;
    using System.Collections.Generic;
    using Time = VVVV.Packs.Time.Time;

    public static class OSCExtensions
    {

        public static Stream ToOSC(this Message message, bool extendedMode = false)
        {
            OSCBundle bundle = new OSCBundle(message.TimeStamp, extendedMode);
            foreach (string name in message.Fields)
            {
                string oscAddress = "";

                foreach (string part in message.Topic.Split('.'))
                {
                    if (part.Trim() != "") oscAddress += "/" + part;
                }

                foreach (string part in name.Split('.'))
                {
                    if (part.Trim() != "") oscAddress += "/" + part;
                }

                OSCMessage m = new OSCMessage(oscAddress, extendedMode);
                Bin bl = message[name];
                for (int i = 0; i < bl.Count; i++) m.Append(bl[i]);
                bundle.Append(m);
            }
            return new MemoryStream(bundle.BinaryData); // packs implicitly
        }

        public static Message FromOSC(Stream stream, bool extendedMode = false, string messagePrefix = "", int contractAddress = 1)
        {
            Message message = new Message();

            MemoryStream ms = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(ms);
            byte[] bytes = ms.ToArray();

            if (bytes.Length == 0) return null;

            var pack = OSCPacket.Unpack(bytes, extendedMode);
            bool useNesting;

            OSCBundle bundle;
            if (pack.IsBundle())
            {
                bundle = (OSCBundle)pack;
                message.TimeStamp = bundle.getTimeStamp();
                useNesting = true;
            }
            else
            {
                bundle = new OSCBundle(extendedMode);
                var m = (OSCMessage)pack;
                bundle.Append(m);
                message.TimeStamp = Time.CurrentTime();
                useNesting = false;
            }


            foreach (OSCMessage m in bundle.Values)
            {

                string[] address = m.Address.Trim(new char[] { '/' }).Split('/');

                string messageAddress = string.Join(".", address.Take(Math.Max(1, address.Length - contractAddress)).ToArray());

                if (messagePrefix.Trim() == "") message.Topic = messageAddress;
                    else message.Topic = messagePrefix + "." + messageAddress;
                
                // empty messages are usually used for requesting data
                if (m.Values.Count <= 0)
                {
                    // leave message emtpy, cannot infer type.
                }
                else
                {
                    var usedTypes = (
                                        from v in m.Values.ToArray()
                                        select v.GetType()
                                    ).Distinct();

                    string attribName = string.Join(".", address.Skip(Math.Max(0, address.Length - contractAddress)).ToArray());

                    if (usedTypes.Count() > 1)
                    {
                        var inner = new Message(message.Topic + "." + attribName);
                        var max = m.Values.Count;

                        for( int i=0;i< max;i++)
                        {
                            var item = m.Values[i];
                            var num = i.ToString();
                            inner[num] = BinFactory.New(item.GetType());
                            inner[num].Add(item);
                        }

                        if (useNesting)
                        {
                            message[attribName] = BinFactory.New(inner);
                        }
                        else
                        {
                            message = inner;
                        }
                    }
                    else
                    {
                        var bin = BinFactory.New(usedTypes.First());
                        bin.AssignFrom(m.Values);
                        message[attribName] = bin;
                    }
                }
            }

            return message;
        }
    }
}
