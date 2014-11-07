using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Registry;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes
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
                return;
            }
            SpreadMax = FName.SliceCount;

            for (int i = 0; i < SpreadMax; i++)
            {
                TypeRegistry.Instance.Define(FName[i], FConfig[i]);
            }
        }
    }

}
