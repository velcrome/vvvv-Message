using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

using VVVV.PluginInterfaces.V2;

using VVVV.Core.Logging;
using VVVV.Utils.OSC;

namespace VVVV.Nodes.OSC
{
    public enum SelectEnum
    {
        All,
        First,
        Last
    }

    enum Filter { Matches, Contains, Starts, Ends, All };

    #region PluginInfo
    [PluginInfo(Name = "Sift", Category = "OSC", Help = "Sift packets for an address", Tags = "elliotwoods")]
    #endregion PluginInfo
    public class SiftNode : IPluginEvaluate
    {
        #region fields & pins
        
        #pragma warning disable 649, 169
        [Input("Input")]
        IDiffSpread<Stream> FInput;

        [Input("Filter address")]
        ISpread<string> FPinInAddress;

        [Input("Mode", IsSingle = true)]
        ISpread<Filter> FPinInFilter;


        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true, BinVisibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FExtendedMode;
        
        [Output("Output", AutoFlush = false)]
        ISpread<Stream> FOutput;
        [Import]
        ILogger FLogger;
        #pragma warning restore

        #endregion fields & pins



        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            //            if (!FInput.IsChanged) return;

            FOutput.SliceCount = 0;
            SpreadMax = FInput.SliceCount;

            if ((FInput.SliceCount == 0) || (FInput[0] == null) || (FInput[0].Length == 0)) return;

            for (int i = 0; i < SpreadMax; i++)
            {
                MemoryStream ms = new MemoryStream();
                FInput[i].Position = 0;
                FInput[i].CopyTo(ms);
                byte[] bytes = ms.ToArray();
                int start = 0;

                OSCPacket packet = OSCPacket.Unpack(bytes, ref start, (int) ms.Length, FExtendedMode[0]);


                bool matches = false;
                for (int j = 0; j < FPinInAddress.SliceCount; j++)
                {
                    switch (FPinInFilter[0])
                    {
                        case Filter.Matches:
                            matches |= packet.Address == FPinInAddress[j];
                            break;

                        case Filter.Contains:
                            matches |= packet.Address.Contains(FPinInAddress[j]);
                            break;

                        case Filter.Starts:
                            matches |= packet.Address.StartsWith(FPinInAddress[j]);
                            break;

                        case Filter.Ends:
                            matches |= packet.Address.EndsWith(FPinInAddress[j]);
                            break;

                        case Filter.All:
                            matches = true;
                            break;
                    }
                }

                if (matches) FOutput.Add(ms);
                //                    } catch (Exception ex) {}
            }

            FOutput.Flush();
        }

    }    
    
    #region PluginInfo
    [PluginInfo(Name = "Bundle", Category = "OSC", Help = "Bundle a bunch of OSC messages", Tags = "velcrome")]
    #endregion PluginInfo
    public class BundleOSCNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        IDiffSpread<Stream> FInput;

        [Input("Distribute", DefaultBoolean = false, IsSingle = true, IsToggle = true)]
        IDiffSpread<bool> FDistribute;

        [Input("Match Rule", DefaultEnumEntry = "All", IsSingle = true)]
        IDiffSpread<SelectEnum> FSelect;

        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true, BinVisibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FExtendedMode;
    
        [Output("Output", AutoFlush = false)]
        ISpread<Stream> FOutput;

        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        protected string MainAddress(string address)
        {
            string r = "";
            var parts = address.Split('/');
            for (int i = 0; i < parts.Length - 1; i++ )
            {
                r += "/" + parts[i];
            }
            return r.Substring(1);
        }


        public void Evaluate(int SpreadMax)
        {

            if (!FInput.IsChanged && !FSelect.IsChanged && !FDistribute.IsChanged) return;

            if ((FInput.SliceCount == 0) || (FInput[0] == null) || (FInput[0].Length <= 1)) return;

            SpreadMax = FInput.SliceCount;

            var isFound = new Dictionary<string, bool>();
            var messages = new List<OSCMessage>();

            for (int i = 0; i < SpreadMax; i++)
            {
                var ms = new MemoryStream();

                int index = FSelect[0] == SelectEnum.Last ? SpreadMax - i - 1 : i;
                FInput[index].Position = 0; // rewind stream
                FInput[index].CopyTo(ms);
                byte[] bytes = ms.ToArray();
                int start = 0;

                OSCPacket packet = OSCPacket.Unpack(bytes, ref start, (int) FInput[index].Length, FExtendedMode[0]);

                if (packet.IsBundle())
                {
                    var packets = ((OSCBundle) packet).Values;
                    for (int j = 0; j < packets.Count; j++)
                    {
                        int innerIndex = FSelect[0] == SelectEnum.Last ? packets.Count - j - 1 : j;
                        var innerMessage = (OSCMessage) packets[innerIndex];

                        if (FSelect[0] == SelectEnum.All)
                        {
                            messages.Add(innerMessage);
                        }
                        else
                        {
                            if (isFound.ContainsKey(innerMessage.Address))
                            {
                                // do nothing.
                            }
                            else
                            {
                                isFound.Add(innerMessage.Address, true);
                                messages.Add(innerMessage);
                            }
                        }
                    }
                }
                else
                {
                    if (FSelect[0] == SelectEnum.All)
                    {
                        messages.Add((OSCMessage) packet);
                    }
                    else
                    {
                        if (isFound.ContainsKey(packet.Address))
                        {
                            // do nothing.
                        }
                        else
                        {
                            isFound.Add(packet.Address, true);
                            messages.Add((OSCMessage) packet);
                        }
                    }
                }
            }

            // all unnecessary messages are gone. now try to distribute them.

            FOutput.SliceCount = 0;
            var bundles = new Dictionary<string, OSCBundle>();
            var singleBundle = new OSCBundle(FExtendedMode[0]);

            foreach (var message in messages)
            {
                if (!FDistribute[0]) singleBundle.Append(message);
                else
                {
                    var a = MainAddress(message.Address);
                    if (!bundles.ContainsKey(a))
                    {
                        bundles.Add(a, new OSCBundle());
                    }
                    bundles[a].Append(message);
                }
            }

            FOutput.SliceCount = 0;

            if (!FDistribute[0])
            {
                FOutput.Add(new MemoryStream((singleBundle.BinaryData)));
            }
            else
            {
                foreach (OSCBundle bundle in bundles.Values)
                {
                    FOutput.Add(new MemoryStream((bundle.BinaryData)));
                }
            }
            FOutput.Flush();
        }

    }

    #region PluginInfo
    [PluginInfo(Name = "UnBundle", Category = "OSC", Help = "UnBundle a bunch of OSC messages", Tags = "velcrome")]
    #endregion PluginInfo
    public class UnBundleOSCNode : IPluginEvaluate
    {
        #pragma warning disable 649, 169
        [Input("Input")]
        IDiffSpread<Stream> FInput;

        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true, BinVisibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FExtendedMode;
            
        [Output("Output", AutoFlush = false)]
        ISpread<ISpread<Stream>> FOutput;

        [Import()]
        protected ILogger FLogger;
        #pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && !FExtendedMode.IsChanged) return;

            if ((FInput.SliceCount == 0) || (FInput[0] == null) || (FInput[0].Length == 0)) return;

            SpreadMax = FInput.SliceCount;
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                FOutput[i].SliceCount = 0;

                MemoryStream ms = new MemoryStream();
                FInput[i].Position = 0;
                FInput[i].CopyTo(ms);
                byte[] bytes = ms.ToArray();
                int start = 0;

                OSCPacket packet = OSCPacket.Unpack(bytes, ref start, (int) FInput[i].Length, FExtendedMode[0]);

                if (packet.IsBundle())
                {
                    var packets = ((OSCBundle) packet).Values;
                    foreach (OSCPacket innerPacket in packets)
                    {
                        MemoryStream memoryStream = new MemoryStream(innerPacket.BinaryData);
                        FOutput[i].Add(memoryStream);
                    }
                }
                else
                {
                    MemoryStream memoryStream = new MemoryStream(packet.BinaryData);
                    FOutput[i].Add(memoryStream);
                }
            }
            FOutput.Flush();
        }
    }


}
