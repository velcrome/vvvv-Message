
#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using VVVV.Packs.Message.Core.Formular;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using System.Linq;
using VVVV.Packs.Message.Core;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Packs.Message.Nodes
{
    using Message = VVVV.Packs.Message.Core.Message;


    [PluginInfo(Name = "Cache",
        Category = "Message",
        AutoEvaluate = true,
        Help = "Stores Messages",
        Author = "velcrome")]
    public class CacheNode : TypeableNode
    {
        #region fields & pins
        [Input("Input")]
        public ISpread<Message> FInput;

        [Input("Retain Time", IsSingle = true, DefaultValue = 1.0)]
        public ISpread<double> FTime;

        [Input("Reset", IsSingle = true)]
        public ISpread<bool> FReset;

        public ISpread<EnumEntry> FUseAsID = null;
        private string EnumName;

        [Output("Cached Output")]
        public ISpread<Message> FOutput;

        [Import()]
        protected IIOFactory FIOFactory;

        protected List<Message> data = new List<Message>();

        #endregion fields & pins

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            CreateEnumPin("Use as ID", new string[] { "vvvv" });
        }

        public void CreateEnumPin(string pinName, string[] entries)
        {
            EnumName = "Enum_" + this.GetHashCode().ToString();

            EnumManager.UpdateEnum(EnumName, entries[0], entries);

            var attr = new InputAttribute(pinName);
            attr.Order = 3;
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

        //called when data for any output pin is requested
        public override void Evaluate(int SpreadMax)
        {
            
            if (FReset[0])
            {
                data.Clear();
            }


            if (FInput.SliceCount > 0)
            {
                var id = FUseAsID[0].Name;

                for (int i = 0; i < SpreadMax; i++)
                {
                    var matched = (from message in data
                                  where message[id] == FInput[i][id]
                                  select message).ToList();

                    if (matched.Count == 0)
                    {
                        data.Add(FInput[i]);
                    } else {
                        var found = matched.First();
                        var k = found += FInput[i];
                        
                    }
                }

            }

            var validTime = Time.Time.CurrentTime() - new TimeSpan(0, 0, (int)Math.Floor(FTime[0]), (int)Math.Floor((FTime[0]*1000)%1000));

            var clear = from message in data
                        where message.TimeStamp < validTime
                        select message;

            foreach (var id in clear.ToArray())
            {
                data.Remove(id);

            }

            FOutput.SliceCount = 0;
            FOutput.AssignFrom(data);

        }
    }


}
