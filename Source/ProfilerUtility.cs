using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Harmony;

namespace Profiler
{
    public static class ProfilerUtility
    {
        public static Dictionary<KeyValuePair<string, string>, Stopwatch> stopwatchDict = new Dictionary<KeyValuePair<string, string>, Stopwatch>();
        public static void StartProfile(string target, string method)
        {
            stopwatchDict[new KeyValuePair<string, string>(target, method)] = Stopwatch.StartNew();
        }

        public static void EndProfile(string target, string method)
        {
            if (stopwatchDict.ContainsKey(new KeyValuePair<string, string>(target, method)))
            {
                ProfilerManager.OnEndProfile(target, method, stopwatchDict[new KeyValuePair<string, string>(target, method)].ElapsedTicks);
            }
        }
    }
}
