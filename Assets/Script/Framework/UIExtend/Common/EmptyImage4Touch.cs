using UnityEngine.UI;

public class EmptyImage4Touch : MaskableGraphic
{
	protected override void Start()
	{
		base.Start();
		raycastTarget = true;
	}

	protected EmptyImage4Touch()
	{
		useLegacyMeshGeneration = false;
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		// 清除顶点信息
		vh.Clear();
	}
}
