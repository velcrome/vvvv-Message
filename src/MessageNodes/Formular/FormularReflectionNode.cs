using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "Reflection", AutoEvaluate = true, Category = "Message", Help = "Outputs the current configuration of a Formular", Version = "Formular", Author =  "velcrome")]
    #endregion PluginInfo
    public class FormularReflectionNode : AbstractFormularableNode
    {
        // spread it
        [Input("Formular", DefaultEnumEntry = "None", EnumName = MessageFormularRegistry.RegistryName, Order = 2, IsSingle = false)]
        public override IDiffSpread<EnumEntry> FFormularSelection
        {
            get;
            set;
        }

        [Output("Field Type", AutoFlush=false)]
        protected ISpread<string> FFieldType;

        [Output("Bin Definition", AutoFlush = false)]
        public ISpread<string> FBinDef;

        [Output("Field Default Size", AutoFlush = false)]
        public ISpread<int> FDefaultSize;

        [Output("Field", AutoFlush = false)]
        public ISpread<ISpread<string>> FFieldName;

        private bool _changed = true;


        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            FormularUpdate += (sender, formular) => _changed = true;
        }

        public override void Evaluate(int SpreadMax)
        {

            if (!_changed && !FFormularSelection.IsChanged) return;
            _changed = false;
            
            SpreadMax = FFieldName.SliceCount = FFormularSelection.SliceCount;
            FFieldType.SliceCount = FBinDef.SliceCount = FDefaultSize.SliceCount = 0;

            var registry = MessageFormularRegistry.Context;
            for (int i = 0; i < SpreadMax; i++)
            {
                FFieldName[i].SliceCount = 0;
                
                var formularName = FFormularSelection[i].Name;
                var def = registry[formularName];
                if (def != null)
                {
                    var descriptors = def.FieldDescriptors;

                    FFieldName[i].AssignFrom(descriptors.Select(f => f.Name));

                    FDefaultSize.AddRange(descriptors.Select(f => f.DefaultSize));
                    FFieldType.AddRange(descriptors.Select(f => TypeIdentity.Instance[f.Type]?.Alias));
                    FBinDef.AddRange(descriptors.Select(f => GetBinDefString(f.DefaultSize)));
                }
            }

            FFieldType.Flush();
            FDefaultSize.Flush();
            FBinDef.Flush();
            FFieldName.Flush();
        }

        private string GetBinDefString(int size)
        {
            string tmp ="";
            if (size < 1) tmp = "[]";
            if (size == 1) tmp = "";
            if (size > 1) tmp = "[" + size.ToString() + "]";

            return tmp;
        }
    }
}
