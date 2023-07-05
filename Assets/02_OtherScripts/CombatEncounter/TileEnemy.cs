using UnityEngine;

public class TileEnemy : TileEntity
{
    public override void OnAwake()
    {
        ContentType = TileContentType.Enemy;
    }

    public override void OnTurnStart()
    {
        if (Random.Range(0, 1) == 0)
        {
            SwitchMode(Mode.Moving);
        }
        else
        {
            SwitchMode(Mode.Attacking);
        }
    }

    protected override void OnSwitchMode()
    {
        if (currentMode == Mode.Moving) 
        {
            movementTargetPosition = surroundingTilesMovementRange[TileContentType.Empty][Random.Range(0, surroundingTilesMovementRange[TileContentType.Empty].Count - 1)];
            ExecuteMode();
        }
        else if (currentMode == Mode.Attacking)
        {
            if (surroundingTilesAttackRange[TileContentType.Player].Count <= 0)
            {
                if (movingModeAvailable) { SwitchMode(Mode.Moving); }
                attackingModeAvailable = false;
                return;
            }

            attackTargetPosition = surroundingTilesAttackRange[TileContentType.Player][Random.Range(0, surroundingTilesMovementRange[TileContentType.Player].Count - 1)];
            ExecuteMode();
        }
    }
}

