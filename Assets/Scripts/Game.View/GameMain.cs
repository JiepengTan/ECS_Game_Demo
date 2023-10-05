using System;
using System.Collections.Generic;
using GamesTan.ECS.Game;
using NUnit.Framework;
using UnityEngine;

namespace GamesTan.Game.View {
    public unsafe class GameMain : MonoBehaviour {
        public void Start() {
            GameEcsManager.Instance.DoStart();
        }

    }
}