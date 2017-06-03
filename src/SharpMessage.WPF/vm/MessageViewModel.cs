using System;
using System.Reactive;
using ReactiveUI;
using VVVV.Packs.Messaging;

namespace SharpMessage.WPF
{
    public class MessageViewModel : ReactiveValidatedObject
    {
        private Message Original;

        public MessageViewModel(Message message = null)
        {
            this.Original = message;
        }

        private string _Topic;
        [ValidatesViaMethod(AllowBlanks = false, AllowNull = false, Name = "IsTopicValid", ErrorMessage = "Please enter a valid topic")]
        public string Topic
        {
            get { return Original.Topic; }
            set { this.RaiseAndSetIfChanged(ref _Topic, value); }
        }

        public bool IsTopicValid(string topic)
        {
            return topic == "foo";
        }

        private bool _IsChanged;
        public bool IsChanged
        {
            get { return _IsChanged; }
            set { this.RaiseAndSetIfChanged(ref _IsChanged, value); }
        }
    }
}