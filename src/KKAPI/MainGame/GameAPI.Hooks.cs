using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ActionGame;
using ADV;
using HarmonyLib;
using Illusion.Extensions;

namespace KKAPI.MainGame
{
    public static partial class GameAPI
    {
        private class Hooks
        {
            public static void SetupHooks(Harmony hi)
            {
                hi.PatchAll(typeof(Hooks));
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SaveData), nameof(SaveData.Load), new[] { typeof(string), typeof(string) })]
            public static void LoadHook(string path, string fileName)
            {
                OnGameBeingLoaded(path, fileName);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SaveData), nameof(SaveData.Save), new[] { typeof(string), typeof(string) })]
            public static void SaveHook(string path, string fileName)
            {
                OnGameBeingSaved(path, fileName);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), "Start")]
            public static void StartProcPost(HSceneProc __instance, ref IEnumerator __result)
            {
                var oldResult = __result;
                __result = new[] { oldResult, OnHStart(__instance) }.GetEnumerator();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), "NewHeroineEndProc")]
            public static void NewHeroineEndProcPost(HSceneProc __instance)
            {
                OnHEnd(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), "EndProc")]
            public static void EndProcPost(HSceneProc __instance)
            {
                OnHEnd(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Cycle), nameof(Cycle.Change), new Type[] { typeof(Cycle.Type) })]
            public static void CycleChangeTypeHook(Cycle.Type type)
            {
                OnPeriodChange(type);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Cycle), nameof(Cycle.Change), new Type[] { typeof(Cycle.Week) })]
            public static void CycleChangeWeekHook(Cycle.Week week)
            {
                OnDayChange(week);
            }

            [HarmonyDelegate(typeof(TextScenario), "RequestNextLine")]
            private delegate bool RequestNextLine();

            [HarmonyDelegate(typeof(ScenarioData), "MultiForce", typeof(Command))]
            private delegate bool MultiForce(Command command);

            [HarmonyDelegate(typeof(ScenarioData.Param), "ConvertAnalyze", typeof(Command), typeof(string[]), typeof(string))]
            private delegate string[] ConvertAnalyze(Command command, string[] args, string fileName);

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CommandList), "CommandGet", typeof(Command))]
            private static bool CommandGetReplacement(ref CommandBase __result, Command command)
            {
                switch (command)
                {
                    case (Command)(-1):
                        __result = null;
                        return false;
                    default:
                        if (!EventUtils.InjectedCommands.ContainsKey(command))
                        {
                            return true;
                        }

                        __result = (CommandBase)Activator.CreateInstance(EventUtils.InjectedCommands[command].Type);
                        return false;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ScenarioData.Param), "Initialize", typeof(string[]))]
            private static bool InitializeReplacement(ref string[] args, ref Command ____command, out bool ____multi,
                ref string[] ____args, ConvertAnalyze ConvertAnalyze, MultiForce MultiForce)
            {
                int count = 1;
                bool flag = bool.TryParse(args[count++], out ____multi);
                string self = args.SafeGet(count++);

                try
                {
                    ____command = (Command)Enum.ToObject(typeof(Command), self.Check(true, Enum.GetNames(typeof(Command))));
                    if (____command == (Command)(-1))
                    {
                        if (int.TryParse(self, out int result))
                        {
                            ____command = (Command)result;
                        }
                        else
                        {
                            foreach (Command c in EventUtils.InjectedCommands.Keys)
                            {
                                if (self == EventUtils.InjectedCommands[c].Type.Name)
                                {
                                    ____command = c;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw new Exception("CommandError:" + string.Join(",", (from s in args select (!s.IsNullOrEmpty()) ? s : "(null)").ToArray()));
                }

                if (!flag)
                {
                    ____multi |= MultiForce(____command);
                }

                ____args = ConvertAnalyze(____command, args.Skip(count).ToArray().LastStringEmptySpaceRemove(), null);
                return false; // Skip the original method.
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TextScenario), "CommandAdd", typeof(bool), typeof(int), typeof(bool), typeof(Command), typeof(string[]))]
            private static bool CommandAddReplacement(List<ScenarioData.Param> ___commandPacks, RequestNextLine RequestNextLine,
                bool isNext, int line, bool multi, Command command, params string[] args)
            {
                List<string> list = new List<string>(args?.Length + 3 ?? 3)
                {
                    "0",
                    multi.ToString(CultureInfo.InvariantCulture),
                    command.ToString(),
                };

                string[] collection = args;
                if (args == null)
                {
                    (collection = new string[1])[0] = string.Empty;
                }

                list.AddRange(collection);
                ___commandPacks.Insert(line, new ScenarioData.Param(list.ToArray()));
                if (isNext)
                {
                    RequestNextLine();
                }

                return false; // Skip the original method.
            }
        }
    }
}
