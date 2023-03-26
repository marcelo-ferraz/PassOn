using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Utilities
{
    internal class Debugger
    {
        [Flags]
        internal enum Params {
            Source = 1 << 0,
            Target = 1 << 1,
            Engine = 1 << 2,
            Index = 1 << 3,
            Result = 1 << 3,
        }

        internal static void EmitDebuggers(ILGenerator il)
        {
            //il.Emit(OpCodes.Ldarg_0);
            //il.Emit(OpCodes.Ldarg_1);
            //il.Emit(OpCodes.Ldarg_2);
            //il.Emit(OpCodes.Ldloc_0);
            //il.Emit(OpCodes.Call, typeof(Debugger).GetMethod("Inspect", new Type[] { typeof(object), typeof(object), typeof(object), typeof(object) }));
        }

        public static void Inspect(object param1, object param2, object param3, object result)
        {
            if (param1 == null)
            {
                System.Diagnostics.Debugger.Break();
            }
            if (param2 == null)
            {
                System.Diagnostics.Debugger.Break();
            }
            if (param3 == null)
            {
                System.Diagnostics.Debugger.Break();
            }

            if (result == null)
            {
                System.Diagnostics.Debugger.Break();
            }


        }

        public static void Inspect<Source, Target>(Source source, Target target, PassOnEngine engine, int recursionIndex)
        {
            System.Diagnostics.Debugger.Break();
        }

        public static void Inspect<Source, Target>(Source source, PassOnEngine engine, int recursionIndex)
        {
            System.Diagnostics.Debugger.Break();
        }
    }
}
