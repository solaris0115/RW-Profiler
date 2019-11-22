using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

using System.Reflection;
using Harmony;
namespace Profiler
{
    public class ProfilerWindow : EditWindow
    {
        private string currentProfileKey;
        private Vector2 scrollPosition = Vector2.zero;
        private string typeMethodName = "";

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(600f, 450f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            inRect = inRect.ContractedBy(8f);
            Text.Font = GameFont.Tiny;
            GUI.BeginGroup(inRect);
            
            typeMethodName = Widgets.TextField(new Rect(0f, 0f, 400f, 30f), typeMethodName);

            if (Widgets.ButtonText(new Rect(410f, 0f, 120f, 30f), "Profile"))
            {
                int i = typeMethodName.LastIndexOf('.');
                string typeStr = typeMethodName.Substring(0, i);
                string methodStr = typeMethodName.Substring(i + 1);
                Type type = AccessTools.TypeByName(typeStr);
                if (type == null)
                {
                    Log.Error(string.Format("Type {0} is not exist.", typeStr));
                }

                MethodInfo methodInfo = AccessTools.Method(type, methodStr);
                if (methodInfo == null)
                {
                    Log.Error(string.Format("Method {0}.{1} is not exist.", typeStr, methodStr));
                }

                ProfilePatcher.Patch(HarmonyPatches.harmonyInstance, type, methodInfo);
            }


            List<TabRecord> tabList = new List<TabRecord>();
            foreach (string key in ProfilerManager.GetTargets())
            {
                tabList.Add(new TabRecord(key, delegate ()
                {
                    scrollPosition = Vector2.zero;
                    currentProfileKey = key;
                }, currentProfileKey == key));
            }

            if (currentProfileKey == null && ProfilerManager.GetTargets().Count() > 0)
            {
                currentProfileKey = ProfilerManager.GetTargets().First();
            }

            TabDrawer.DrawTabs(new Rect(0f, 70f, 560f, 330f), tabList);
            if (currentProfileKey != null)
            {
                int count = ProfilerManager.GetMethods(currentProfileKey).Count();
                Rect scrollOutRect = new Rect(0f, 90f, 560f, 320f);
                Rect scrollViewRect = new Rect(0f, 0f, 550f, count * 22f);

                float y = 0f;
                Widgets.BeginScrollView(scrollOutRect, ref scrollPosition, scrollViewRect, true);

                foreach (string method in ProfilerManager.GetMethods(currentProfileKey))
                {
                    long currentTick = ProfilerManager.GetProfileData(currentProfileKey, method).Max();

                    Widgets.Label(new Rect(0f, y, 420f, 22f), method);
                    Widgets.Label(new Rect(420f, y, 80f, 22f), currentTick.ToString());

                    y += 22f;
                }
                Widgets.EndScrollView();
            }

            GUI.EndGroup();
            Text.Font = GameFont.Small;
        }
    }
}
