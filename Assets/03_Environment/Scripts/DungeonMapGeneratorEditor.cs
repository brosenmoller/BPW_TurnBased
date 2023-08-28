#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonMapGenerator), true)]
public class DungeonMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DungeonMapGenerator dungeonMapGenerator = (DungeonMapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (dungeonMapGenerator.autoUpdate)
            {
                dungeonMapGenerator.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            dungeonMapGenerator.GenerateMap();
        }
    }
}
#endif
