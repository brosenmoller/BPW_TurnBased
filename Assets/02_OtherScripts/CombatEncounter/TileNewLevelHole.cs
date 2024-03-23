

public class TileNewLevelHole : TileContent
{
    public override void OnAwake()
    {
        ContentType = TileContentType.NewLevelHole;
    }

    public override void Interact(TileEntity tileEntity)
    {
        combatRoomController.FinishStage();
    }
}
