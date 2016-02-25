using System;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.PluginInterfaces.V2.NonGeneric;
using System.Collections.Generic;

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

        [Input("Topic", DefaultString = "Event", Order = 3)]
        ISpread<string> FTopic;

        [Output("Output", AutoFlush = false)]
        ISpread<Message> FOutput;

#pragma warning restore

        protected override IOAttribute SetPinAttributes(FormularFieldDescriptor configuration)
        {
            var attr = new InputAttribute(configuration.Name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = configuration.DefaultSize;

            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount + 1;

            attr.CheckIfChanged = false; // very lazy inputs. only do something when New is hit.
            attr.AutoValidate = false;

            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            if (!FNew.Any(x => x)) // if none true
            {
                if (FOutput.SliceCount != 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                return;
            }

            SpreadMax = FNew.CombineWith(FTopic);
            foreach (string name in FPins.Keys)
            {
                var pin = FPins[name].ToISpread();
                pin.Sync();
                SpreadMax = Math.Max(pin.SliceCount, SpreadMax);
            }

            FOutput.SliceCount = 0;
            for (int i = 0; i < SpreadMax; i++)
            {

                if (FNew[i])
                {
                    var message = new Message();

                    message.Topic = FTopic[i];
                    foreach (string name in FPins.Keys)
                    {
                        var pin = FPins[name].ToISpread();

                        if (!pin.IsAnyInvalid() && !(pin[i] as ISpread).IsAnyInvalid())
                            message.AssignFrom(name, pin[i] as ISpread);
                    }
                    FOutput.Add(message);
                }
            }
            FOutput.Flush();
        }
    }
}