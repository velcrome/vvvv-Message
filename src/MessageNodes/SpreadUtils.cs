using System;
using System.Linq;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Packs.Messaging;

namespace VVVV.Utils
{
    public static class SpreadUtils
    {
        #region pin access
        public static bool IsAnyInvalid(this ISpread spread, params ISpread[] spreads)
        {
            if (spread == null) return true;

            if (spread.SliceCount == 0 || spread[0] == null) return true;
            else
            {
                for (int i = 0; i < spreads.Length; i++)
                {
                    if (spreads[i] == null) return true;
                    if (spreads[i].SliceCount == 0 || spreads[i][0] == null) return true;
                }
            }

            return false;
        }

        public static ISpread ToISpread(this IIOContainer pinContainer)
        {
            return (ISpread)(pinContainer.RawIOObject);
        }


        public static Type GetInnerMostType(this IIOContainer pinContainer)
        {
            var type = pinContainer.GetType();
            while (type.GenericTypeArguments.Length > 0)
                type = type.GenericTypeArguments[0];

            return type;
        }
        #endregion pin access

        #region fast flushing

        public static void FlushNil(this ISpread spread)
        {
            if (spread.SliceCount != 0)
            {
                spread.SliceCount = 0;
                spread.Flush();
            }
        }

        public static void FlushResult<T>(this ISpread<T> spread, IEnumerable<T> result)
        {
            spread.SliceCount = 0;
            if (result != null)
                spread.AssignFrom(result);
            spread.Flush();
        }

        public static void FlushBool(this ISpread<bool> spread, bool result)
        {
            spread.SliceCount = 1;
            spread[0] = result;
            spread.Flush();
        }

        public static void FlushInt(this ISpread<int> spread, int result)
        {
            spread.SliceCount = 1;
            spread[0] = result;
            spread.Flush();
        }

        #endregion fast flushing

        #region bin access

        public static bool IsAnyInvalid(this Bin bin)
        {
            if (bin == null) return true;

            if (bin.Count == 0 || bin[0] == null) return true;

            return false;
        }
        #endregion

    }
}