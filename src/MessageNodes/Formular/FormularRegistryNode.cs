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

        [Input("Inherits", IsSingle = true)]
        public ISpread<MessageFormular> FInherits;

        [Input("Update", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        public IDiffSpread<bool> FUpdate;

        [Input("Clear All", IsSingle = true, IsBang = true, DefaultBoolean = false, Visibility = PinVisibility.OnlyInspector)]
        public IDiffSpread<bool> FClear;

        [Output("Formular")]
        public ISpread<MessageFormular> FOutput;


        private bool firstFrame = true;

        public void Evaluate(int SpreadMax)
        {
            
            if (!FClear.IsAnyInvalid() && FClear[0])
            {
                var registry = MessageFormularRegistry.Instance;
                registry.Clear();

                EnumManager.UpdateEnum(registry.RegistryName, MessageFormular.DYNAMIC, registry.Keys.ToArray());
            }
            
            if (FUpdate.IsAnyInvalid() || FConfig.IsAnyInvalid()) return;
            if (!FUpdate[0] && !firstFrame) return; 

            FOutput.SliceCount = SpreadMax = FName.SliceCount;

            var reg = MessageFormularRegistry.Instance; 

            for (int i = 0; i < SpreadMax; i++)
            {
                var config = string.Join(", ", FConfig[i]);
                var formular = new MessageFormular(config, FName[i]);

                if (!FInherits.IsAnyInvalid() && FInherits[i] != null)
                    formular.Append(FInherits[i], false);

                FOutput[i] = reg.Define(formular, firstFrame);
            }

            firstFrame = false;
            if (SpreadMax > 0) EnumManager.UpdateEnum(reg.RegistryName, reg.Keys.First(), reg.Keys.ToArray());

        }
    }

}
