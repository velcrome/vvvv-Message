using System.ComponentModel.Composition;
using System.Linq;

using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Sift", Category = "Message", Help = "Filter Messages", Tags = "string, Wildcard", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageSiftWildCardNode : IPluginEvaluate
    {
        [Input("Input")] 
        protected IDiffSpread<Message> FInput;

        [Input("Filter", DefaultString = "*")]
        protected IDiffSpread<string> FFilter;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        [Output("NotFound", AutoFlush = false)]
        protected ISpread<Message> FNotFound;

        [Import()] protected ILogger FLogger;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsAnyInvalid() || FFilter.IsAnyInvalid()) 
                SpreadMax = 0;
                else SpreadMax = FInput.SliceCount;
            
            if (SpreadMax == 0)
            {
                FOutput.FlushNil();
                FNotFound.FlushNil();
                return;
            }

            if (!FInput.IsChanged && !FFilter.IsChanged) return;

            FOutput.SliceCount = 0;
            FNotFound.SliceCount = 0;

            bool[] found = new bool[SpreadMax];
            for (int i = 0; i < SpreadMax; i++) found[i] = false;

            for (int i = 0; i < FFilter.SliceCount; i++)
            {
                string[] filter = FFilter[i].Split('.');
                var regex = FFilter[i].CreateWildCardRegex();
                
                for (int j = 0; j < SpreadMax; j++)
                {
                    if (!found[j]) found[j] = regex.IsMatch(FInput[j].Topic);
                }
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                if (found[i]) FOutput.Add(FInput[i]);
                else FNotFound.Add(FInput[i]);
            }
            FOutput.Flush();
            FNotFound.Flush();
        }
    }

 
}