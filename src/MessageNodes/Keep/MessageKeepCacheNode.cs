#region usings
using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;
using System.Linq;
using VVVV.Packs.Messaging;
#endregion usings

namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "Cache",
        Category = "Message.Keep",
        Version = "Typeable", 
        AutoEvaluate = true,
        Help = "Stores Messages and removes them, if no change was detected for a certain time",
        Author = "velcrome")]
    public class TypableMessageCacheNode : TypableMessageKeepNode
    {
        #region fields & pins

        // overwrite FDefault
        [Input("Default", IsSingle = true, Order = 3, Visibility = PinVisibility.OnlyInspector, AutoValidate = false)]
        private new ISpread<Message> FDefault;
        
        [Input("Retain Time", IsSingle = true, DefaultValue = -1.0, Order = 3)]
        public ISpread<double> FTime;

        [Output("Removed Messages", AutoFlush = false, Order = 2)]
        public ISpread<Message> FRemovedMessages;

        #endregion fields & pins

        //called when data for any output pin is requested
        public override void Evaluate(int SpreadMax)
        {
            if (FReset[0])
            {
                Keep.Clear();
            }

            // inject all incoming messages and keep a list of all
            var idFields = from fieldName in FUseAsID
                         select fieldName.Name;

            var changed = (
                    from message in FInput
                    where message != null
                    select MatchOrInsert(message, idFields)
                ).Distinct().ToList();


            if (FTime[0] > 0) RemoveOld(changed);
            
            SpreadMax = Keep.Count;
            FChanged.SliceCount = FOutput.SliceCount = SpreadMax;
         
  
            for (int i = 0; i < SpreadMax;i++ )
            {
                var message = Keep[i];
                FOutput[i] = message;
                FChanged[i] = changed.Contains(message);

            }

            if (changed.Any())
            {
                FOutput.Flush();
                FChanged.Flush();
            }



        }

        private void RemoveOld(List<Message> changed)
        {
            var validTime = Time.Time.CurrentTime() -
                            new TimeSpan(0, 0, 0, (int)Math.Floor(FTime[0]), (int)Math.Floor((FTime[0] * 1000) % 1000));

            var clear = (from message in Keep
                         where message.TimeStamp < validTime
                         select message).ToArray();

            foreach (var m in clear)
            {
                var index = Keep.IndexOf(m);
                Keep.RemoveAt(index);
                changed.Remove(m);
            }

            if (FRemovedMessages.SliceCount > 0 || clear.Length != 0)
            {
                FRemovedMessages.SliceCount = 0;
                FRemovedMessages.AssignFrom(clear);
                FRemovedMessages.Flush();
            }
            else
            {
                // output still empty, no need to flush
            }
        }

    }


}
