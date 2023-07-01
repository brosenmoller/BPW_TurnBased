using UnityEngine;

public class GridEnemy : GridTileContent
{
    public override void OnAwake()
    {
        ContentType = GridTileContentType.Enemy;
    }

    public override void OnTurnStart()
    {
        CalculateSurroundingTiles(grid.WorldToCell(transform.position));

        agent.isStopped = false;
        agent.SetDestination(surroundingTiles[GridTileContentType.Empty][Random.Range(0, surroundingTiles[GridTileContentType.Empty].Count - 1)]);
    }

    private void FixedUpdate()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && agent.isStopped == false)
        {
            TurnEnd();
        }
    }
}

