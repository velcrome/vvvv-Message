using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "Reflection", AutoEvaluate = true, Category = "Formular", Help = "Outputs the type of a given Field in a Formular", Tags = "Message", Author =  "velcrome")]
    #endregion PluginInfo
    public class FormularReflectionNode : AbstractFormularableNode, IPluginEvaluate
    {
        #pragma warning disable 649, 169
        [Input("Field Name", DefaultString = "Foo")]
        public ISpread<string> FFieldName;

        [Output("Variable Type")]
        ISpread<string> FOutput;
        #pragma warning restore

        protected override void HandleConfigChange(IDiffSpread<string> config) {

        }


        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = FFieldName.CombineWith(FType);
            FOutput.SliceCount = SpreadMax;
            var registry = MessageFormularRegistry.Instance;
            for (int i = 0; i < SpreadMax; i++)
            {
                var formular = FType[i].Name;
                if (registry.ContainsKey(formular))
                {
                    var fieldName = FFieldName[i];
                    if (registry[formular].Fields.Contains(fieldName))
                    {
                        var type = registry[formular][fieldName].Type;
                        FOutput[i] = TypeIdentity.Instance.FindAlias(type);
                    }
                    else FOutput[i] = "";
                }
                else FOutput.SliceCount = 0;
            }
        }
    }
}
