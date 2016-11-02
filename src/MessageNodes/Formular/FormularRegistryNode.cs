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
        public ISpread<ISpread<string>> FConfiguration;

        [Input("Inherit All")]
        public IDiffSpread<MessageFormular> FInherits;

        [Input("Update", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        public IDiffSpread<bool> FUpdate;

        [Output("Formular", AutoFlush = false)]
        public ISpread<MessageFormular> FOutput;

        [Output("Error", AutoFlush = false)]
        public ISpread<string> FError;

        [Import()]
        protected ILogger FLogger;

        [Import]
        protected IPluginHost2 PluginHost;

        private bool _firstFrame = true;
        private Exception _lastException = null;
        
        public void Evaluate(int SpreadMax)
        {
            if (FConfiguration.IsAnyInvalid() || FName.IsAnyInvalid()) return;

            var update = (!FUpdate.IsAnyInvalid() && FUpdate[0]) || FInherits.IsChanged || FName.IsChanged;

            if (!_firstFrame && !update)
            {
                if (_lastException != null)
                {
                    FError.FlushItem(_lastException.ToString());
                    throw _lastException;
                }
            }

            if (_lastException != null)
            {
                var tmp = _lastException;
                _lastException = null; // assume innocence
                FError.FlushItem(_lastException.ToString());
                throw tmp;
            }

            FOutput.SliceCount = SpreadMax = FName.SliceCount;

            var id = this.PluginHost.GetNodePath(false);
            var reg = MessageFormularRegistry.Context; 

            for (int i = 0; i < SpreadMax; i++)
            {
                if (string.IsNullOrWhiteSpace(FName[i]))
                {
                    _lastException = new ParseFormularException("A Formular cannot have an empty Name.");
                    return;
                }

                var config = string.Join(", ", FConfiguration[i]).Split(',');

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
                        if (!formular.CanAppend(field))
                        {
                            var duplicate = formular[field.Name];
                            _lastException = new DuplicateFieldException("Cannot add new Field \"" + field.ToString() + "\" to Formular [" + formular.Name + "]. Field is already defined as \"" + duplicate.ToString() + "\".", field, duplicate);
                            return;
                        } else
                        {
                            try
                            {
                                formular.Append(field, true);
                            }
                            catch (DuplicateFieldException e)
                            {
                                _lastException = e;
                                return;
                            }
                        }
                    }
                }

                if (_firstFrame || (!FUpdate.IsAnyInvalid() && FUpdate[0]))
                try
                {
                        var defined = reg.Define(id, formular); // will raise Change events to inform all formularable nodes
                        if (defined)
                            FOutput[i] = formular;
                }
                catch (RegistryException e)
                {
                    _lastException = e;
                    return;
                }
                catch (ArgumentNullException e)
                {
                    _lastException = e;
                    return;
                }
            }

            foreach (var form in reg.GetFormularsFrom(id).ToList())
            {
                if (!FName.Contains(form.Name))
                    reg.Undefine(id, form);
            }

            _firstFrame = false;
            FOutput.Flush();
            EnumManager.UpdateEnum(MessageFormularRegistry.RegistryName, reg.AllFormularNames.First(), reg.AllFormularNames.ToArray());
            
        }

        public void Dispose()
        {
            var reg = MessageFormularRegistry.Context;

            reg.UndefineAll(PluginHost.GetNodePath(false));
            EnumManager.UpdateEnum(MessageFormularRegistry.RegistryName, reg.AllFormularNames.First(), reg.AllFormularNames.ToArray());
        }
    }

}
