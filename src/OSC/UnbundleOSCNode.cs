using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

using VVVV.PluginInterfaces.V2;

using VVVV.Utils;
using VVVV.Core.Logging;
using VVVV.Utils.OSC;

namespace VVVV.Nodes.OSC
{
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
            if (FInput.IsAnyInvalid() || FExtendedMode.IsAnyInvalid()) SpreadMax = 0;
            else SpreadMax = FInput.SliceCount;

            if (SpreadMax == 0)
            {
                if (FOutput.SliceCount != 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                return;
            }

            if (!FInput.IsChanged && !FExtendedMode.IsChanged)
            {
                return;
            }

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
