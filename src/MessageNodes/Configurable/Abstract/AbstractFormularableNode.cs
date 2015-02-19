using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractFormularableNode : ConfigurableNode, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Config("Autolearn Type", IsSingle = true, DefaultBoolean = true, IsToggle = true)]
        public IDiffSpread<bool> FAutoLearnMode;
        
        [Input("Message Formular", DefaultEnumEntry = "None", EnumName = "VVVV.Packs.Message.Core.Formular", Order = 2)]
        public IDiffSpread<EnumEntry> FType;

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            FType.Changed += HandleTypeChange;
            FAutoLearnMode.Changed += HandleLearnModeChange;

            var reg = MessageFormularRegistry.Instance;
            reg.TypeChanged += ConfigChanged;

            EnumManager.UpdateEnum(reg.RegistryName, reg.Keys.First(), reg.Keys.ToArray());
        }

        protected virtual void HandleLearnModeChange(IDiffSpread<bool> spread)
        {
            SetFormular();
        }

        protected virtual void HandleTypeChange(IDiffSpread<EnumEntry> spread)
        {
            SetFormular();
        }

        protected virtual IList<string> SetFormular() 
        {
            var forms = new List<string>();
            
            if (!FAutoLearnMode[0] || FType.IsAnyInvalid()) return forms;

            FConfig.SliceCount = FType.SliceCount;

            for (int i = 0; i < FType.SliceCount;i++ )
            {
                var form = FType[i].Name;
                if (form != MessageFormular.DYNAMIC) FConfig[i] = MessageFormularRegistry.Instance[form].ToString(true);
                forms.Add(form);
                
            }
            return forms; //returns names of the Formulars.
        }

        protected virtual void ConfigChanged(MessageFormularRegistry sender, MessageFormularChangedEvent e)
        {
            if (FType.IsAnyInvalid()) return;

            var used = false;
            
            foreach (var type in FType) 
                if (type.Name == e.Formular.Name) used = true;

            if (used) SetFormular();
        }

  
    }
}