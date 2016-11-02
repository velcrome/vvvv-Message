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
            if (spread[0] is ISpread)
            {
                if ((spread[0] as ISpread).IsAnyInvalid()) return true;

                //for (int i= 0; i < spread.SliceCount;i++)
                //{
                //    if ((spread[i] as ISpread).IsAnyInvalid()) return true;
                //}
            }

            for (int i = 0; i < spreads.Length; i++)
            {
                if (spreads[i] == null) return true;
                if (spreads[i].SliceCount == 0 || spreads[i][0] == null) return true;
            }

            return false;
        }

        public static ISpread ToISpread(this IIOContainer pinContainer)
        {
            return (ISpread)(pinContainer.RawIOObject);
        }


        public static TypeRecord GetTypeRecord(this IIOContainer pinContainer)
        {
            var type = pinContainer.GetType();
            while (type.GenericTypeArguments.Length > 0)
            {
                type = type.GenericTypeArguments[0];
                if (TypeIdentity.Instance.ContainsKey(type)) return TypeIdentity.Instance[type];
            }

            return null;
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

        public static void FlushItem<T>(this ISpread<T> spread, T result)
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

        public static bool IsComparable(this Bin bin)
        {
            if (bin == null) return false;
            if (bin.IsAnyInvalid()) return false;

            if (bin.GetType() is IComparable) return true;
                else return false;
        }

        #endregion

    }
}