using System.Linq;
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

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            (FWindow as FormularLayoutPanel).Locked = true;
        }

        protected override void OnConfigChange(IDiffSpread<string> config) {
            _changed = true;
        }

        protected override void OnSelectFormular(IDiffSpread<EnumEntry> spread)
        {
            base.OnSelectFormular(spread);

            var window = (FWindow as FormularLayoutPanel);
            var fields = window.Controls.OfType<FieldPanel>();

            foreach (var field in fields) field.Checked = true;
            window.Locked = FFormular[0] != MessageFormular.DYNAMIC;
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
                
                var formularName = FFormular[i].Name;
                var def = registry[formularName];
                if (def != null)
                {
                    var descriptors = def.FieldDescriptors;

                    FFieldName[i].AssignFrom(from field in descriptors select field.Name);

                    FDefaultSize.AddRange(from field in descriptors select field.DefaultSize);
                    FFieldType.AddRange(from field in descriptors select TypeIdentity.Instance.FindAlias(field.Type));
                    FBinDef.AddRange(from field in descriptors select GetBinDefString(field.DefaultSize));
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
