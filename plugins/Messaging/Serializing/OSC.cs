using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VVVV.Pack.Messaging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Messaging.Serializing
{

    #region PluginInfo
    [PluginInfo(Name = "AsOSC", Category = "Message", Help = "Outputs OSC Bundle Strings", Tags = "Dynamic, OSC, velcrome")]
    #endregion PluginInfo
    public class MessageMessageAsOscNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        IDiffSpread<Message> FInput;

        [Output("OSC", AutoFlush = false)]
        ISpread<Stream> FOutput;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FInput.SliceCount <= 0 || FInput[0] == null) SpreadMax = 0;
            else SpreadMax = FInput.SliceCount;

            if (!FInput.IsChanged) return;
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                FOutput[i] = FInput[i].ToOSC();
            }
            FOutput.Flush();
        }
    }


    #region PluginInfo
    [PluginInfo(Name = "AsMessage", Category = "Message, OSC", Help = "Converts OSC Bundles into Messages ", Tags = "Dynamic, OSC, velcrome")]
    #endregion PluginInfo
    public class MessageOscAsMessageNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("OSC")]
        IDiffSpread<Stream> FInput;

        [Input("Additional Address", DefaultString = "OSC", IsSingle = true)]
        IDiffSpread<string> FAddress;

        [Input("Contract Address Parts", DefaultValue = 1, IsSingle = true, MinValue = 1)]
        IDiffSpread<int> FContract;

        [Output("Output", AutoFlush = false)]
        ISpread<Message> FOutput;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {

            if (!FInput.IsChanged && !FAddress.IsChanged && !FContract.IsChanged) return;
            if ((FInput.SliceCount == 0) || (FInput[0] == null) || (FInput[0].Length == 0)) return;


            if (FInput.SliceCount <= 0 || FInput[0] == null) SpreadMax = 0;
            else SpreadMax = FInput.SliceCount;

            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = Message.FromOSC(FInput[i], FAddress[0], FContract[0]);
                FOutput[i] = message;
            }
            FOutput.Flush();
        }
    }

}
