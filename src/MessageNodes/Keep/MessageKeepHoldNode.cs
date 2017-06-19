using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils;
using System.Collections.Generic;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "HoldKeep", Category = "Message", Help = "Holds the last bunch of Messages that traveled through. Pick some or all.",
        Tags = "velcrome")]
    #endregion PluginInfo
    public class MessageKeepHoldNode : AbstractMessageKeepNode
    {
        public enum HoldEnum
        {
            All,
            Index
        }

        [Input("Formular", Visibility = PinVisibility.False, EnumName = MessageFormularRegistry.RegistryName)]
        public override IDiffSpread<EnumEntry> FFormularSelection {
            get;
            set;
        }

        [Input("Match Rule", DefaultEnumEntry = "All", IsSingle = true, Order = 2)]
        protected IDiffSpread<HoldEnum> FHold;

        [Input("Index", Order = int.MaxValue)]
        protected IDiffSpread<int> FIndex;

        public override void Evaluate(int SpreadMax)
        {
            var update = CheckReset();

            if (FInput.IsChanged && !FInput.IsAnyInvalid())
            {
                Keep.AssignFrom(FInput);
                update = true;
            }

            if (UpKeep()) update = true;

            if (FHold.IsChanged ) update = true;
            if (FHold[0] == HoldEnum.Index && FIndex.IsChanged) update = true;

            if (update) 
            {
                switch (FHold[0])
                {
                    case HoldEnum.All:
                        FOutput.SliceCount = 0;
                        FOutput.AssignFrom(Keep);
                        break;
                    case HoldEnum.Index:
                        if (Keep.Count == 0) 
                        {
                            FOutput.SliceCount = 0;
                        } 
                        else 
                        {
                            FOutput.SliceCount = FIndex.SliceCount;
                            for (int i = 0; i < FIndex.SliceCount;i++ )
                            {
                                var index = VMath.Zmod(FIndex[i], Keep.Count);
                                FOutput[i] = Keep[index];
                            }
                        }
                        break;
                }
                FOutput.Flush();
            }
         }

    }
}