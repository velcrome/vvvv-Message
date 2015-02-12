using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class TypableMessageKeepNode : AbstractMessageKeepNode
    {
        public ISpread<EnumEntry> FUseAsID = null;
        private string EnumName;

        [Output("Changed Slice", Order = 1)]
        public ISpread<bool> FChanged;

        [Import()]
        protected IIOFactory FIOFactory;

        // clear insight
        protected override void Reset(IDiffSpread<bool> spread)
        {
            base.Reset(spread);
            FChanged.SliceCount = 1;
            FChanged[0] = true;
        }


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
            var compatibleBins = idFields.Intersect(message.Attributes);
            bool isCompatible = compatibleBins.Count() == idFields.Distinct().Count();

            var matched = (from keep in Keep
                           where isCompatible
                                from fieldName in compatibleBins
                           where keep[fieldName] == message[fieldName]// slicewise check of Bins' equality
                           select keep).ToList();

            if (matched.Count == 0)
            {
                Keep.Add(message); // record message
                return message;   
            }
            else
            {
                var found = matched.First(); // found a matching record

                var k = found += message; // copy all attributes from message to matching record
                found.TimeStamp = message.TimeStamp; // update time

                return found; 
            }
        }
        



  
    }
}