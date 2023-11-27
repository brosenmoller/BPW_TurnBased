using UnityEngine;
using UnityEngine.AI;

public class DungeonManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonMapGenerator generator;
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private CombatRoomController combatRoomController;

    private int stage = 0;

    private void Awake()
    {
        GenerateMap();
    }

    public void FinishStage()
    {
        stage++;
        Debug.Log(stage);
        GenerateMap();
    }

    private void GenerateMap()
    {
        generator.GenerateMap(Random.Range(0, 100000));
        navMeshSurface.hideEditorLogs = true;
        navMeshSurface.RemoveData();
        navMeshSurface.BuildNavMesh();
        Invoke(nameof(SetupCombatRoomController), .1f);
    }

    private void SetupCombatRoomController()
    {
        combatRoomController.Setup(this);
    }
}
