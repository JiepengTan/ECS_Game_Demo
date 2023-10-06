using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;


public class Example : MonoBehaviour
{
    #region Variables

    // Public
    public bool indirectRenderingEnabled = true;
    public bool createInstancesOnAwake = false;
    public bool shouldInstantiatePrefabs = false;
    public float areaSize = 5000f;
    public NumberOfInstances numberOfInstances;
    public IndirectRenderer indirectRenderer;
    public IndirectInstanceData[] instances;

    // Enums
    public enum NumberOfInstances
    {
        _64 = 64,
        _256 = 256,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192,
        _16384 = 16384,
        _32768 = 32768,
        _65536 = 65536,
        _131072 = 131072,
        _262144 = 262144
    }

    private bool lastIndirectRenderingEnabled = false;
    private bool lastIndirectDrawShadows = false;
    private GameObject normalInstancesParent;
    #endregion

    #region MonoBehaviour



    #endregion

    #region Private Functions

    private void SetShadowCastingMode(ShadowCastingMode newMode)
    {
        Renderer[] rends = normalInstancesParent.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].shadowCastingMode = newMode;
        }
    }


    // Taken from:
    // http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/
    // https://www.shadertoy.com/view/4dtBWH
    private Vector2 Nth_weyl(Vector2 p0, float n)
    {
        Vector2 res = p0 + n * new Vector2(0.754877669f, 0.569840296f);
        res.x %= 1;
        res.y %= 1;
        return res;
    }

    #endregion
}
