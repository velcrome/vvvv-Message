#region usings
using System.IO;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.OSC;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "OSCDecoder", Category = "Network", Version ="Advanced", Help = "Basic raw template which copies up to count bytes from the input to the output", Tags = "")]
    #endregion PluginInfo
    public class NetworkOSCSeriesNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        #region fields & pins
        [Input("Input")]
        public ISpread<Stream> FInput;

        [Input("Address")]
        public IDiffSpread<string> FAddress;
        protected List<Regex> ValidAddress = new List<Regex>();

        [Output("Argument")]
        public ISpread<ISpread<string>> FArgument;

        [Output("Type")]
        public ISpread<string> FType;

        [Output("Match")]
        public ISpread<ISpread<int>> FMatch;

        [Import()]
        protected ILogger FLogger;

        #endregion fields & pins

        //called when all inputs and outputs defined above are assigned from the host
        public void OnImportsSatisfied()
        {
            FAddress.Changed += UpdateAddress;
        }

        protected void UpdateAddress(IDiffSpread<string> spread)
        {
            ValidAddress.Clear();

            for (int i=0;i<FAddress.SliceCount;i++)
            {
                var address = FAddress[i];
                ValidAddress.Add(CreateWildCardRegex(address));
            }
        }

        public static string PeekAddress(Stream stream)
        {
            if (stream == null || stream.Length == 0) return "";

            string address = "";
            var enc = new ASCIIEncoding();

            stream.Position = 0;
            using (var reader = new BinaryReader(stream, enc, true)) // don't close stream when reader disposes
            {
                while (reader.PeekChar() != ',' && stream.Position < stream.Length)
                {
                    // todo: break early if not starting with '/' or hitting invalid char

                    var c = reader.ReadChars(4);
                    address += new string(c);
                }
            }

            return address.TrimEnd('\0'); // remove padding
        }

        //      use simple wildcard pattern: use * for any amount of characters (including 0) or ? for exactly one character.
        protected Regex CreateWildCardRegex(string wildcardPattern)
        {
            var regexPattern = "^" + Regex.Escape(wildcardPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return regex;
        }

        protected IEnumerable<int> IsMatch(Stream stream)
        {
            var address = PeekAddress(stream);
            for (int i=0;i<ValidAddress.Count;i++)
            {
                if (ValidAddress[i].IsMatch(address))
                    yield return i;
            }
            
        } 

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            spreadMax = FInput.SliceCount;
            if (FInput.SliceCount <= 0 || FInput[0] == null || FInput[0].Length == 0) spreadMax = 0;

            FArgument.SliceCount =
            FType.SliceCount =
            FMatch.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var stream = FInput[i];
                var match = IsMatch(stream).ToArray();
                FMatch[i].AssignFrom(match);

                if (match.Length == 0)
                {
                    FType[i] = "";
                    FArgument[i].SliceCount = 0;
                    continue;
                }

                try
                {
                    stream.Position = 0;
                    var length = (int)stream.Length;

                    byte[] buffer = new byte[length];
                    stream.Read(buffer, 0, length);

                    var osc = OSCPacket.Unpack(buffer, false) as OSCMessage;
                    string type = "";

                    var argCount = osc.Values.Count;
                    FArgument[i].SliceCount = argCount;

                    for (int j = 0; j < argCount; j++)
                    {
                        var arg = osc.Values[j];
                        FArgument[i][j] = arg.ToString();
                        type += arg.GetType() == typeof(int) ? "i" : "";
                        type += arg.GetType() == typeof(float) ? "f" : "";
                        type += arg.GetType() == typeof(double) ? "d" : "";
                        type += arg.GetType() == typeof(string) ? "s" : "";
                    }

                    FType[i] = type;
                } catch (Exception e)
                {
                    FLogger.Log(LogType.Error, "Error parsing OSC message.");
                    FLogger.Log(e, LogType.Debug);
                    FType[i] = "";
                    FArgument[i].SliceCount = 0;
                }
            }

        }
    }
}
