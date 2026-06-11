using Framework;
using UnityEngine;

public partial class Config_BlockColor
{
    private const string WhiteBlockImg = "UI_zdn_img_bai";

    /// <summary>
    /// 获取颜色图片
    /// </summary>
    /// <param name="colorType"></param>
    /// <param name="touMing"> 是否需要无底的图片 </param>
    /// <returns></returns>
    public string GetColorImg(int colorType, bool touMing = false)
    {
        string colorStr;
        if (colorType == 0)
        {
            colorStr = "UI_zdn_img_touming";
        }
        else if (colorType == BlockData.WhiteColorType)
        {
            colorStr = WhiteBlockImg;
        }
        else
        {
            var cfg  = Config.GetConfig<Config_BlockColor>().GetConfigById(colorType);
            if (colorType > 5 && touMing)
            {
                colorStr = cfg.Img1;
            }
            else
            {
                colorStr = cfg.Img;
            }
           
        }
        
        return colorStr;
    }
}
