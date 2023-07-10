using UnityEngine;
using UnityEngine.AI;

public class DungeonManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonMapGenerator generator;
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private CombatRoomController combatRoomController;

    private void Awake()
    {
        generator.GenerateMap(Random.Range(0, 1000));
        navMeshSurface.RemoveData();
        navMeshSurface.BuildNavMesh();
        combatRoomController.Setup();
    }
}

