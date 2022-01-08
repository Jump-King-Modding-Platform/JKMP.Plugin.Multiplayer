using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JumpKing;
using JumpKing.PauseMenu;

namespace JKMP.Plugin.Multiplayer.Patches
{
    [HarmonyPatch(typeof(JumpGame), "Update")]
    internal static class DisablePausePatch
    {
        private static readonly FieldInfo PauseManagerInstanceField = AccessTools.Field(typeof(PauseManager), nameof(PauseManager.instance));
        private static readonly MethodInfo IsPausedMethodInfo = AccessTools.PropertyGetter(typeof(PauseManager), nameof(PauseManager.IsPaused));
        
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();

            for (int i = 0; i < list.Count; ++i)
            {
                var instruction = list[i];

                if (i < list.Count - 3 && instruction.LoadsField(PauseManagerInstanceField) && list[i + 1].Calls(IsPausedMethodInfo))
                {
                    i += 2;
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}