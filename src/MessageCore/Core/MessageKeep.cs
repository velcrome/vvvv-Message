using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VVVV.Packs.Messaging
{
    public class MessageKeep : IReadOnlyList<Message>
    {
        protected List<Message> Messages = new List<Message>();
        protected Dictionary<Message, Message> Changes = new Dictionary<Message, Message>();

        #region Fields
        private bool _quickMode = true;
        public bool QuickMode
        {
            get
            {
                return _quickMode;
            }
            set
            {
                if (value == _quickMode) return;
                _quickMode = value;

                foreach (var message in Messages)
                {
                    Changes.Clear();

                    if (_quickMode)
                    {
                        message.ChangedWithDetails -= MessageChangedWithDetails;
                        message.Changed += MessageChanged;
                    }
                    else
                    {
                        message.Changed -= MessageChanged;
                        message.ChangedWithDetails += MessageChangedWithDetails;
                    }
                }
            }
        }


        public bool IsChanged
        {
            get {
                return Changes.Count > 0 || Messages.Any(m => m.IsChanged);
            }
            protected set {
                if (value == false)
                {
                    //foreach (var message in Messages) message.IsChanged = false;
                    Changes.Clear();
                }
            }
        }
    #endregion Fields

        public MessageKeep()
        {
        }

        public MessageKeep(bool quickMode)
        {
            QuickMode = quickMode;
        }


        #region Event Handlers 
        
        // come with selective Quickmode subscription to individual Messages
        protected void MessageChangedWithDetails(Message original, Message change)
        {
            if (!Messages.Contains(original))
            {
                original.ChangedWithDetails -= MessageChangedWithDetails;
                return; // spam
            }

            if (Changes.ContainsKey(original) && Changes[original] != null)
                Changes[original].InjectWith(change, true, false);
            else
            {
                change.Topic = original.Topic;
                Changes[original] = change;
            }
        }

        protected void MessageChanged(Message original)
        {
            if (!Messages.Contains(original))
            {
                original.Changed -= MessageChanged;
                return; // spam
            }

            if (!Changes.ContainsKey(original))
            {
                Changes[original] = null;
            }
        }
        #endregion Event Handlers

        #region Syncing
        public IEnumerable<Message> Sync(out IEnumerable<int> indexes)
        {
            var temp = new List<int>();
            var changes = new List<Message>();

            foreach (var message in Messages) message.Sync(); 

            foreach (var orig in Changes.Keys)
            {
                if (Messages.Contains(orig))
                {
                    temp.Add(Messages.IndexOf(orig));
                    if (!QuickMode) changes.Add(Changes[orig]);
//                        else changes.Add(orig);#
                    Changes[orig].Data.Clear();
                }
            }

            indexes = temp;
            IsChanged = false;
            return changes;
        }

        public IEnumerable<Message> Sync()
        {
            List<Message> changes = null;

            foreach (var message in Messages) message.Sync(); // will inform all subscribers of the message about changes.

            if (!QuickMode)
                changes = new List<Message>(Changes.Values);

            IsChanged = false;
            return changes;
        }
        #endregion Syncing

        public void Sort()
        {
            Messages.Sort(
                delegate(Message x, Message y)
                {
                    return (x.TimeStamp.UniversalTime > y.TimeStamp.UniversalTime) ? 1 : 0;
                }
            );
        }

        public void AssignFrom(IEnumerable<Message> input)
        {

            Clear();
            foreach (var message in input)
            {
                Add(message);
            }
        }

        #region IList implementation

        
        public void Add(Message message)
        {
            if (Messages.Contains(message)) return; // no duplicates

            Messages.Add(message);
            if (QuickMode)
            {
                Changes.Add(message, null);
                message.Changed += MessageChanged;
            }
            else
            {
//                Changes.Add(message, message);
                Changes.Add(message, message.Clone() as Message);  // to clone or not to clone...
                message.ChangedWithDetails += MessageChangedWithDetails;
            }

        }

        public void Clear()
        {
            foreach (var message in Messages)
            {
                message.ChangedWithDetails -= MessageChangedWithDetails;
                message.Changed -= MessageChanged;
            }
            Changes.Clear();
            Messages.Clear();

        }

        public bool Contains(Message message)
        {
            return Messages.Contains(message);
        }

        public int Count
        {
            get { return Messages.Count; }
        }

        public bool Remove(Message message)
        {
            message.ChangedWithDetails -= MessageChangedWithDetails;
            message.Changed -= MessageChanged;

            Changes.Remove(message);
            return Messages.Remove(message);
        }

        public void RemoveRange(int index, int count)
        {
            for (int i = index; i < index + count; i++)
            {
                if (Messages.Count < i)
                {
                    Messages[i].Changed -= MessageChanged;
                    Messages[i].ChangedWithDetails -= MessageChangedWithDetails;
                    Changes.Remove(Messages[i]);
                }

            }            
            Messages.RemoveRange(index, count);

        }


        public IEnumerator<Message> GetEnumerator()
        {
            return Messages.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Messages.GetEnumerator();
        }

        public int IndexOf(Message message)
        {
            return Messages.IndexOf(message);
        }
        public Message this[int index]
        {
            get
            {
                return Messages[index];
            }
            set
            {
                var message = Messages[index];
                message.ChangedWithDetails -= MessageChangedWithDetails;
                message.Changed -= MessageChanged;

                Messages[index] = value;
                if (QuickMode)
                    message.Changed += MessageChanged;
                    else value.ChangedWithDetails += MessageChangedWithDetails;
            }
        }

        #endregion List
    }
}
