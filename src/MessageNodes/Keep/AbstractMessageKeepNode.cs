using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Packs.Messaging.Core;
using VVVV.Packs.Messaging.Core.Formular;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractMessageKeepNode : TypeableNode
    {
        [Input("Input", Order = 0)]
        public ISpread<Message> FInput;

        public ISpread<EnumEntry> FUseAsID = null;
        private string EnumName;

        [Input("Reset", IsSingle = true, Order = int.MaxValue-1)]
        public ISpread<bool> FReset;

        [Input("Replace Dump", Order = int.MaxValue)]
        public ISpread<List<Message>> FReplaceData;

        [Output("Output", Order = 0)]
        public ISpread<Message> FOutput;

        [Output("Changed Slice", Order = 1)]
        public ISpread<bool> FChanged;

        [Output("Dump", Order = 2)]
        public ISpread<List<Message>> FDump;

        [Import()]
        protected IIOFactory FIOFactory;

        // clear insight
        public readonly List<Message> MessageKeep = new List<Message>();

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


        public Message MatchOrInsert(Message message, string fieldName)
        {
            var matched = (from keep in MessageKeep
                           where keep.Attributes.Contains(fieldName)
                           // incompatible if old saved message has no field with id
                           where keep[fieldName] == message[fieldName]
                           // slicewise check of Bins' equality
                           select keep).ToList();

            if (matched.Count == 0)
            {
                MessageKeep.Add(message); // record message
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
        

        protected virtual void SortKeep()
        {
            MessageKeep.Sort(delegate(Message x, Message y)
            {
                return (x.TimeStamp > y.TimeStamp) ? 1 : 0;
            });

            return;
        }

  
    }
}