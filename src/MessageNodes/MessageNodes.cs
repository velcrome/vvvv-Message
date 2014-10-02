#region usings
using System.Globalization;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using System.Text;
using System.Linq;
using VVVV.Nodes.Generic;
using VVVV.Packs.Message;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;
using VVVV.Utils.Streams;


#endregion usings

namespace VVVV.Pack.Message.Nodes
{
    using Message = VVVV.Packs.Message.Core.Message;

    #region PluginInfo
    [PluginInfo(Name = "Info", Category = "Message", Help = "Help to Debug Messages", Tags = "Dynamic, TTY, velcrome")]
    #endregion PluginInfo
    public class MessageInfoNode : IPluginEvaluate
    {
        #pragma warning disable 649, 169

        [Input("Input")] private IDiffSpread<Message> FInput;

        [Input("Print Message", IsBang = true)] private IDiffSpread<bool> FPrint;

        [Output("Address", AutoFlush = false)] private ISpread<string> FAddress;

        [Output("Timestamp", AutoFlush = false)] private ISpread<string> FTimeStamp;

        [Output("Output", AutoFlush = false)] private ISpread<string> FOutput;

        [Output("Configuration", AutoFlush = false)] private ISpread<string> FConfigOut;

        [Import()] protected ILogger FLogger;

        #pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FInput.SliceCount <= 0 || FInput[0] == null) SpreadMax = 0;
            else SpreadMax = FInput.SliceCount;

            if (!FInput.IsChanged) return;

            FOutput.SliceCount = SpreadMax;
            FTimeStamp.SliceCount = SpreadMax;
            FAddress.SliceCount = SpreadMax;
            FConfigOut.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                Message m = FInput[i];
                FOutput[i] = m.ToString();
                FAddress[i] = m.Address;
                FTimeStamp[i] = m.TimeStamp.UniversalTime.ToString(CultureInfo.InvariantCulture);
                FConfigOut[i] = FInput[i].GetConfig();

                if (FPrint[i])
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("== Message " + i + " ==");
                    sb.AppendLine();

                    sb.AppendLine(FInput[i].ToString());
                    sb.Append("====\n");
                    FLogger.Log(LogType.Debug, sb.ToString());
                }
            }
            FAddress.Flush();
            FTimeStamp.Flush();
            FOutput.Flush();
            FConfigOut.Flush();
        }
    }


    #region PluginInfo
    [PluginInfo(Name = "Sift", Category = "Message", Help = "Filter Messages", Tags = "Dynamic, velcrome")]
    #endregion PluginInfo
    public class MessageSiftNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")] private IDiffSpread<Message> FInput;

        [Input("Filter", DefaultString = "*")] private IDiffSpread<string> FFilter;

        [Output("Output", AutoFlush = false)] private ISpread<Message> FOutput;

        [Output("NotFound", AutoFlush = false)] private ISpread<Message> FNotFound;

        [Import()] protected ILogger FLogger;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged) return;

            SpreadMax = FInput.SliceCount;

            FOutput.SliceCount = 0;
            FNotFound.SliceCount = 0;
            bool[] found = new bool[SpreadMax];
            for (int i = 0; i < SpreadMax; i++) found[i] = false;

            for (int i = 0; i < FFilter.SliceCount; i++)
            {
                string[] filter = FFilter[i].Split('.');

                for (int j = 0; j < SpreadMax; j++)
                {
                    if (!found[j]) found[j] = FInput[j].AddressMatches(FFilter[i]);
                }
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                if (found[i]) FOutput.Add(FInput[i]);
                else FNotFound.Add(FInput[i]);
            }
            FOutput.Flush();
            FNotFound.Flush();
        }
    }



    #region PluginInfo
    [PluginInfo(Name = "FrameDelay", Category = "Message", Help = "Allows Feedback Loops for Messages",
        Tags = "velcrome", AutoEvaluate = true)]
    #endregion PluginInfo
    public class MessageFrameDelayNode : IPluginEvaluate
    {
        #pragma warning disable 649, 169
        [Input("Input")] private IDiffSpread<Message> FInput;

        [Input("Default")] private ISpread<Message> FDefault;

        [Input("Initialize", IsSingle = true, IsBang = true)] private IDiffSpread<bool> FInit;

        [Output("Message", AutoFlush = false, AllowFeedback = true)] private ISpread<Message> FOutput;

        private List<Message> lastFrame;

        [Import()] protected ILogger FLogger;
#pragma warning restore

        public MessageFrameDelayNode()
        {
            lastFrame = new List<Message>();
        }


        public void Evaluate(int SpreadMax)
        {
            if (FInit[0])
            {
                lastFrame.Clear();
                lastFrame.AddRange(FDefault);
            }
            else
            {
                lastFrame.Clear();

                if (FInput.SliceCount > 0 && FInput[0] != null) lastFrame.AddRange(FInput);
            }

            FOutput.SliceCount = lastFrame.Count;

            FOutput.AssignFrom(lastFrame);
            FOutput.Flush();
        }

        [PluginInfo(Name = "Search", Category = "Message", Help = "Allows LINQ queries for Messages", Tags = "velcrome")
        ]
        public class MessageSearchNode : IPluginEvaluate
        {
#pragma warning disable 649, 169
            [Input("Input")] private IDiffSpread<Message> FInput;

            [Input("Where", DefaultString = "Foo = \"bar\"")] private IDiffSpread<string> FWhere;

            [Input("SendQuery", IsSingle = true, IsBang = true)] private IDiffSpread<bool> FSendQuery;

            [Output("Message", AutoFlush = false)] private ISpread<ISpread<Message>> FOutput;

            [Output("Former Slice", AutoFlush = false)] private ISpread<ISpread<int>> FFormerSlice;

            [Output("Query", AutoFlush = false)] private ISpread<string> FQuery;

            [Import()] protected ILogger FLogger;
#pragma warning restore

            public void Evaluate(int SpreadMax)
            {
                if (!FInput.IsChanged && !FSendQuery.IsChanged && !FQuery.IsChanged && !FSendQuery[0]) return;


                if (FSendQuery[0])
                {
                    SpreadMax = FWhere.SliceCount;
                    FQuery.SliceCount = SpreadMax;
                    FOutput.SliceCount = SpreadMax;
                    FFormerSlice.SliceCount = SpreadMax;


                    for (int i = 0; i < SpreadMax; i++)
                    {

                        var query = FWhere[i].Split('=');

                        string attrib = query[0].Trim();
                        string val = "";

                        for (int j = 1; j < query.Count(); j++) val += query[j];
                        val = val.Trim(' ').Trim('\"');

                        FQuery[i] = "FROM Message IN Input WHERE " + attrib + " = \"" + val + "\" SELECT Message";

                        var result =
                            from m in FInput
                            where m != null
                            where m[attrib].Count > 0 && m[attrib][0].Equals(val)
                            select m;

                        FOutput[i].AssignFrom(result);

                        FFormerSlice[i].SliceCount = 0;

                        foreach (var m in result)
                        {
                            for (int j = 0; j < FInput.SliceCount; j++)
                            {

                                if (m.Equals(FInput[j]))
                                {
                                    FFormerSlice[i].Add(j);
                                    break;
                                }
                            }
                        }
                    }

                    if (SpreadMax == 1 && ((FOutput[0].SliceCount == 0) || (FOutput[0][0] == null)))
                    {
                        FOutput.SliceCount = 0;
                    }

                    FFormerSlice.Flush();
                    FQuery.Flush();
                    FOutput.Flush();
                }
            }
        }

/*        this one is broken. use zip instead.
 
        [PluginInfo(Name = "Cons", Category = "Message", Help = "Concatenates all Messages", Tags = "velcrome")]
        public class MessageConsNode : Cons<Message>
        {}
*/

/*        this one is lame. use store instead
        [PluginInfo(Name = "Buffer", Category = "Message", Help = "Buffers all Messages", Tags = "velcrome")]
        public class MessageBufferNode : BufferNode<Message>
        {
        }
*/

/*        no real use case for this one
        [PluginInfo(Name = "RingBuffer", Category = "Message", Help = "Ringbuffers all Messages", Tags = "velcrome")]
        public class MessageRingBufferNode : RingBufferNode<Message>
        {
        }
*/


        [PluginInfo(Name = "S+H", Category = "Message", Help = "Save a Message", Tags = "")]
        public class MessageSAndHNode : SAndH<Message>
        {
        }

        // better than the GetSlice (Node), because it allows binning and Index Spreading
        [PluginInfo(Name = "GetSlice", Category = "Message", Help = "GetSlice Messages", Tags = "velcrome")]
        public class MessageGetSliceNode : GetSlice<Message>
        {
        }

        [PluginInfo(Name = "Select", Category = "Message", Help = "Select Messages", Tags = "")]
        public class MessageSelectNode : Select<Message>
        {
        }

        [PluginInfo(Name = "Queue", Category = "Message", Help = "Queues all Messages", Tags = "")]
        public class MessageQueueNode : QueueNode<Message>
        {
        }

        [PluginInfo(Name = "Zip", Category = "Message", Version="Bin", Help = "Zip Messages", Tags = "")]
        public class MessageZipBinNode : Zip<IInStream<Message>>
        {
        }

        [PluginInfo(Name = "Zip", Category = "Message", Help = "Zip Messages", Tags = "")]
        public class MessageZipNode : Zip<Message>
        {
        }

        [PluginInfo(Name = "UnZip", Category = "Message", Help = "UnZip Messages", Tags = "")]
        public class MessageUnZipNode : Unzip<IInStream<Message>>
        {
        }

    }

}