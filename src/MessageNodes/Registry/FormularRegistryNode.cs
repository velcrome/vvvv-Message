using System.Linq;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Formular;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "MessageType", AutoEvaluate = true, Category = "Message", Help = "Define a high level Template for Messages", Tags = "Dynamic, Bin, velcrome")]
    #endregion PluginInfo
    public class FormularRegistryNode : IPluginEvaluate
    {
        [Input("Type Name", DefaultString = "Event")]
        public ISpread<string> FName;

        [Input("Configuration", DefaultString = "string Foo")]
        public ISpread<string> FConfig;

        [Input("Update", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        public IDiffSpread<bool> FUpdate;

        private bool firstFrame = true;

        public void Evaluate(int SpreadMax)
        {
            if (!FUpdate[0] && !firstFrame)
            {
                return;
            }
            SpreadMax = FName.SliceCount;

            var reg = MessageFormularRegistry.Instance;
            for (int i = 0; i < SpreadMax; i++)
            {
                reg.Define(FName[i], FConfig[i], firstFrame);
            }

            firstFrame = false;
            if (SpreadMax > 0) EnumManager.UpdateEnum(reg.RegistryName, reg.Keys.First(), reg.Keys.ToArray());

        }
    }

}
