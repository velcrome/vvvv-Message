#region usings
using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

#endregion usings

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Reflect", Category = "Object", Help = "Transcode any VL type to Message", Tags = "vl")]
    #endregion PluginInfo
    public class MessageReflectFromVL : AbstractFormularableNode
    {
        #region fields & pins
        [Input("Input")]
        public ISpread<object> FInput;

        [Input("Learn", IsSingle = true, IsBang = true)]
        public ISpread<bool> FLearn;

        [Output("Name")]
        public ISpread<string> FName;

        [Output("Output")]
        public ISpread<Message> FOutput;


        protected Dictionary<string, Func<object, object>> Getters = new Dictionary<string, Func<object, object>>();
        #endregion fields & pins

        //called when data for any output pin is requested
        public override void Evaluate(int SpreadMax)
        {
            var FieldCount = Getters.Count;
            if (FLearn[0] && FInput.SliceCount > 0 && FInput[0] != null)
            {
                var type = FInput[0].GetType();

                FLogger.Log(LogType.Debug, "Learning Type... "+type.FullName);

                FieldInfo[] fields = type.GetFields();

                FieldCount = fields.Length;
                FName.SliceCount = FieldCount;

                Getters.Clear();
                var formular = new MessageFormular(type.FullName, "");
                for (var i = 0; i < FieldCount; i++)
                {
                    var name = fields[i].Name;
                    FName[i] = name;

                    var fieldInfo = type.GetField(name);

                    try
                    {
                        formular.Append(new FormularFieldDescriptor(fieldInfo.FieldType, name, 1), true);

                        var getter = fieldInfo.CompileGetter();
                        Getters.Add(name, getter);

                        FLogger.Log(LogType.Debug, "Success: " + fieldInfo.FieldType.Name + " " + name);
                    }
                    catch (Exception)
                    {
                        FLogger.Log(LogType.Debug, "Failed: " + fieldInfo.FieldType.Name + " " + name);
                        //FLogger.Log(ex, LogType.Debug);
                    }
                }
                Formular = formular;
            }

            SpreadMax = FInput.SliceCount;
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var m = new Message(Formular.Name);
                foreach (var fieldName in Formular.FieldNames)
                {
                    var getter = Getters[fieldName];

                    var bin = BinFactory.New(Formular[fieldName].Type);
                    bin.Add(getter(FInput[i]));
                    m[fieldName] = bin;

                }
                FOutput[i] = m;
            }

        }
    }

    public static class ReflectionUtility
    {
        public static Func<object, object> CompileGetter(this FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, typeof(object), new[] { typeof(object) }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldsfld, field);
                gen.Emit(field.FieldType.IsClass ? OpCodes.Castclass : OpCodes.Box, field.FieldType);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Castclass, field.DeclaringType);
                gen.Emit(OpCodes.Ldfld, field);
                gen.Emit(field.FieldType.IsClass ? OpCodes.Castclass : OpCodes.Box, field.FieldType);
            }
            gen.Emit(OpCodes.Ret);
            return (Func<object, object>)setterMethod.CreateDelegate(typeof(Func<object, object>));
        }

        public static Action<object, object> CompileSetter(this FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, null, new[] { typeof(object), typeof(object) }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(field.FieldType.IsClass ? OpCodes.Castclass : OpCodes.Unbox_Any, field.FieldType);
                gen.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Castclass, field.DeclaringType);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(field.FieldType.IsClass ? OpCodes.Castclass : OpCodes.Unbox_Any, field.FieldType);
                gen.Emit(OpCodes.Stfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return (Action<object, object>)setterMethod.CreateDelegate(typeof(Action<object, object>));
        }
    }
}
