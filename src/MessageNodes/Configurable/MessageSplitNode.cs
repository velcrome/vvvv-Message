#region usings
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils;

#endregion usings

namespace VVVV.Packs.Messaging.Nodes
{
    using Time = VVVV.Packs.Time.Time;

    #region PluginInfo

    [PluginInfo(Name = "Split", AutoEvaluate = true, Category = "Message", Version = "Formular",
        Help = "Splits a Message into custom dynamic pins", Tags = "Formular, Bin", Author = "velcrome")]

    #endregion PluginInfo

    public class MessageSplitNode : TypeablePinsNode
    {
        [Input("Input", Order = 0)]
        protected IDiffSpread<Message> FInput;

        [Output("Topic", AutoFlush = false, Visibility = PinVisibility.Hidden, Order = 1)]
        protected ISpread<string> FTopic;

        [Output("Timestamp", AutoFlush = false, Visibility = PinVisibility.OnlyInspector, Order = 2)] 
        protected ISpread<Time> FTimeStamp;

        protected override IOAttribute SetPinAttributes(FormularFieldDescriptor configuration)
        {
            var attr = new OutputAttribute(configuration.Name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.AutoFlush = false;

            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount + 1;
            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            bool warnPinSafety = false;
            bool layoutChanged = false;
            if (RemovePinsFirst)
            {
                layoutChanged = RetryConfig();
                warnPinSafety = !layoutChanged;
            }

            // quit early. this will keep the last valid output until situation is resolved
            if (warnPinSafety)
                throw new PinConnectionException("Manually remove unneeded links first! [Split]. ID = [" + PluginHost.GetNodePath(false) + "]");


            if (!FInput.IsChanged || layoutChanged)
            {
                return;
            }
            
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            if (SpreadMax <= 0)
            {
                foreach (string name in FPins.Keys)
                {
                    var pin = FPins[name].ToISpread();
                    pin.SliceCount = 0;
                    pin.Flush();
                }
                FTopic.SliceCount = 0;
                FTimeStamp.SliceCount = 0;

                FTopic.Flush();
                FTimeStamp.Flush();

                return;
            }

            FTimeStamp.SliceCount = SpreadMax;
            FTopic.SliceCount = SpreadMax;

            foreach (string name in FPins.Keys)
            {
                FPins[name].ToISpread().SliceCount = SpreadMax;
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];
                FTopic[i] = message.Topic;
                FTimeStamp[i] = message.TimeStamp;

                foreach (string name in FPins.Keys)
                {
                    var targetPin = FPins[name].ToISpread();
                    var targetBin = targetPin[i] as ISpread;

                    Bin sourceBin = message[name];
                    int count = 0;

                    if (sourceBin as object == null)
                    {
                            FLogger.Log(LogType.Warning,
                                        "\"" + Formular[name].Type + " " + name + "\" is not defined in Input Message.");
                    }
                    else count = sourceBin.Count;

                    targetBin.SliceCount = count;
                    for (int j = 0; j < count; j++)
                    {
                        targetBin[j] = sourceBin[j];
                    }
                }

            }

            FTimeStamp.Flush();
            FTopic.Flush();
            foreach (string name in FPins.Keys)
            {
                FPins[name].ToISpread().Flush();
            }
        }

    }
}