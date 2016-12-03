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
        protected Pin<Message> FInput;

        [Input("Verbose Logging", Order = int.MaxValue, DefaultBoolean = true, IsSingle = true)]
        protected IDiffSpread<bool> FVerbose;

        [Output("Topic", AutoFlush = false, Visibility = PinVisibility.Hidden, Order = 1)]
        protected ISpread<string> FTopic;

        [Output("Timestamp", AutoFlush = false, Visibility = PinVisibility.OnlyInspector, Order = 2)] 
        protected ISpread<Time> FTimeStamp;

        protected bool LayoutChanged;

        protected override IOAttribute SetPinAttributes(FormularFieldDescriptor configuration)
        {
            var attr = new OutputAttribute(configuration.Name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.AutoFlush = false;

            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount + 1;
            return attr;
        }

        protected override IIOContainer CreatePin(FormularFieldDescriptor field)
        {
            LayoutChanged = true; // this forces Evaluate to push data to output, even though no new messages got pushed
            return base.CreatePin(field); 
        }

        public override void Evaluate(int SpreadMax)
        {
            // quit early. this will keep the last valid output until situation is resolved
            if (RemovePinsFirst)
            {
                if (!RetryConfig()) throw new PinConnectionException("Manually remove unneeded links first! [Split]. ID = [" + PluginHost.GetNodePath(false) + "]");
                else LayoutChanged = true;
            }

            if (!FInput.IsChanged && !LayoutChanged) return;
            
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            if (SpreadMax <= 0)
            {
                foreach (string name in FPins.Keys)
                    FPins[name].ToISpread().FlushNil();

                FTopic.FlushNil();
                FTimeStamp.FlushNil();
                return;
            }

            FTimeStamp.SliceCount = SpreadMax;
            FTopic.SliceCount = SpreadMax;
            foreach (string name in FPins.Keys) FPins[name].ToISpread().SliceCount = SpreadMax;

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
                        if (FVerbose[0]) FLogger.Log(LogType.Warning,
                                        "\"" + Formular[name].Type + " " + name + "\" is not defined in Message ["+ message.Topic+"], so skipped its bin on the output too.");
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

            // no need to worry next frame. all's well, because node failed early
            LayoutChanged = false;
        }

    }
}