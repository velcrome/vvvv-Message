using System;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Create", AutoEvaluate=true, Category = "Message", Version="Formular", Help = "Joins a Message from custom dynamic pins", Tags = "Dynamic, Bin", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageCreateNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("New", IsToggle = true, DefaultBoolean = true, Order = 0)]
        ISpread<bool> FNew;

        [Input("Address", DefaultString = "Event", Order = 3)]
        ISpread<string> FAddress;

        [Output("Output", AutoFlush = false)]
        Pin<Message> FOutput;

        public readonly MessageKeep Keep = new MessageKeep();

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
            if (!FNew.Any(x => x)) // if any true
            {
                FOutput.SliceCount = 0;
                FOutput.Flush();
                return;
            }

            SpreadMax = FNew.CombineWith(FAddress);

            foreach (string name in FPins.Keys)
            {
                var pin = FPins[name].ToISpread();
                pin.Sync();
                SpreadMax = Math.Max(pin.SliceCount, SpreadMax);
            }

            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                var message = new Message();

                message.Address = FAddress[i];
                foreach (string name in FPins.Keys)
                {
                    var pin = FPins[name].ToISpread();
                        
                    if (!pin.IsAnyInvalid()) 
                    message.AssignFrom(name, pin[i] as ISpread);
                }
                FOutput[i] = message;
            }
            FOutput.Flush();
        }
    }
}