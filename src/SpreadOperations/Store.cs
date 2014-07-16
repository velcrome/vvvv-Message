using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.Pack.Message;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Pack.Game.Nodes
{


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

        [Input("Add Element")]
        Pin<T> FAdd;


        [Output("Output", AutoFlush = false)]
        private Pin<T> FOutput;


        protected List<T> FElements = new List<T>();

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
            if (FClear[0]) FElements.Clear();

            if (FDeleteNow[0])
            {
                var del = FDeleteIndex.ToList();

                int size = FElements.Count();

                for (int i = 0; i < del.Count; i++)
                {
                    del[i] = VMath.Zmod(del[i], size);
                }
                del.Sort();

                for (int i = 0; i < del.Count; i++)
                {
                    if (FElements.Count > i)
                        FElements.RemoveAt(del[i] - i);
                }
            }


            if (!FAdd.IsAnyInvalid())
            {
                FElements.AddRange(FAdd);
                Sort();
            }

            FOutput.AssignFrom(FElements);
            FOutput.Flush();

        }

        protected abstract void Sort();
    }
}
