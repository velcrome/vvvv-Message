using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Generic.Broadcast
{

    public abstract class BroadCastNode<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        #region fields and pins
        [Import()]
        protected IHDEHost FHDEHost;



        [Import]
        protected ILogger FLogger;

        private static IEnumerable<T> _send = Enumerable.Empty<T>();
        private static IEnumerable<T> _receive = Enumerable.Empty<T>();

        public static IEnumerable<T> Sending { 
            get { return _send; }
            protected set { _send =  value; } 
        }

        public static void Send(IEnumerable<T> items)
        {
            _send = items.Concat(_send);
        }
        public static IEnumerable<T> Receive
        {
            get { return _receive; }
            protected set { _receive = value; }
        }

        protected static double CurrentFrame;

        #endregion fields and pins

        #region evaluation management
        public virtual void OnImportsSatisfied()
        {
            FHDEHost.MainLoop.OnPrepareGraph += EnsureUpdate;
        }

        private void EnsureUpdate(object sender, EventArgs eventArgs)
        {
            var time = FHDEHost.FrameTime;
            if (CurrentFrame != time)
            {
                CurrentFrame = time;
                _receive = _send;
                _send = Enumerable.Empty<T>();
            }
        }

        #endregion

        public abstract void Evaluate(int SpreadMax);
    }
}
