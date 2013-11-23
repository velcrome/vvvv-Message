using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Nodes.Messaging.Typing;
using VVVV.Pack.Messaging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Messaging.Type
{

    #region PluginInfo
    [PluginInfo(Name = "MessageType", AutoEvaluate = true, Category = "Message", Help = "Define a high level Template for Messages", Tags = "Dynamic, Bin, velcrome")]
    #endregion PluginInfo
    public class MessageTypeMessageNode : IPluginEvaluate
    {
        [Input("Type Name", DefaultString = "Event")]
        public ISpread<string> FName;

        [Input("Configuration", DefaultString = "string Foo")]
        public ISpread<string> FConfig;

        [Input("Update", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        public IDiffSpread<bool> FUpdate;

        public void Evaluate(int SpreadMax)
        {
            if (!FUpdate[0])
            {
                if (FUpdate.IsChanged) TypeDictionary.IsChanged = false; // has updated last frame, but not anymore
                return;
            }
            SpreadMax = FName.SliceCount;

            TypeDictionary.IsChanged = true;
            for (int i = 0; i < SpreadMax; i++)
            {
                var dict = TypeDictionary.Instance;

                if (dict.ContainsKey(FName[i]))
                    dict[FName[i]] = FConfig[i];
                else dict.Add(FName[i], FConfig[i]);
            }
        }
    }

}
