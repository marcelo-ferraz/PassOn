using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;

namespace PassOn.Utilities
{
    internal static class EmitMixins
    {
        public static void Construct(this ILGenerator il, Type type)
        {
            ConstructorInfo cInfo = type.GetConstructor(Type.EmptyTypes);
            if (cInfo != null)
            {
                il.Emit(OpCodes.Newobj, cInfo);
                return;
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call,
                type.GetMethod("GetType"));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Call,
                ((Func<Type, object>) FormatterServices.GetSafeUninitializedObject).Method);
            il.Emit(OpCodes.Castclass, type);
        }

        public static void EmitCopyWithCustomParser(this ILGenerator il, LocalBuilder cloneVariable, PropertyInfo srcProperty, PropertyInfo destProperty, int customPassOnIndex)
        {
            var @params =
                il.DeclareLocal(typeof(object[]));

            //IL_000e: ldloc.0
            il.Emit(OpCodes.Ldloc, cloneVariable);
            //IL_000f: ldarg.2
            il.Emit(OpCodes.Ldarg_1);
            //IL_0010: ldc.i4.1
            il.Emit(OpCodes.Ldc_I4, customPassOnIndex);
            //IL_0011: ldelem.ref
            il.Emit(OpCodes.Stelem_Ref);
            //IL_0012: ldc.i4.1
            il.Emit(OpCodes.Ldc_I4_1);
            //IL_0013: newarr [mscorlib]System.Object
            il.Emit(OpCodes.Newarr, typeof(object));
            //IL_0018: stloc.1
            il.Emit(OpCodes.Stloc, @params);
            //IL_0019: ldloc.1
            il.Emit(OpCodes.Ldloc, @params);
            //IL_001a: ldc.i4.0
            il.Emit(OpCodes.Ldc_I4_0);
            //IL_001b: ldarg.1
            il.Emit(OpCodes.Ldarg_0);
            //IL_001c: stelem.ref
            il.Emit(OpCodes.Stelem_Ref);
            //IL_001d: ldloc.1
            il.Emit(OpCodes.Localloc, @params);
            //IL_001e: callvirt instance object [mscorlib]System.Delegate::DynamicInvoke(object[])
            il.EmitCall(OpCodes.Callvirt, typeof(Delegate).GetMethod("DynamicInvoke"), new[] { typeof(object[]) });

            //IL_0023: unbox.any [mscorlib]System.Int32
            il.Emit(
                destProperty.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass,
                destProperty.PropertyType);

            //IL_0028: callvirt instance void PassOn.Tests.Model.BaseClass::set_Int(int32)            
            var setter =
                destProperty.GetSetMethod();

            il.Emit(
                setter.IsVirtual ? OpCodes.Callvirt : OpCodes.Call,
                setter);
        }

        public static void EmitDefaultCopy(this ILGenerator il, LocalBuilder cloneVariable, PropertyInfo srcProperty, PropertyInfo destProperty)
        {
            il.Emit(OpCodes.Ldloc, cloneVariable);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, srcProperty.GetGetMethod());
            il.Emit(OpCodes.Call, destProperty.GetSetMethod());
        }

        /// <summary>
        /// Helper method to clone a reference type.
        /// This method clones IList and IEnumerables and other reference types (classes)
        /// Arrays are not yet supported (ex. string[])
        /// </summary>
        /// <param name="il">IL il to emit code to.</param>
        /// <param name="cloneVar">Local store wheren the clone object is located. (or child of)</param>
        /// <param name="srcProperty">Property definition of the reference type to clone.</param>
        public static void EmitCopyRefType(this ILGenerator il, LocalBuilder cloneVar, PropertyInfo source, PropertyInfo destination)
        {
            // does not copy a delegate
            if (source.PropertyType.IsSubclassOf(typeof(Delegate))) { return; }

            il.Emit(OpCodes.Ldloc, cloneVar);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, source.GetGetMethod());
            il.Emit(OpCodes.Call,
                Get.CorrectClonningMethod(source.PropertyType, destination.PropertyType));
            il.Emit(OpCodes.Call, destination.GetSetMethod());
        }
    }
}
