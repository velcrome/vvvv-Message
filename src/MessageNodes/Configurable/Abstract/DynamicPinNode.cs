using System;
using System.ComponentModel.Composition;
using VVVV.Packs.Message.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Packs.Message.Nodes
{
    public abstract class DynamicPinNode : TypeableNode, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Input", Order = 0)] protected IDiffSpread<Core.Message> FInput;

        [Input("Key", DefaultString = "Foo", IsSingle = true, Order = 1)]
        public IDiffSpread<string> FKey;

        [Input("Type", EnumName = "TypeIdentityEnum", IsSingle = true, Order = 2)]
        public IDiffSpread<EnumEntry> FAlias;

        [Output("Output", AutoFlush = false)] protected Pin<Core.Message> FOutput;
        public IIOContainer FValue;

        [Import()]
        protected IIOFactory FIOFactory;

        public void override OnImportsSatisfied()
        {
            var types = TypeIdentity.Instance.Types;
            EnumManager.UpdateEnum("TypeIdentityEnum", "string", types);
            
            FAlias.Changed += ConfigPin;
            FKey.Changed   += ConfigPin;

            ConfigPin(null);
        }

        protected abstract IOAttribute DefinePin(string name, Type type, int binSize = -1);

        protected void ConfigPin(IDiffSpread spread)
        {

            Type type = typeof (string);
            if (FAlias != null && FAlias.SliceCount > 0) type = TypeIdentity.Instance.FindType(FAlias[0].Name);

            string name = "Foo";
            if (FKey != null && FKey.SliceCount > 0) name = FKey[0];

            if (FValue!=null) FValue.Dispose();
            
            IOAttribute attr = DefinePin(name, type); // each implementation of DynamicNode must create its own InputAttribute or OutputAttribute (
            Type pinType = typeof(ISpread<>).MakeGenericType((typeof(ISpread<>)).MakeGenericType(type)); // the Pin is always a binsized one
            FValue = FIOFactory.CreateIOContainer(pinType, attr);
        }

        public abstract void Evaluate(int SpreadMax);
    }
}