using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Messaging;

namespace VVVV.Nodes.Messaging
{
    [PluginInfo(Name = "GetAttribute", Category = "Message", Help = "Builds a spread across multiple Messages", Tags = "velcrome, float")]
    public class FloatMessageGetAttributeNode : MessageGetAttributeNode<float>
    { }

    [PluginInfo(Name = "GetAttribute", Category = "Message", Help = "Spreads multiple Messages", Tags = "velcrome, string")]
    public class StringMessageGetAttributeNode : MessageGetAttributeNode<float>
    { }

    [PluginInfo(Name = "GetAttribute", Category = "Message", Help = "Spreads multiple Messages", Tags = "velcrome, int")]
    public class IntMessageGetAttributeNode : MessageGetAttributeNode<float>
    { }

    public class MessageGetAttributeNode<T> : IPluginEvaluate
    {
        [Input("Input")]
        IDiffSpread<Message> FInput;

        [Input("Default")]
        ISpread<T> FDefault;

        [Input("Attribute Name")]
        IDiffSpread<string> FName;

        [Output("Value", AutoFlush = false)]
        ISpread<ISpread<T>> FOutput;

        [Output("IsValid", AutoFlush = false)]
        ISpread<bool> FValid;

        [Import()]
        protected ILogger FLogger;

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && !FDefault.IsChanged && !FName.IsChanged) return;

            if (FName.SliceCount > 0 && FName[0] != null)
                SpreadMax = FName.SliceCount;
            else SpreadMax = 0;

            FValid.SliceCount = SpreadMax;
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                FOutput[i].SliceCount = 0;
                foreach (Message m in FInput)
                {
                    if (m != null && m[FName[i]] != null)
                    {

                        for (int j = 0; j < m[FName[i]].Count; j++) FOutput[i].Add((T)m[FName[i]][j]);
                    }
                }
                if (FOutput[i].SliceCount == 0)
                {
                    FOutput[i].Add(FDefault[i]);
                    FValid[i] = false;
                }
                else FValid[i] = true;
            }


            FOutput.Flush();
            FValid.Flush();

        }
    }


}
