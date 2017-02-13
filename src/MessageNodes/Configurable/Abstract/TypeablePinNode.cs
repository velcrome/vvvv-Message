using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.DX11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils;


namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class TypeablePinNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        public const string TypeIdentityEnum = "TypeIdentityEnum";

        [Config("Configuration", DefaultString = "string Foo", Visibility = PinVisibility.True)]
        public IDiffSpread<string> FConfig;

        [Input("Input", Order = 0)] 
        protected IDiffSpread<Message> FInput;

        [Input("Type", EnumName = TypeIdentityEnum, IsSingle = true, Order = 1)]
        public IDiffSpread<EnumEntry> FAlias;
        
        [Input("Key", DefaultString = "Foo", Order = 2)]
        public IDiffSpread<string> FKey;

        [Output("Output", AutoFlush = false)] 
        protected Pin<Message> FOutput;

        protected IIOContainer FValue;

        protected Type TargetDynamicType;

        [Import()]
        protected IIOFactory FIOFactory;

        [Import()]
        protected ILogger FLogger;

        [Import()]
        protected IPluginHost FHost;

        public void OnImportsSatisfied()
        {
            FConfig.Changed += OnConfigChange;

            var types = TypeIdentity.Instance.Aliases;
            EnumManager.UpdateEnum(TypeIdentityEnum, "string", types);
            
            FAlias.Changed += ConfigPin;
        }

        protected abstract IOAttribute DefinePin(FormularFieldDescriptor field);

        protected void ConfigPin(IDiffSpread spread)
        {
            string typeAlias = "string";
            if (!FAlias.IsAnyInvalid()) typeAlias = FAlias[0].Name;

            var newConfig = typeAlias + " Value";
            if (newConfig != FConfig[0]) FConfig[0] = newConfig; // first frame or user mistake will not reconfigure
        }

       protected virtual void OnConfigChange(IDiffSpread<string> configSpread)
        {
            var formular = new MessageFormular(MessageFormular.DYNAMIC, configSpread[0] ?? "string Value");
            if (formular.FieldNames.Count() < 1) return;

            if (FValue != null)
            {
                FValue.Dispose();
            }

            var name = formular.FieldNames.First();
            TargetDynamicType = formular[name].Type;
            
            IOAttribute attr = DefinePin(formular[name]); // each implementation of DynamicNode must create its own InputAttribute or OutputAttribute 
            Type pinType = typeof(ISpread<>).MakeGenericType((typeof(ISpread<>)).MakeGenericType(TargetDynamicType)); // the Pin is always a binsized one
  
           FValue = FIOFactory.CreateIOContainer(pinType, attr);
        }

        public abstract void Evaluate(int SpreadMax);

        #region dx11 ResourceDataRetriever
        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;

        protected void InitDX11Graph()
        {
            if (this.RenderRequest != null) { RenderRequest((IDX11ResourceDataRetriever)this, FHost); }
        }

        #endregion dx11 ResourceDataRetriever

        #region dx11 ResourceHost
        public void Update(DX11RenderContext context)
        { }

        public void Destroy(DX11RenderContext context, bool force)
        {
                var pin = FValue.ToISpread() as ISpread<ISpread<DX11Resource<IDX11Resource>>>;
                if (pin == null) return;

                for (int i = 0; i < pin.SliceCount; i++)
                    for (int j = 0; j < pin[i].SliceCount; j++)
                        pin[i][j].Dispose(context);
        }
        #endregion dx11 ResourceHost
    }
}