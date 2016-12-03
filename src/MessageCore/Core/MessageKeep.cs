using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VVVV.Packs.Messaging
{
    /// <summary>
    /// An archive for Messages. Archives will have knowledge of changes in any Message they keep.
    /// </summary>
    public class MessageKeep : IReadOnlyList<Message>    {
        #region fields
        protected List<Message> Messages = new List<Message>();
        protected Dictionary<Message, Message> Changes = new Dictionary<Message, Message>();

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

        /// <summary>
        /// Detects if any changes happened to Messages in the Keep since the last Sync.
        /// </summary>
        public bool IsChanged
        {
            get {
                return Changes.Count > 0 || Messages.Any(m => m.IsChanged);
            }
            protected set {
                if (value == false)
                {
                    Changes.Clear();
                }
            }
        }

        public bool IsSweeping
        {
            get
            {
                foreach (var m in Messages)
                {
                    if (m.Fields.Any(field => m[field].IsSweeping())) return true;
                }
                return false;
            }
        }
        #endregion fields

        #region ctor
        public MessageKeep() {}

        public MessageKeep(bool quickMode) : base()
        {
            QuickMode = quickMode;
        }
        #endregion ctor

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
                Changes[original].InjectWith(change, false); // always update
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

            // this will force the message to publish change events to all other  
            // interested parties, including this keep
            foreach (var message in Messages) message.Commit(this); 

            foreach (var orig in Changes.Keys)
            {
                if (Messages.Contains(orig))
                {
                    temp.Add(Messages.IndexOf(orig));
                    if (!QuickMode) changes.Add(Changes[orig]);
                } else
                {
                    // should never happen!
                }
            }

            indexes = temp;
            IsChanged = false;
            return changes;
        }

        public IEnumerable<Message> Sync()
        {
            List<Message> changes = null;

            // this will force the message to publish change events to all other  
            // interested parties, including this keep
            foreach (var message in Messages) message.Commit(this);

            if (!QuickMode)
                changes = new List<Message>(Changes.Values);

            IsChanged = false;
            return changes;
        }
        #endregion Syncing

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
            if (Messages.Any(m => m == message)) return; // no direct duplicates

            Messages.Add(message);
            if (QuickMode)
            {
                Changes.Add(message, null);
                message.Changed += MessageChanged;
            }
            else
            {
                Changes.Add(message, message.Clone() as Message);  // to clone or not to clone...
                message.ChangedWithDetails += MessageChangedWithDetails;
            }

        }

        public void Clear()
        {
            foreach (var message in Messages.ToArray())
            {
                Remove(message);
            }
            Changes.Clear();
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

            if (message.HasRecentCommit())
            {
                foreach (var bin in message.Data.Values)
                    if (bin.IsSweeping(this)) bin.Sweep();
            }

            Changes.Remove(message);
            return Messages.Remove(message);
        }

        public void RemoveRange(int index, int count)
        {
            for (int i = index; i < index + count; i++)
            {
                if (Messages.Count < i)
                {
                    Remove(Messages[i]);
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
                if (Count == 0) return null;
                else return Messages[index];
            }
            set
            {
                Message message;
                if (Messages.ElementAtOrDefault(index) != null)
                {
                    message = Messages[index];
                    message.ChangedWithDetails -= MessageChangedWithDetails;
                    message.Changed -= MessageChanged;
                }

                Messages[index] = value;
                if (QuickMode)
                    value.Changed += MessageChanged;
                    else value.ChangedWithDetails += MessageChangedWithDetails;
            }
        }

        #endregion List


    }
}
