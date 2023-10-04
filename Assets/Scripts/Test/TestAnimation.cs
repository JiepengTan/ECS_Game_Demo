using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TestAnimation : MonoBehaviour
{
    public Renderer Renderer;
    private Material mat;
    [Range(0,300)]
    public int Frame;
    private Matrix4x4 _matrix;
    private bool _EnableAnimation = true;

    public void Start()
    {
        mat = new Material(Renderer.sharedMaterial);
        Renderer.material = mat;
    }


    public void Update()
    {
        _matrix.m00 = 1;
        _matrix.m02 = Frame;
        mat.SetFloat("_EnableAnimation",_EnableAnimation?1:0);
        mat.SetMatrix("_AnimationState",_matrix); 
    }
}
