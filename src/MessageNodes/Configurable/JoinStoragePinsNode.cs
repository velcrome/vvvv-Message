using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.Packs.Message.Core.Formular;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Storage", AutoEvaluate = true, Category = "Join", Help = "Joins a permanent Message from custom dynamic pins", Tags = "Dynamic, Bin", Author = "velcrome")]
    #endregion PluginInfo
    public class StorageJoinNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("Spread Count", IsSingle = true, DefaultValue = 1)]
        ISpread<int> FSpreadCount;

        [Input("Address", DefaultString = "State")]
        ISpread<string> FAddress;

        [Output("Output", AutoFlush = false)]
        Pin<Core.Message> FOutput;

        protected List<Core.Message> messages = new List<Core.Message>();
        protected bool Changed = true;

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
            if (FConfig.IsChanged) Changed = true;
            
            SpreadMax = FSpreadCount[0];
            for (int j=messages.Count;j<SpreadMax;j++)
            {
                Changed = true;
                messages.Add(new Core.Message(new MessageFormular(FConfig[0] )));
            }

            foreach (string name in FPins.Keys)
            {
                var pin = ToISpread(FPins[name]);
                pin.Sync();
                if (pin.IsChanged) Changed = true;
            }


            FOutput.SliceCount = SpreadMax;
            int i = 0;
            if (Changed) foreach (var message in messages)
            {
                message.Address = FAddress[i];
                foreach (string name in FPins.Keys)
                {
                    message.AssignFrom(name, (IEnumerable)ToISpread(FPins[name])[i]);
                }
                FOutput[i] = message;

                // FLogger.Log(LogType.Debug, "== Message "+i+" == \n" + message.ToString());
                //	foreach (string name in message.GetDynamicMemberNames()) FLogger.Log(LogType.Debug, message[name].GetType()+" "+ name);
                i++;
            }
            if (Changed) FOutput.Flush();

            Changed = false;

        }
    }
}