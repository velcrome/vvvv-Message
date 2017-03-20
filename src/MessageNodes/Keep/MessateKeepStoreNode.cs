using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "StoreKeep", Category = "Message", Help = "Holds all Messages that traveled through.",
        Tags = "velcrome")]
    #endregion PluginInfo
    public class MessageKeepStoreNode : AbstractMessageKeepNode
    {
        [Input("Formular", Visibility = PinVisibility.False)]
        public override IDiffSpread<EnumEntry> FFormularSelection
        {
            get;
            set;
        }


        public override void Evaluate(int SpreadMax)
        {
            var update = CheckReset();

            if (FInput.IsChanged && !FInput.IsAnyInvalid())
            {
                Keep.AssignFrom(FInput);
                update = true;
            }
            if (UpKeep()) update = true;

            if (update) FOutput.FlushResult(Keep);
        }

    }
}