using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Assets/InstanceConfig")]
public class InstanceConfig : ScriptableObject {
    public List<IndirectInstanceData> Infos = new List<IndirectInstanceData>();
}