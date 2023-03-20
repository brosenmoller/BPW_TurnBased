using UnityEngine;

public class GridEnemy : GridTileContent
{
    public override void OnTurnStart()
    {
        tilesInMovementRange = CalculateMovementRangeTiles(grid.WorldToCell(transform.position));

        agent.isStopped = false;
        agent.SetDestination(tilesInMovementRange[Random.Range(0, tilesInMovementRange.Length - 1)]);
    }

    private void FixedUpdate()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && agent.isStopped == false)
        {
            agent.isStopped = true;
            combatRoomController.CurrentTurnEnd();
        }
    }
}

