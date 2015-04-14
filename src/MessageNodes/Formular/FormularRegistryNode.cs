using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "Formular", AutoEvaluate = true, Category = "Message", Help = "Define a high level Template for Messages", Tags = "Dynamic, Bin, velcrome")]
    #endregion PluginInfo
    public class FormularRegistryNode : IPluginEvaluate
    {
        [Input("Formular Name", DefaultString = "Event")]
        public ISpread<string> FName;

        [Input("Configuration", DefaultString = "string Foo", BinSize=1)]
        public ISpread<ISpread<string>> FConfig;

        [Input("Clear All", IsSingle = true, IsBang = true, DefaultBoolean = false, Visibility = PinVisibility.Hidden)]
        public IDiffSpread<bool> FClear;

        [Input("Update", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        public IDiffSpread<bool> FUpdate;

        private bool firstFrame = true;

        public void Evaluate(int SpreadMax)
        {
            
            if (!FClear.IsAnyInvalid() && FClear[0])
            {
                var registry = MessageFormularRegistry.Instance;
                
                var none = registry[MessageFormular.DYNAMIC];
                registry.Clear();

                registry.Add(MessageFormular.DYNAMIC, none);
                EnumManager.UpdateEnum(registry.RegistryName, MessageFormular.DYNAMIC, registry.Keys.ToArray());
            }
            
            if (FUpdate.IsAnyInvalid() || FConfig.IsAnyInvalid()) return;
            if (!FUpdate[0] && !firstFrame) return; 

            SpreadMax = FName.SliceCount;

            var reg = MessageFormularRegistry.Instance; 

            for (int i = 0; i < SpreadMax; i++)
            {
                var config = "";
                for (int j = 0; j < FConfig[i].SliceCount; j++)
                {
                    config += FConfig[i][j] + ", ";
                }
                if (config != "") config = config.Substring(0, config.Length - 2); // remove tailing comma

                reg.Define(FName[i], config, firstFrame);
            }

            firstFrame = false;
            if (SpreadMax > 0) EnumManager.UpdateEnum(reg.RegistryName, reg.Keys.First(), reg.Keys.ToArray());

        }
    }

}
