using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "ContainsField", Category = "Message", Help = "Tells if a Message contains a certain attribute indicated by Name", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageContainsFieldNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private IDiffSpread<Message> FInput;

        [Input("Field Names", DefaultString = "Foo")]
        private ISpread<string> FFilter;

        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;

        [Output("Match", AutoFlush = false)]
        private ISpread<bool> FMatch;

        [Import()]
        protected ILogger FLogger;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;

            if (SpreadMax <= 0)
            {
                FMatch.FlushNil();
                FOutput.FlushNil();

                return;
            }
            FMatch.SliceCount = FOutput.SliceCount = 0;

            foreach (var message in FInput)
            {
                var match = new bool[FFilter.SliceCount];
                for (int i = 0;i<FFilter.SliceCount;i++)
                    match[i] = message.Fields.Contains(FFilter[i]);

                if (match.Any())
                {
                    FOutput.Add(message);
                    FMatch.AddRange(match);
                }
            }

            FOutput.Flush();
            FMatch.Flush();
        }
    }
}