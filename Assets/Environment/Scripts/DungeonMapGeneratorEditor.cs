using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonMapGenerator), true)]
public class DungeonMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DungeonMapGenerator cc = (DungeonMapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (cc.autoUpdate)
            {
                cc.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            cc.GenerateMap();
        }
    }
}
