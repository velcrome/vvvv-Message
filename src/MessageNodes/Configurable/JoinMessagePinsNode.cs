using System;
using System.Collections;
using VVVV.Packs.Message.Core.Formular;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Message", AutoEvaluate=true, Category = "Join", Help = "Joins a Message from custom dynamic pins", Tags = "Dynamic, Bin", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageJoinNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("Send", IsToggle = true, IsSingle = true, DefaultBoolean = true)]
        ISpread<bool> FSet;

        [Input("Address", DefaultString = "Event")]
        ISpread<string> FAddress;

        [Output("Output", AutoFlush = false)]
        Pin<Core.Message> FOutput;
#pragma warning restore

        protected override IOAttribute DefinePin(string name, Type type, int binSize = -1)
        {
            var attr = new InputAttribute(name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = binSize;
            attr.Order = FCount;
            attr.BinOrder = FCount + 1;
            attr.AutoValidate = false;  // need to sync all pins manually. Don't forget to Flush()

            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = 0;
            if (!FSet[0])
            {
                //if (FInput.IsChanged)
                //{

                //}
                
                FOutput.SliceCount = 0;
                FOutput.Flush();
                return;
            }

            foreach (string name in FPins.Keys)
            {
                var pin = ToISpread(FPins[name]);
                pin.Sync();
                SpreadMax = Math.Max(pin.SliceCount, SpreadMax);
            }


            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                Core.Message message = new Core.Message();

                message.Address = FAddress[i];
                foreach (string name in FPins.Keys)
                {
                    message.AssignFrom(name, (IEnumerable)ToISpread(FPins[name])[i]);
                }
                FOutput[i] = message;

                // FLogger.Log(LogType.Debug, "== Message "+i+" == \n" + message.ToString());
                //	foreach (string name in message.GetDynamicMemberNames()) FLogger.Log(LogType.Debug, message[name].GetType()+" "+ name);
            }
            FOutput.Flush();
        }
    }
}