using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace VVVV.Utils
{
    public static class SpreadUtils
    {
        public static bool IsAnyInvalid(this ISpread spread, params ISpread[] spreads)
        {
            if (spread.SliceCount == 0 || spread[0] == null) return true;
            else
            {
                for (int i = 0; i < spreads.Length; i++)
                {
                    if (spreads[i].SliceCount == 0 || spreads[i][0] == null) return true;
                }
            }

            return false;
        }

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
        public static IEnumerable ToEnumerable(this ISpread spread)  
        {
            for (int i = 0; i < spread.SliceCount; i++) 
                yield return spread[i];
        }


        #region cast tools
        public static VVVV.PluginInterfaces.V2.NonGeneric.ISpread ToISpread(this IIOContainer pin)
        {
            return (VVVV.PluginInterfaces.V2.NonGeneric.ISpread)(pin.RawIOObject);
        }

        public static VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread ToIDiffSpread(this IIOContainer pin)
        {
            return (VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread)(pin.RawIOObject);
        }
        public static VVVV.PluginInterfaces.V2.ISpread<T> ToGenericISpread<T>(this IIOContainer pin)
        {
            return (VVVV.PluginInterfaces.V2.ISpread<T>)(pin.RawIOObject);
        }

        public static Type GetInnerMostType(this IIOContainer pin)
        {
            var type = pin.GetType();
            while (type.GenericTypeArguments.Length > 0)
                type = type.GenericTypeArguments[0];
            
           return type;
        }
        #endregion cast tools
    }
}