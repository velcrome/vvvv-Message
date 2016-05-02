using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "Formular", AutoEvaluate = true, Category = "Message", Help = "Define a high level Template for Messages", Tags = "Dynamic, Bin, velcrome")]
    #endregion PluginInfo
    public class FormularRegistryNode : IPluginEvaluate, IDisposable
    {
        [Input("Formular Name", DefaultString = MessageFormularRegistry.DefaultFormularName)]
        public IDiffSpread<string> FName;

        [Input("Configuration", DefaultString = MessageFormularRegistry.DefaultField, BinSize=-1)]
        public ISpread<ISpread<string>> FConfig;

        [Input("Inherit All")]
        public IDiffSpread<MessageFormular> FInherits;

        [Input("Update", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        public IDiffSpread<bool> FUpdate;

        [Output("Formular", AutoFlush = false)]
        public ISpread<MessageFormular> FOutput;

        [Import()]
        protected ILogger FLogger;

        [Import]
        protected IPluginHost2 PluginHost;

        private bool _firstFrame = true;
        private Exception _lastException = null;
        
        public void Evaluate(int SpreadMax)
        {
            if (FUpdate.IsAnyInvalid() || FConfig.IsAnyInvalid() || FName.IsAnyInvalid()) return;

            var update = FUpdate[0] || FInherits.IsChanged || FName.IsChanged;

            if (!_firstFrame && !update)
            {
                if (_lastException != null) throw _lastException;
                return;
            }
            _firstFrame = false;


            _lastException = null; // assume innocence

            FOutput.SliceCount = SpreadMax = FName.SliceCount;

            var id = this.PluginHost.GetNodePath(false);
            var reg = MessageFormularRegistry.Instance; 

            for (int i = 0; i < SpreadMax; i++)
            {
                if (string.IsNullOrWhiteSpace(FName[i]))
                {
                    _lastException = new FormatException("A Formular cannot have an empty Name.");
                    return;
                }

                var config = string.Join(", ", FConfig[i]).Split(',');

                var fields = new List<FormularFieldDescriptor>();
                foreach (var def in config)
                {
                    FormularFieldDescriptor field;
                    try
                    {
                        field = new FormularFieldDescriptor(def.Trim(), true);
                    }
                    catch (ParseFormularException e)
                    {
                        _lastException = e;
                        return;
                    }

                    var match = (
                                    from f in fields
                                    where f.Name == field.Name
                                    select f
                                ).FirstOrDefault();
                        
                    if (match != null)
                    {
                        _lastException = new DuplicateFieldException("Cannot add \"" + def + "\" in Formular for [" + FName[i] + "]. Field with the same name already defined.", match, field);
                        return;
                    }
                    else fields.Add(field);
                }


                var formular = new MessageFormular(FName[i], fields);

                if (!FInherits.IsAnyInvalid())
                {
                    var allFields = (
                                        from form in FInherits
                                        from field in form.FieldDescriptors
                                        select field
                                     ).Distinct();

                    foreach (var field in allFields)
                    {
                        try
                        {
                            formular.Append(field, false);
                        }
                        catch (DuplicateFieldException e)
                        {
                            _lastException = e;
                            return;
                        }
                    }
                }

                try
                {
                    var defined = reg.Define(id, formular, _firstFrame);
                    if (defined)
                        FOutput[i] = formular;
                }
                catch (RegistryException e)
                {
                    _lastException = e;
                    return;
                } 
            }

            foreach (var form in reg.FromId(id))
            {
                if (!FName.Contains(form.Name))
                    reg.Undefine(id, form);
            }

            FOutput.Flush();

            EnumManager.UpdateEnum(MessageFormularRegistry.RegistryName, reg.Names.First(), reg.Names);
            
        }

        public void Dispose()
        {
            var reg = MessageFormularRegistry.Instance;

            reg.UndefineAll(PluginHost.GetNodePath(false));
            EnumManager.UpdateEnum(MessageFormularRegistry.RegistryName, reg.Names.First(), reg.Names);
        }
    }

}
