using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Formular;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "GetVariableType", AutoEvaluate = true, Category = "Message", Help = "Outputs the type of a given variable", Tags = "Dynamic, Bin", Author =  "velcrome")]
    #endregion PluginInfo
    public class GetVariableTypeNode : IPluginEvaluate
    {
        #pragma warning disable 649, 169
        [Input("Type", DefaultString = "Event", IsSingle = true)]
        public ISpread<string> FType;

        [Input("Variable Name", DefaultString = "VariableName")]
        public ISpread<string> FVariableName;

        [Output("Variable Type")]
        ISpread<string> FOutput;
        #pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            SpreadMax = FVariableName.SliceCount;
            FOutput.SliceCount = SpreadMax;
            var registry = MessageFormularRegistry.Instance;
            for (int i = 0; i < SpreadMax; i++)
            {
                if (registry.ContainsKey(FType[0]))
                {

                    if (registry[FType[0]].Fields.Contains(FVariableName[i]))
                    {
                        var type = registry[FType[0]].GetType(FVariableName[i]);
                        FOutput[i] = TypeIdentity.Instance.FindAlias(type);
                    }
                    else FOutput[i] = "";
                }
                else FOutput.SliceCount = 0;
            }
        }
    }
}
