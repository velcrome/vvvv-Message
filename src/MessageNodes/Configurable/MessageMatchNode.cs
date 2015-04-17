using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{

    #region PluginInfo
//    [PluginInfo(Name = "Inject", AutoEvaluate = true, Category = "Message", Version = "Formular", Help = "Injects a Message into a matching other Message", Tags = "", Author = "velcrome")]
    #endregion PluginInfo    
    public class MessageInjectNode : AbstractFormularableNode
    {
        public ISpread<EnumEntry> FUseAsID = null;
        private string EnumName;

        [Input("Input Messages")]
        public IDiffSpread<Message> FInput;

        [Input("Input Keep", AutoValidate=false)]
        public ISpread<Message> FKeep;

        [Output("Output Keep", AutoFlush = false, Order = 0)]
        public ISpread<Message> FOutput;

        [Output("Changed Keep Slice", AutoFlush = false, Order = 1)]
        public ISpread<bool> FChanged;

        [Output("Unknown Messages", AutoFlush=false)]
        public ISpread<Message> FUnknown;

        [Import()]
        protected IIOFactory FIOFactory;


        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            CreateEnumPin("Use as ID", new string[] { "Foo" });
        }

        public void CreateEnumPin(string pinName, string[] entries)
        {
            EnumName = "Enum_" + this.GetHashCode().ToString();

            EnumManager.UpdateEnum(EnumName, entries[0], entries);

            var attr = new InputAttribute(pinName);
            attr.Order = 2;
            attr.AutoValidate = true;

            attr.EnumName = EnumName;

            Type pinType = typeof(ISpread<EnumEntry>);
            var pin = FIOFactory.CreateIOContainer(pinType, attr);
            FUseAsID = (ISpread<EnumEntry>)(pin.RawIOObject);
        }

        private void FillEnum(string[] entries)
        {
            EnumManager.UpdateEnum(EnumName, entries[0], entries);
        }

        protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
            var form = new MessageFormular(configSpread[0]);

            FillEnum(form.Fields.ToArray());
        }


        public Message MatchOrInsert(Message message, IEnumerable<string> idFields)
        {
            var compatibleBins = idFields.Intersect(message.Fields);
            bool isCompatible = compatibleBins.Count() == idFields.Distinct().Count();

            var matched = (from keep in FKeep
                           where isCompatible
                           from fieldName in compatibleBins
                           where keep[fieldName] == message[fieldName]// slicewise check of Bins' equality
                           select keep).ToList();

            if (matched.Count == 0)
            {
                // this fresh one does not match anything. discard rogue message
                return null;
            }
            else
            {
                var found = matched.First(); // found a matching record

                found.InjectWith(message, true); // copy all attributes from message to matching record
                found.TimeStamp = message.TimeStamp; // forceUpdate time

                return found;
            }
        }

        //called when data for any output pin is requested
        public override void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged) return;

            if (FInput.IsAnyInvalid())
            {
                FUnknown.SliceCount = 0;
                FUnknown.Flush();

                FChanged.SliceCount = 0;
                FChanged.SliceCount = FKeep.SliceCount;
            }

            // inject all incoming messages and keep a list of all
            var idFields = from fieldName in FUseAsID
                           select fieldName.Name;

            var changed = (
                    from message in FInput
                    where message != null
                    select MatchOrInsert(message, idFields)
                ).Distinct().ToList();


            SpreadMax = FKeep.SliceCount;
            FChanged.SliceCount = FOutput.SliceCount = SpreadMax;


            for (int i = 0; i < SpreadMax; i++)
            {
                var message = FKeep[i];
                FOutput[i] = message;
                FChanged[i] = changed.Contains(message);

            }

            if (changed.Any())
            {
                FOutput.Flush();
                FChanged.Flush();
            }
        }



    }
}