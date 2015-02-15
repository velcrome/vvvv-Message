using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VVVV.Packs.Messaging
{
    public class MessageKeep : IList<Message>
    {
        protected List<Message> Messages = new List<Message>();
        protected Dictionary<Message, Message> Changes = new Dictionary<Message, Message>();

        public bool IsChanged
        {
            get {
                foreach (var message in this)
                    message.ConfirmChanges(); 
                
                return Changes.Count > 0;   
            }
            protected set {
                if (!value) Changes.Clear();   
            }
        }


        protected void MessageChanged(Message original, Message change)
        {
            if (!Messages.Contains(original))
            {
                original.Changed -= MessageChanged;
                return; // spam
            }

            if (Changes.ContainsKey(original))
                Changes[original] += change;
            else Changes[original] = change;
        }

        public IEnumerable<Message> Sync(out IEnumerable<int> indexes)
        {
            var temp = new List<int>();
            var changes = new List<Message>();

            foreach (var orig in Changes.Keys)
            {
                if (Messages.Contains(orig))
                {
                    temp.Add(Messages.IndexOf(orig));
                    changes.Add(Changes[orig]);
                }
            }

            indexes = temp;
            IsChanged = false;
            return changes;
        }

        public IList<Message> Sync()
        {
            var changes = new List<Message>(Changes.Values);

            IsChanged = false;
            return changes;
        }


        public void Sort()
        {
            Messages.Sort(
                delegate(Message x, Message y)
                {
                    return (x.TimeStamp > y.TimeStamp) ? 1 : 0;
                }
            );
        }

        public void AssignFrom(IEnumerable<Message> input)
        {

            Messages.Clear();
            Messages.AddRange(input.Distinct()); // no need to keep duplicates

            foreach (var message in input)
            {
                message.Changed += MessageChanged;
            }
        }
        
        public void Add(Message message)
        {
            if (Messages.Contains(message)) return; // no duplicates

            Messages.Add(message);
            message.Changed += MessageChanged;

        }

        public void Clear()
        {
            foreach (var message in Messages) message.Changed -= MessageChanged;
            Messages.Clear();

        }

        public bool Contains(Message message)
        {
            return Messages.Contains(message);
        }

        public void CopyTo(Message[] array, int arrayIndex)
        {
            throw new NotImplementedException("MessageKeep.CopyTo");

 //           foreach (var message in array) message.Changed += MessageChanged;
 //           Messages.CopyTo(array, arrayIndex);

        }

        public int Count
        {
            get { return Messages.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Message message)
        {
            message.Changed -= MessageChanged;

            return Messages.Remove(message);
        }

        public void RemoveRange(int index, int count)
        {
            for (int i = index; i < index + count; index++)
            {
                if (Messages.Count < i) Messages[i].Changed -= MessageChanged;

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

        public void Insert(int index, Message message)
        {
            message.Changed += MessageChanged;
            Messages.Insert(index, message);
        }

        public void RemoveAt(int index)
        {
            var message = Messages[index];
            message.Changed -= MessageChanged;

            Messages.RemoveAt(index);
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
                message.Changed -= MessageChanged;

                Messages[index] = value;
                value.Changed += MessageChanged;
            }
        }
    }
}
