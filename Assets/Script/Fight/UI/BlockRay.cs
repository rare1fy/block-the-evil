using UnityEngine;

public class BlockRay : UIBase
{
    public Vector2Int pos;
    public BlockData data;
    public void SetItem(Vector2Int pos)
    {
        this.pos = pos;
        data = GameManager.Instance.CurFightControl.Model.GetBlockDataByPos(pos);
    }
}
