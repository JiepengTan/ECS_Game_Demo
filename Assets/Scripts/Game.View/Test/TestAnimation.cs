using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class TestAnimation : MonoBehaviour {
    public Material Material;
    public bool EnableAnimation = true;
    [Range(0, 300)] public int Frame;
    public bool isAutoUpdate;
    public int MaxFrame = 300;
    [Range(1, 60)] public float FrameRate = 30;
    private Matrix4x4 _matrix;

    private float timer;

    public void Update() {
        if (Material == null) {
            return;
        }

        if (!Application.isPlaying) {
            Material.SetFloat("_EnableInstance", 0);
        }
        else {
            Material.SetFloat("_EnableInstance", 1);
        }

        if (isAutoUpdate) {
            timer += Time.deltaTime;
            float interval = 1 / FrameRate;
            while (timer > interval) {
                timer -= interval;
                Frame = (++Frame) % MaxFrame;
            }
        }

        _matrix.m00 = 1;
        _matrix.m02 = Frame;
        Material.SetFloat("_EnableAnimation", EnableAnimation ? 1 : 0);
        Material.SetMatrix("_AnimationState", _matrix);
    }
}