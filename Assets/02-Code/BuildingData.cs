using UnityEngine;

[CreateAssetMenu(menuName = "Buildings/Building Data")]
public class BuildingData : ScriptableObject
{
    public string displayName;
    public GameObject prefab;
    public int level = 1;
    public int initialCount = 1;
}