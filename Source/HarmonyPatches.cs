using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using Verse;
using RimWorld;
using Harmony;
namespace Profiler
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        public static HarmonyInstance harmonyInstance;
        public static Stopwatch stopwatch = new Stopwatch();
        public static StreamWriter stream;

        static HarmonyPatches()
        {
            harmonyInstance = HarmonyInstance.Create("rimworld.gguake.profiler");
            ProfilePatcher.Patch(harmonyInstance, typeof(Game), AccessTools.Method(typeof(Game), "UpdatePlay"));

            harmonyInstance.Patch(AccessTools.Method(typeof(DebugWindowsOpener), "DevToolStarterOnGUI"), 
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DevToolStarterOnGUIPostfix)));

            Log.Message("profiler init");
        }

        public static void DevToolStarterOnGUIPostfix()
        {
            if (Event.current.isKey && Event.current.keyCode == KeyCode.F8 && Event.current.control)
            {
                Find.WindowStack.Add(new ProfilerWindow());
                Event.current.Use();
            }
        }
    }
}
