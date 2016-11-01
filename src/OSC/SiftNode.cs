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

        [Output("Not found", AutoFlush = false)]
        ISpread<Stream> FOther;

        [Import]
        ILogger FLogger;
#pragma warning restore

        #endregion fields & pins



        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            if (FInput.IsAnyInvalid())
            {
                if (FOutput.SliceCount > 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                if (FOther.SliceCount > 0)
                {
                    FOther.SliceCount = 0;
                    FOther.Flush();
                }
                return;
            }

            FOther.SliceCount = FOutput.SliceCount = 0;
            SpreadMax = FInput.SliceCount;

            if ((FInput.SliceCount == 0) || (FInput[0] == null) || (FInput[0].Length == 0)) return;

            for (int i = 0; i < SpreadMax; i++)
            {
                MemoryStream ms = new MemoryComStream();
                FInput[i].Position = 0;
                FInput[i].CopyTo(ms);
                byte[] bytes = ms.ToArray();
                int start = 0;

                OSCPacket packet = OSCPacket.Unpack(bytes, ref start, (int)ms.Length, FExtendedMode[0]);


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
                else FOther.Add(ms);
            }

            FOutput.Flush();
            FOther.Flush();
        }

    }
}
