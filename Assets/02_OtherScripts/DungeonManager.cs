using UnityEngine;
using UnityEngine.AI;

public class DungeonManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DungeonMapGenerator generator;
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private CombatRoomController combatRoomController;

    private int stage = 0;

    private GameUIView gameUIView;

    private void Awake()
    {
        GenerateMap();
    }

    private void Start()
    {
        gameUIView = (GameUIView)GameManager.UIViewManager.GetView(typeof(GameUIView));
        gameUIView.SetLevelIndicatorText(stage.ToString());
    }

    public void FinishStage()
    {
        stage++;
        gameUIView.SetLevelIndicatorText(stage.ToString());
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
