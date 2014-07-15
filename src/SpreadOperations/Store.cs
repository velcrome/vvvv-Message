using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.Pack.Message;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Pack.Game.Nodes
{
    [PluginInfo(
        Name = "Store",
        Category = "Game",
        Help = "Stores Agents, is the root of all Behavior Trees",
        AutoEvaluate = true,
        Tags = "velcrome")]
    public class ElementStoreNode : StoreNode<string>
    {


    }

    public abstract class StoreNode<T> :  IPluginEvaluate, IPartImportsSatisfiedNotification
    {
    #region pins and fields
#pragma warning disable 649, 169


        [Input("Delete Slice")]
        ISpread<int> FDeleteIndex;

        [Input("Delete", IsSingle = true, IsBang = true)]
        ISpread<bool> FDeleteNow;

        [Input("Clear", IsSingle = true, IsBang = true)]
        ISpread<bool> FClear;

        [Input("Add Agent")]
        Pin<T> FAdd;


        [Output("Output", AutoFlush = false)]
        private Pin<T> FOutput;


        private List<T> FAgents = new List<T>();

        [Import()]
        protected ILogger FLogger;

#pragma warning restore
        #endregion

        #region evaluation management
        public void OnImportsSatisfied()
        {


        }

        private void connect(object sender, PinConnectionEventArgs args)
        {

        }
        #endregion

        public void Evaluate(int SpreadMax)
        {
            if (FClear[0]) FAgents.Clear();

            if (FDeleteNow[0])
            {
                var del = FDeleteIndex.ToList();

                int size = FAgents.Count();

                for (int i = 0; i < del.Count; i++)
                {
                    del[i] = VMath.Zmod(del[i], size);
                }
                del.Sort();

                for (int i = 0; i < del.Count; i++)
                {
                    if (FAgents.Count > i)
                        FAgents.RemoveAt(del[i] - i);
                }
            }


            if (!FAdd.IsAnyInvalid())
            {
                FAgents.AddRange(FAdd);
                FAgents.Sort();
            }

            FOutput.AssignFrom(FAgents);
            FOutput.Flush();

        }

    }
}
