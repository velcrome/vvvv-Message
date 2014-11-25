using System;
using System.ComponentModel.Composition;
using VVVV.Packs.Message.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Packs.Message.Core.Formular;


namespace VVVV.Packs.Message.Nodes
{
    public abstract class DynamicPinNode : ConfigurableNode, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Input", Order = 0)] 
        protected IDiffSpread<Core.Message> FInput;

        [Input("Key", DefaultString = "Foo", IsSingle = true, Order = 1)]
        public IDiffSpread<string> FKey;

        [Input("Type", EnumName = "TypeIdentityEnum", IsSingle = true, Order = 2)]
        public IDiffSpread<EnumEntry> FAlias;

        [Output("Output", AutoFlush = false)] protected Pin<Core.Message> FOutput;
        public IIOContainer FValue;

        [Import()]
        protected IIOFactory FIOFactory;

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            var types = TypeIdentity.Instance.Types;
            EnumManager.UpdateEnum("TypeIdentityEnum", "string", types);
            
            FAlias.Changed += ConfigPin;
            FKey.Changed   += ConfigPin;


        }

        protected abstract IOAttribute DefinePin(string name, Type type, int binSize = -1);

        protected void ConfigPin(IDiffSpread spread)
        {

            string typeAlias = "invalid";
            if (FAlias != null && FAlias.SliceCount > 0) typeAlias = FAlias[0].Name;

            string name = "";
            if (FKey != null && FKey.SliceCount > 0) name = FKey[0];
            
            var newConfig = typeAlias + " " + name;
            
            if (newConfig != FConfig[0] && typeAlias != "invalid") FConfig[0] = newConfig; // first frame or user mistake will not reconfigure
        }

       protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
            var formular = new MessageFormular(configSpread[0] ?? "");

            if (FValue != null) FValue.Dispose();

            foreach (var name in formular.Fields)
            {
                Type type = formular.GetType(name);
            
                IOAttribute attr = DefinePin(name, type); // each implementation of DynamicNode must create its own InputAttribute or OutputAttribute (
                Type pinType = typeof(ISpread<>).MakeGenericType((typeof(ISpread<>)).MakeGenericType(type)); // the Pin is always a binsized one
                FValue = FIOFactory.CreateIOContainer(pinType, attr);

                break; // handle only first one for now.
            }
           
           

 
        }

   
    }
}