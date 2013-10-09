using System;
using VVVV.Pack.Messaging;
using VVVV.Pack.Messaging.Collections;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;

namespace VVVV.Nodes.Messaging
{
    [PluginInfo(Name = "SetMessage (String)", Category = "Message", Help = "Updates strings of a message", Tags = "Dynamic, velcrome")]
    public class SetMessageNodeString : SetMessageNode<string> { }

    [PluginInfo(Name = "SetMessage (Value)", Category = "Message", Help = "Updates values of a message", Tags = "Dynamic, velcrome")]
    public class SetMessageNodeValue : SetMessageNode<double> { }

    [PluginInfo(Name = "SetMessage (Color)", Category = "Message", Help = "Updates colors of a message", Tags = "Dynamic, velcrome")]
    public class SetMessageNodeColor : SetMessageNode<RGBAColor> { }

    public class SetMessageNode<T> : IPluginEvaluate
    {
        #pragma warning disable 649, 169

        [Input("Input")]
        IDiffSpread<Message> FInput;

        [Input("Key", DefaultString = "Foo")]
        public ISpread<string> FKey;

        [Input("Value")]
        public ISpread<ISpread<T>> FValue;

        [Input("Update", IsBang = true)]
        ISpread<bool> Update;

        [Output("Output", AutoFlush = false)]
        Pin<Message> FOutput;

        #pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.SliceCount;

            if (FInput.SliceCount == 0 || FInput[0] == null)
            {
                FOutput.SliceCount = 0;
                return;
            }

            FOutput.SliceCount = SpreadMax;


            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];

                if (Update[i])
                {
                    for (int keyCount = 0; keyCount < FKey.SliceCount; keyCount++)
                    {

                        SpreadList attr = message[FKey[keyCount]];
                        if (attr != null)
                        {
                            var typeOld = attr[0].GetType();
                            var typeNew = FValue[keyCount][0].GetType();
                            if (!(typeOld == typeNew))
                            {
                                attr.Clear();
                                for (int j = 0; j < FValue[i].SliceCount; j++)
                                {
                                    attr.Add(Convert.ChangeType(FValue[keyCount][j], typeOld));
                                }
                            }
                            else
                            {
                                attr.Clear();
                                attr.AssignFrom(FValue[i]);
                            }

                        }

                    }
                }
                FOutput[i] = message;


            }

            FOutput.Flush();

        }
    }

}
