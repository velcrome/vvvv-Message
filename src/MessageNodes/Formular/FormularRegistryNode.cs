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
        [Input("Formular Name", DefaultString = "Event")]
        public ISpread<string> FName;

        [Input("Configuration", DefaultString = "string Foo", BinSize=1)]
        public ISpread<ISpread<string>> FConfig;

        [Input("Inherits")]
        public ISpread<MessageFormular> FInherits;

        [Input("Update", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        public IDiffSpread<bool> FUpdate;

        //[Input("Clear All", IsSingle = true, IsBang = true, DefaultBoolean = false, Visibility = PinVisibility.OnlyInspector)]
        //public IDiffSpread<bool> FClear;

        [Output("Formular")]
        public ISpread<MessageFormular> FOutput;

        [Import()]
        protected ILogger FLogger;

        [Import]
        protected IPluginHost2 PluginHost;

        private bool _firstFrame = true;
        private bool _success = true;
        
        public void Evaluate(int SpreadMax)
        {
            if (FUpdate.IsAnyInvalid() || FConfig.IsAnyInvalid()) return;
            if (!FUpdate[0] && !_firstFrame)
            {
                if (!_success) throw new Exception("Somthing wrong with this Formular!");
                return;
            }

            _success = true; // assume innocence

            FOutput.SliceCount = SpreadMax = FName.SliceCount;

            var id = this.PluginHost.GetNodePath(false);
            var reg = MessageFormularRegistry.Instance; 

            for (int i = 0; i < SpreadMax; i++)
            {
                var config = string.Join(", ", FConfig[i]).Split(',');

                var fields = new List<FormularFieldDescriptor>();
                foreach (var def in config)
                {
                    try
                    {
                        var field = new FormularFieldDescriptor(def.Trim(), true);
                        
                        if (fields.Any(f => f.Name == field.Name))
                        {
                            FLogger.Log(LogType.Error, "Cannot add \"" + def + "\" in Formular for [" + FName[i] + "]. Field with the same name already defined.");
                            _success = false;
                        }
                        else fields.Add(field);
                    }
                    catch (Exception e)
                    {
                        FLogger.Log(LogType.Error, "Cannot parse \"" + def + "\" in Formular for [" + FName[i]+"]. "+ e.Message);
                        _success = false;
                    }
                }


                var formular = new MessageFormular(fields, FName[i]);

                if (!FInherits.IsAnyInvalid())
                {
                    var allFields = from form in FInherits
                                    from field in form.FieldDescriptors
                                    select field;

                    foreach (var field in allFields)
                    {
                        try
                        {
                            formular.Append(field, false);
                        }
                        catch (Exception e)
                        {
                            FLogger.Log(LogType.Error, e.Message);
                            _success = false;
                        }
                    }
                }

                try
                {
                    var defined = reg.Define(id, formular, _firstFrame);
                    if (defined)
                    {
                        FOutput[i] = formular;
                    }
                    else _success = false;
                }
                catch (Exception e)
                {
                    FLogger.Log(LogType.Error, e.Message);
                    _success = false;
                } 
            }

            foreach (var form in reg.FromId(id))
            {
                if (!FName.Contains(form.Name))
                    reg.Undefine(id, form);
            }
            EnumManager.UpdateEnum(MessageFormularRegistry.RegistryName, reg.Names.First(), reg.Names);
            _firstFrame = false;
            
        }

        public void Dispose()
        {
            var reg = MessageFormularRegistry.Instance;

            reg.UndefineAll(PluginHost.GetNodePath(false));
            EnumManager.UpdateEnum(MessageFormularRegistry.RegistryName, reg.Names.First(), reg.Names);
        }
    }

}
