using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Profiler
{
    public static class ProfilerManager
    {
        private const int maxProfileCount = 50;
        private static Dictionary<string, Dictionary<string, Queue<long>>> profileDict = new Dictionary<string, Dictionary<string, Queue<long>>>();

        public static void OnEndProfile(string target, string method, long tick)
        {
            if (!profileDict.ContainsKey(target))
            {
                profileDict[target] = new Dictionary<string, Queue<long>>();
            }
            
            if (!profileDict[target].ContainsKey(method))
            {
                profileDict[target][method] = new Queue<long>();
            }

            Queue<long> queue = profileDict[target][method];
            if (queue.Count >= maxProfileCount)
            {
                queue.Dequeue();
            }

            queue.Enqueue(tick);
        }

        public static IEnumerable<string> GetTargets()
        {
            return profileDict.Keys;
        }

        public static IEnumerable<string> GetMethods(string target)
        {
            if (profileDict.ContainsKey(target))
            {
                return profileDict[target].Keys;
            }

            return null;
        }

        public static IEnumerable<long> GetProfileData(string target, string method)
        {
            if (profileDict.ContainsKey(target))
            {
                if (profileDict[target].ContainsKey(method))
                {
                    return profileDict[target][method];
                }
            }

            return null;
        }
    }
}
