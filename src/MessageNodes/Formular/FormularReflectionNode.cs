using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "Reflection", AutoEvaluate = true, Category = "Message", Help = "Outputs the current configuration of a Formular", Tags = "Formular", Author =  "velcrome")]
    #endregion PluginInfo
    public class FormularReflectionNode : AbstractFormularableNode, IPluginEvaluate
    {
        #pragma warning disable 649, 169
        [Output("Field Type", AutoFlush=false)]
        ISpread<string> FFieldType;

        [Output("Bin Definition", AutoFlush = false)]
        public ISpread<string> FBinDef;

        [Output("Field Default Size", AutoFlush = false)]
        public ISpread<int> FDefaultSize;

        [Output("Field", AutoFlush = false)]
        public ISpread<ISpread<string>> FFieldName;
        #pragma warning restore

        private bool _changed = true;

        protected override void OnConfigChange(IDiffSpread<string> config) {
            _changed = true;
        }


        public override void Evaluate(int SpreadMax)
        {

            if (!_changed) return;
            _changed = false;
            
            SpreadMax = FFieldName.SliceCount = FFormular.SliceCount;
            FFieldType.SliceCount = FBinDef.SliceCount = FDefaultSize.SliceCount = 0;

            var registry = MessageFormularRegistry.Instance;
            for (int i = 0; i < SpreadMax; i++)
            {
                FFieldName[i].SliceCount = 0;
                
                var formular = FFormular[i].Name;
                if (registry.ContainsKey(formular))
                {

                    var f = registry[formular];
                    foreach (var field in f.FieldNames)
                    {
                        FFieldType.Add(TypeIdentity.Instance.FindAlias(f[field].Type));

                        var size = f[field].DefaultSize;
                        FDefaultSize.Add(size);

                        if (size < 1) FBinDef.Add("[]");
                        if (size == 1) FBinDef.Add("");
                        if (size > 1) FBinDef.Add("["+size.ToString()+"]");

                        FFieldName[i].Add(f[field].Name);
                    }
                }
            }

            FFieldType.Flush();
            FDefaultSize.Flush();
            FBinDef.Flush();
            FFieldName.Flush();
        }
    }
}
