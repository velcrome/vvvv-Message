using System;
using System.Collections.Generic;
using System.IO;
using VVVV.Nodes;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Serializing;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using System.Linq;
using System.ComponentModel.Composition;

namespace VVVV.Packs.Messaging.Nodes.Serializing
{
    #region PluginInfo
    [PluginInfo(Name = "AsOscBundle", Category = "Message", Help = "Outputs OSC Bundle Streams", Tags = "Raw", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageAsOscBundleNode : IPluginEvaluate
    {
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true)]
        protected IDiffSpread<bool> FExtendedMode;
        
        [Output("OSC", AutoFlush = false)]
        protected ISpread<Stream> FOutput;

        public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            if (!FInput.IsChanged && !FExtendedMode.IsChanged) return;

            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                FOutput[i] = FInput[i].ToOSC(FExtendedMode[0]);
            }
            FOutput.Flush();
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "AsOscMessage", Category = "Message", Help = "Outputs OSC Message Streams", Tags = "Raw", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageAsOscMessageNode : AbstractFormularableNode, IPluginEvaluate
    {
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true)]
        protected IDiffSpread<bool> FExtendedMode;

        [Output("OSC", AutoFlush = false)]
        protected ISpread<Stream> FOutput;

        public ISpread<EnumEntry> FUseAsID = null;
        private string EnumName;

        private MessageFormular Formular;

        [Import()]
        protected IIOFactory FIOFactory;

        // most code copied from abstract TypableMessageKeepNode 
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            CreateEnumPin("Use as ID", new string[] { "Foo" });

            (FWindow as FormularLayoutPanel).Locked = true;
        }

        public void CreateEnumPin(string pinName, IEnumerable<string> entries)
        {
            EnumName = "Enum_" + this.GetHashCode().ToString();

            EnumManager.UpdateEnum(EnumName, entries.First(), entries.ToArray());

            var attr = new InputAttribute(pinName);
            attr.Order = 2;
            attr.AutoValidate = true;

            attr.EnumName = EnumName;

            Type pinType = typeof(ISpread<EnumEntry>);
            var pin = FIOFactory.CreateIOContainer(pinType, attr);
            FUseAsID = (ISpread<EnumEntry>)(pin.RawIOObject);
        }

        private void FillEnum(IEnumerable<string> fields)
        {
            EnumManager.UpdateEnum(EnumName, fields.First(), fields.ToArray());
        }

        protected override void OnConfigChange(IDiffSpread<string> configSpread)
        {
            Formular = new MessageFormular(FConfig[0], MessageFormular.DYNAMIC);
            FillEnum(Formular.FieldNames.ToArray());
        }

        protected override void OnSelectFormular(IDiffSpread<EnumEntry> spread)
        {
            base.OnSelectFormular(spread);

            var window = (FWindow as FormularLayoutPanel);
            var fields = window.Controls.OfType<FieldPanel>();

            foreach (var field in fields) field.Checked = true;
            window.Locked = FFormular[0] != MessageFormular.DYNAMIC;
        }

        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            if (!FInput.IsChanged && !FExtendedMode.IsChanged) return;

            FOutput.SliceCount = SpreadMax;


            var fields = from fieldName in FUseAsID
                         select Formular[fieldName];

            for (int i = 0; i < SpreadMax; i++)
            {
                FOutput[i] = FInput[i].ToOSC(fields, FExtendedMode[0]);
            }
            FOutput.Flush();
        }
    }


    #region PluginInfo
    [PluginInfo(Name = "AsMessage", Category = "Raw", Help = "Converts OSC Bundles into Messages ", Tags = "OSC", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageOscAsMessageNode : IPluginEvaluate
    {
        #pragma warning disable 649, 169
        [Input("OSC")]
        IDiffSpread<Stream> FInput;

        [Input("Additional Topic", DefaultString = "", IsSingle = true)]
        IDiffSpread<string> FTopicAdd;

        [Input("Contract Topic Parts", DefaultValue = 1, IsSingle = true, MinValue = 1)]
        IDiffSpread<int> FContract;

        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true, BinVisibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FExtendedMode;

        [Output("Output", AutoFlush = false)]
        ISpread<Message> FOutput;
        #pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsAnyInvalid())
            {
                SpreadMax = 0;
                if (FOutput.SliceCount != 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                return;
            }
            else SpreadMax = FInput.SliceCount;
            
            if (!FInput.IsChanged && !FTopicAdd.IsChanged && !FContract.IsChanged) return;

            FOutput.SliceCount = 0;

            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = OSCExtensions.FromOSC(FInput[i], FExtendedMode[0], FTopicAdd[0], FContract[0]);
                if (message != null) FOutput.Add(message);
            }
            FOutput.Flush();
        }
    }

}
