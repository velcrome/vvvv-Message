using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Reactive;
using ReactiveUI;
using VVVV.Packs.Messaging;
using System.Linq;

namespace SharpMessage.WPF
{
    public class MainViewModel : ReactiveObject
    {

        public MainViewModel()
        {
            var m = new Message("Hello World");
            m.Add("test", "eins", "zwei", "drei");

            Message = new MessageViewModel(m);
        }


        private int _Count;
        public int Count
        {
            get { return _Count; }
            set { this.RaiseAndSetIfChanged(ref _Count, value); }
        }

        private MessageViewModel _Message;

        public MessageViewModel Message
        {
            get { return _Message; }
            set { this.RaiseAndSetIfChanged(ref _Message, value); }
        }

    }
}