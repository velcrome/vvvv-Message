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

        [Input("Update", IsBang = true)]
        ISpread<bool> FReset;

        [Output("Output", AutoFlush = false)]
        Pin<Core.Message> FOutput;

        protected List<Core.Message> messages = new List<Core.Message>();

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
            bool allChanged = FConfig.IsChanged;

            SpreadMax = FSpreadCount[0];

            if (SpreadMax <= 0)
            {
                messages.Clear();
                FOutput.SliceCount = 0;
                FOutput.Flush();
                return;
            }

            var changed = FReset.ToSpread();
            changed.ResizeAndDismiss(SpreadMax);

            for (int j = messages.Count; j < SpreadMax; j++)
            {
                changed[j] = true;
                messages.Add(new Core.Message(new MessageFormular(FConfig[0])));
            }

            bool anyChanged = false; // pull input pins only when change detected
            
            for (int j = 0; j < changed.SliceCount; j++) if (changed[j] || allChanged)
            {
                anyChanged = true;
                SyncPins();
                break;
            }
            FOutput.SliceCount = SpreadMax;

            int i = 0;
            foreach (var message in messages)
            {
                if (changed[i] || allChanged )
                {
                    message.Address = FAddress[i];
                    message.TimeStamp = Time.Time.CurrentTime();
                    foreach (string name in FPins.Keys)
                        message.AssignFrom(name, (IEnumerable) ToISpread(FPins[name])[i]);

                }
                FOutput[i] = message;
                i++;
            }

            if (anyChanged)
            {
                FOutput.Flush();
            }


    }
    }
}