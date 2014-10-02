using System.ComponentModel.Composition;
using System.IO;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using VVVV.Packs.Message.Core;


namespace VVVV.Nodes.Messaging
{
    [PluginInfo(Name = "GetAttribute", Version = "Value", Category = "Message", Help = "Builds a spread across multiple Messages", Tags = "velcrome")]
    public class ValueMessageGetAttributeNode : MessageGetAttributeNode<double>
    { }

    [PluginInfo(Name = "GetAttribute", Version = "String", Category = "Message", Help = "Spreads multiple Messages", Tags = "velcrome")]
    public class StringMessageGetAttributeNode : MessageGetAttributeNode<string>
    { }

    [PluginInfo(Name = "GetAttribute", Version="Color", Category = "Message", Help = "Spreads multiple Messages", Tags = "velcrome")]
    public class ColorMessageGetAttributeNode : MessageGetAttributeNode<RGBAColor>
    { }

    [PluginInfo(Name = "GetAttribute", Version = "Raw", Category = "Message", Help = "Spreads multiple Messages", Tags = "velcrome")]
    public class RawMessageGetAttributeNode : MessageGetAttributeNode<Stream>
    { }

    [PluginInfo(Name = "GetAttribute", Version = "Vector2D", Category = "Message", Help = "Spreads multiple Messages", Tags = "velcrome")]
    public class Vector2DMessageGetAttributeNode : MessageGetAttributeNode<Vector2D>
    { }

    [PluginInfo(Name = "GetAttribute", Version = "Vector3D", Category = "Message", Help = "Spreads multiple Messages", Tags = "velcrome")]
    public class Vector3DMessageGetAttributeNode : MessageGetAttributeNode<Vector3D>
    { }

    [PluginInfo(Name = "GetAttribute", Version = "Vector4D", Category = "Message", Help = "Spreads multiple Messages", Tags = "velcrome")]
    public class Vector4DMessageGetAttributeNode : MessageGetAttributeNode<Vector4D>
    { }

    [PluginInfo(Name = "GetAttribute", Version = "Transform", Category = "Message", Help = "Spreads multiple Messages", Tags = "velcrome")]
    public class TransformMessageGetAttributeNode : MessageGetAttributeNode<Matrix4x4>
    { }

//    [PluginInfo(Name = "GetAttribute", Version = "Message", Category = "Message", Help = "Spreads multiple Messages", Tags = "velcrome")]
    public class MessageMessageGetAttributeNode : MessageGetAttributeNode<Message>
    { }

    public class MessageGetAttributeNode<T> : IPluginEvaluate
    {
        #pragma warning disable 649, 169
        
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

        #pragma warning restore

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
