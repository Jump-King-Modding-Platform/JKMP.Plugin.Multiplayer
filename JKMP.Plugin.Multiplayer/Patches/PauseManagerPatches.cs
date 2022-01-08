using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using EntityComponent;
using HarmonyLib;
using JumpKing.Controller;
using JumpKing.PauseMenu;

namespace JKMP.Plugin.Multiplayer.Patches
{
    [HarmonyPatch(typeof(PauseManager), nameof(PauseManager.PauseUpdate))]
    internal static class PauseManagerPatches
    {
        private static readonly MethodInfo UpdateComponentsMethod = AccessTools.Method(typeof(Entity), nameof(Entity.UpdateComponents));
        private static readonly MethodInfo GetIsPausedMethod = AccessTools.PropertyGetter(typeof(PauseManager), nameof(PauseManager.IsPaused));
        
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = instructions.ToList();
            Label retLabel = generator.DefineLabel();

            for (int i = 0; i < list.Count; ++i)
            {
                var instruction = list[i];

                if (instruction.Calls(GetIsPausedMethod) && list[i + 1].Branches(out _))
                {
                    list[i + 1].operand = retLabel;
                    yield return instruction;
                }
                else if (i < list.Count - 3 &&
                    instruction.opcode == OpCodes.Ldarg_0 &&
                    list[i + 1].opcode == OpCodes.Ldarg_1 &&
                    list[i + 2].Calls(UpdateComponentsMethod))
                {
                    // Skip calling UpdateComponents. It's not necessary due to pausing not actually pausing the game anymore.
                    // If left unpatched the PauseManager components update twice on each update.
                    i += 2;
                }
                else if (instruction.opcode == OpCodes.Ret)
                {
                    instruction.labels.Add(retLabel);
                    yield return instruction;
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}