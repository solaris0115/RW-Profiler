using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using System.Reflection;
using System.Reflection.Emit;

using Harmony;


namespace Profiler
{
    public static class ProfilePatcher
    {
        private static Type type;
        private static MethodInfo method;
        private static List<MethodInfo> patched = new List<MethodInfo>();

        public static void Patch(HarmonyInstance harmony, Type callerType, MethodInfo targetMethod)
        {
            if (patched.Contains(targetMethod))
            {
                return;
            }

            patched.Add(targetMethod);

            type = callerType;
            method = targetMethod;

            harmony.Patch(method, transpiler: new HarmonyMethod(typeof(ProfilePatcher), nameof(UpdatePlayTranspiler)));
        }

        public static string GetTargetName()
        {
            return type.Name + "." + method.Name;
        }

        private static IEnumerable<CodeInstruction> UpdatePlayTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instList = codeInstructions.ToList();

            for (int i = 0; i < instList.Count; ++i)
            {
                if (instList[i].opcode == OpCodes.Call || instList[i].opcode == OpCodes.Callvirt)
                {
                    MethodInfo callMethod = instList[i].operand as MethodInfo;
                    
                    DynamicMethod startProfileMethod = new DynamicMethod(GetTargetName() + ":" + callMethod.Name + "_StartProfile", null, null);
                    var ilStart = startProfileMethod.GetILGenerator();
                    ilStart.Emit(OpCodes.Ldstr, GetTargetName());
                    ilStart.Emit(OpCodes.Ldstr, callMethod.Name);
                    ilStart.EmitCall(OpCodes.Call, AccessTools.Method(typeof(ProfilerUtility), nameof(ProfilerUtility.StartProfile)), null);
                    ilStart.Emit(OpCodes.Ret);

                    yield return new CodeInstruction(OpCodes.Call, startProfileMethod);
                    yield return instList[i];

                    DynamicMethod endProfileMethod = new DynamicMethod(GetTargetName() + ":" + callMethod.Name + "_EndProfile", null, null);
                    var ilEnd = endProfileMethod.GetILGenerator();
                    ilEnd.Emit(OpCodes.Ldstr, GetTargetName());
                    ilEnd.Emit(OpCodes.Ldstr, callMethod.Name);
                    ilEnd.EmitCall(OpCodes.Call, AccessTools.Method(typeof(ProfilerUtility), nameof(ProfilerUtility.EndProfile)), null);
                    ilEnd.Emit(OpCodes.Ret);
                    yield return new CodeInstruction(OpCodes.Call, endProfileMethod);
                }
                else
                {
                    yield return instList[i];
                }
            }
        }
    }
}
