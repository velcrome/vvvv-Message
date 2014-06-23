using System;
using VVVV.Packs.Message;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Packs.Message.Nodes
{
    [PluginInfo(Name = "SetMessage (String)", Category = "Message", Help = "Updates strings of a message", Tags = "Dynamic, velcrome")]
    public class SetMessageNodeString : SetMessageNode<string> { }

    [PluginInfo(Name = "SetMessage (Value)", Category = "Message", Help = "Updates values of a message", Tags = "Dynamic, velcrome")]
    public class SetMessageNodeValue : SetMessageNode<double> { }
	
	[PluginInfo(Name = "SetMessage (Vector2D)", Category = "Message", Help = "Updates values of a message", Tags = "Dynamic, velcrome")]
    public class SetMessageNodeVector2D : SetMessageNode<Vector2D> { }

    [PluginInfo(Name = "SetMessage (Color)", Category = "Message", Help = "Updates colors of a message", Tags = "Dynamic, velcrome")]
    public class SetMessageNodeColor : SetMessageNode<RGBAColor> { }

    public class SetMessageNode<T> : IPluginEvaluate
    {
        #pragma warning disable 649, 169

        [Input("Input")]
        IDiffSpread<Message> FInput;

        [Input("Key", DefaultString = "Foo", IsSingle = true)]
        public ISpread<string> FKey;

        [Input("Value")]
        public ISpread<ISpread<T>> FValue;

        [Input("Update", IsBang = true)]
        ISpread<bool> FUpdate;

        [Output("Output", AutoFlush = false)]
        Pin<Message> FOutput;

        #pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.SliceCount;

            if (FInput.SliceCount == 0 || FInput[0] == null)
            {
                FOutput.SliceCount = 0;
            	FOutput.Flush();
                return;
            }

            FOutput.SliceCount = SpreadMax;


            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];

                if (FUpdate[i])
                {
                    SpreadList attr = message[FKey[0]];  
                        
                    if (attr != null)
                    {
                        if (attr.SpreadType != typeof(T)) // does not mean it mismatches. can still be both "Value" for example
                        {
                            attr.Clear();
                            for (int j = 0; j < FValue[i].SliceCount; j++)
                            {

                                attr.Add( Convert.ChangeType(FValue[i][j], attr.SpreadType));
                            }
                        }
                        else
                        {
                            attr.Clear();
                            attr.AssignFrom(FValue[i]);
                        }

                    } else
                    {
                        message.AddFrom(FKey[0], FValue[i]);
                    }
                }
                FOutput[i] = message;


            }

            FOutput.Flush();

        }
    }

}
