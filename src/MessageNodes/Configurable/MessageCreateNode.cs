using System;
using System.Collections;
using VVVV.Packs.Messaging.Core;
using VVVV.Packs.Messaging.Core.Formular;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Create", AutoEvaluate=true, Category = "Message", Version="Formular", Help = "Joins a Message from custom dynamic pins", Tags = "Dynamic, Bin", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageJoinNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("Send", IsToggle = true, IsSingle = true, DefaultBoolean = true, Order = 0)]
        ISpread<bool> FSet;

        [Input("Address", DefaultString = "Event", Order = 3)]
        ISpread<string> FAddress;

        [Output("Output", AutoFlush = false)]
        Pin<Message> FOutput;
#pragma warning restore

        protected override IOAttribute DefinePin(FormularFieldDescriptor configuration)
        {
            var attr = new InputAttribute(configuration.Name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = configuration.DefaultSize;
            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount + 1;

//         Manual Sync seems a good idea, but had some glitches, because binsize for Color and string will only be updated, if actual input had changed.  
//           attr.AutoValidate = false;  // need to sync all pins manually. Don't forget to Flush()

            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = 0;
            if (!FSet[0])
            {
               
                FOutput.SliceCount = 0;
                FOutput.Flush();
                return;
            }

            foreach (string name in FPins.Keys)
            {
                var pin = ToISpread(FPins[name]);
//                pin.Sync();
                SpreadMax = Math.Max(pin.SliceCount, SpreadMax);
            }


            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = new Message();

                message.Address = FAddress[i];
                foreach (string name in FPins.Keys)
                {
                    var spread = ToISpread(FPins[name])[i] as VVVV.PluginInterfaces.V2.NonGeneric.ISpread;
                    message.AssignFrom(name, spread.ToEnumerable());
                }
                FOutput[i] = message;
            }
            FOutput.Flush();
        }
    }
}