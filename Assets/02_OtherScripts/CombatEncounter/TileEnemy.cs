﻿using UnityEngine;

public class TileEnemy : TileEntity
{
    public override void OnAwake()
    {
        base.OnAwake();
        ContentType = TileContentType.Enemy;
    }

    protected override void OnTurnStart()
    {
        if (Random.Range(0, 2) == 0)
        {
            SwitchMode(Mode.Moving);
        }
        else
        {
            SwitchMode(Mode.Attacking);
        }
    }

    protected override void OnNoModeAvailable()
    {
        OnTurnEnd();
        TurnEnd();
    }

    protected override void OnSwitchMode()
    {
        if (currentMode == Mode.Moving) 
        {
            if (surroundingTilesMovementRange[TileContentType.Empty].Count <= 0)
            {
                movingModeAvailable = false;
                SwitchMode(Mode.Attacking);
                return;
            }

            movementTargetPosition = surroundingTilesMovementRange[TileContentType.Empty][Random.Range(0, surroundingTilesMovementRange[TileContentType.Empty].Count - 1)];
            ExecuteMode();
        }
        else if (currentMode == Mode.Attacking)
        {
            if (surroundingTilesAttackRange[TileContentType.Player].Count <= 0)
            {
                if (!movingModeAvailable) { attackingModeAvailable = false; }
                SwitchMode(Mode.Moving);
                return;
            }

            attackTargetPosition = surroundingTilesAttackRange[TileContentType.Player][Random.Range(0, surroundingTilesAttackRange[TileContentType.Player].Count - 1)];
            ExecuteMode();
        }
    }

    protected override void OnDeath()
    {
        combatRoomController.RemoveEnemy(this);
    }
}

