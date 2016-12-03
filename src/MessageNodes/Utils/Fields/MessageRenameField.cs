using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "RenameField", Category = "Message", Help = "Renames specific fields inside a Message.", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageFieldRename : IPluginEvaluate
    {
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Input("Match Field Name", DefaultString = "Foo")]
        protected ISpread<string> FOld;

        [Input("New Field Name", DefaultString = "Bar")]
        protected ISpread<string> FNew;

        [Input("Force Overwrite", DefaultBoolean = true, IsSingle = true, IsToggle = true)]
        protected ISpread<bool> FOverwrite;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;

        public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;

            if (SpreadMax <= 0)
            {
                FOutput.FlushNil();
                return;
            }


            var translate = new Dictionary<string, string>();

            var max = FNew.CombineWith(FOld);
            for (int i = 0; i < max; i++)
            {
                var n = FNew[i].Trim();
                var o = FOld[i].Trim();

                if (string.IsNullOrWhiteSpace(n) || string.IsNullOrWhiteSpace(o) || !n.IsValidFieldName() )
                    throw new ParseFormularException("\"" + n + "\" is not a valid name for a Message's field. Only use alphanumerics, dots, hyphens and underscores. ");

                translate[o] = n;
            }

            foreach (var message in FInput)
            {
                foreach (var fieldName in message.Fields.ToArray())
                {
                    if (translate.ContainsKey(fieldName))
                    {
                        var success = message.Rename(fieldName, translate[fieldName], FOverwrite[0]);

                        if (!success) FLogger.Log(LogType.Error, "Cannot rename " + fieldName + " to " + translate[fieldName] + " because it already exists.");
                    }
                }
            }

            FOutput.FlushResult(FInput);
        }
    }

}