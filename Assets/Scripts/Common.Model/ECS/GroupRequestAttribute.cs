// Copyright 2019 谭杰鹏. All Rights Reserved //https://github.com/JiepengTan 

using System;
using System.Collections.Generic;
using System.Linq;

namespace GamesTan.ECS {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class GroupRequestAttribute : System.Attribute {
        public int[] Ids;
        static HashSet<int> set = new HashSet<int>();

        public GroupRequestAttribute(params int[] cmpIds){
            if (cmpIds.Length < 1) throw new Exception("GroupRequest at least has one Component");
            set.Clear();
            foreach (var id in cmpIds) {
                set.Add(id);
            }

            this.Ids = set.ToArray();
        }
    }
}