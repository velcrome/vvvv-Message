using System;
using System.Linq;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Create", AutoEvaluate=true, Category = "Message", Version="Formular", Help = "Joins a Message from custom dynamic pins", Tags = "Dynamic, Bin", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageCreateNode : TypeablePinsNode
    {
        [Input("New", IsToggle = true, DefaultBoolean = true, Order = 0)]
        protected ISpread<bool> FNew;

        [Input("Topic", DefaultString = "Event", Order = 3)]
        protected ISpread<string> FTopic;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        protected override IOAttribute SetPinAttributes(FormularFieldDescriptor field)
        {
            var attr = new InputAttribute(field.Name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = field.DefaultSize;

            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount + 1;

            attr.CheckIfChanged = false; // very lazy inputs. only do something when New is hit.
            attr.AutoValidate = false;

            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            InitDX11Graph();

            bool warnPinSafety = false;
            if (RemovePinsFirst) warnPinSafety = !RetryConfig();

            if (!FNew.Any()) // if none true
            {
                FOutput.FlushNil();
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
                        if (pin.SliceCount > 0)
                            message.AssignFrom(name, pin[i] as ISpread); 
                        else message[name] = BinFactory.New(Formular[name].Type); // will do empty spreads as well, but ignore faults
                    }
                    FOutput.Add(message);
                }
            }
            FOutput.Flush();

            if (warnPinSafety)
                throw new PinConnectionException("Manually remove unneeded links first! [Create]. ID = [" + PluginHost.GetNodePath(false) + "]");

        }


    }
}