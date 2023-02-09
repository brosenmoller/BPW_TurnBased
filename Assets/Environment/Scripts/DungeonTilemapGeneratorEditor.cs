using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonTilemapGenerator), true)]
public class DungeonTilemapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DungeonTilemapGenerator cc = (DungeonTilemapGenerator)target;

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
