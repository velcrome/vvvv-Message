using System.Collections;
using System.Collections.Generic;

namespace VVVV.Packs.Messaging
{
    public class MessageKeep : List<Message>
    {
        public void AssignFrom(IEnumerable<Message> input)
        {
            this.Clear();
            this.AddRange(input);
        }
    }
}
