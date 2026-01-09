using UnityEngine.UI;

public class PromptItem : UIItemBase
{
    private CustomText _txtDesc;

    protected override void InitItem()
    {
        _txtDesc = GetNodeByName<CustomText>("_Txt_Desc");
    }

    public void SetDesc(string desc)
    {
        _txtDesc.text = desc;
    }
}
