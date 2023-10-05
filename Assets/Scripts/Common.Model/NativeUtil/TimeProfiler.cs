using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Debug = UnityEngine.Debug;

namespace Lockstep.Util
{
    public class TimeProfiler
    {
        public class Info
        {
            public int Count;
            public double Time;
        }

        private static Dictionary<string, DateTime> _tag2DateTime = new Dictionary<string, DateTime>();
        private static Dictionary<string, Info> _tag2Time = new Dictionary<string, Info>();

        [Conditional("DEBUG")]
        public static void OnFrameStart()
        {
            _tag2DateTime.Clear();
            _tag2Time.Clear();
        }
        [Conditional("DEBUG")]
        public static void OnFrameEnd()
        {
        }


        [Conditional("DEBUG")]
        public static void BeginFrameTime(object tag)
        {
            BeginFrameTime(tag.ToString());
        }

        [Conditional("DEBUG")]
        public static void EndFrameTime(object tag)
        {
            EndFrameTime(tag.ToString());
            Profiler.EndSample();
        }
        
        [Conditional("DEBUG")]
        public static void BeginFrameTime(string tag)
        {
            _tag2DateTime[tag] = DateTime.Now;
            Profiler.BeginSample(tag);
        }

        [Conditional("DEBUG")]
        public static void EndFrameTime(string tag)
        {
            if (!_tag2DateTime.ContainsKey(tag))
            {
                Debug.LogError("Can not Find Match tag" + tag);
                Profiler.EndSample();
                return;
            }

            var useTime = (DateTime.Now - _tag2DateTime[tag]).TotalMilliseconds;
            if (!_tag2Time.ContainsKey(tag))
            {
                _tag2Time[tag] =new Info();
            }
            _tag2Time[tag].Time+= useTime;
            _tag2Time[tag].Count++;
            Profiler.EndSample();
        }
        public static string GetProfilerInfo()
        {
            StringBuilder sb = new StringBuilder();
            var pairs = _tag2Time.ToList();
            pairs.Sort((a,b)=>a.Key.CompareTo(b.Key));
            foreach (var pair in pairs)
            {
                sb.AppendLine($"{pair.Key} : {pair.Value.Time.ToString("0.000")}ms  count={pair.Value.Count}");
            }
            return sb.ToString();
        }
    }
}