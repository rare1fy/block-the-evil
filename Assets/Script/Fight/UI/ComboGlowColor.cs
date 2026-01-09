using Pb;
using UnityEngine;
using UnityEngine.UI;

public class ComboGlowColor : UIBase
{
    public RawImage img;

    public void SetColor(FighteffectBase cfg)
    {
        img.color = StringToColor(cfg.Color);
    }

    public Color StringToColor(string colorString)
    {
        if (string.IsNullOrEmpty(colorString))
        {
            return new Color(255, 255, 255, 255);
        }

        string[] parts = colorString.Split('#');

        if (parts.Length == 3) // RGB
        {
            int r = int.Parse(parts[0]);
            int g = int.Parse(parts[1]);
            int b = int.Parse(parts[2]);
            return new Color(r, g, b, 255);
        }
        else if (parts.Length == 4) // ARGB
        {
            int r = int.Parse(parts[0]);
            int g = int.Parse(parts[1]);
            int b = int.Parse(parts[2]);
            int a = int.Parse(parts[3]);
            return new Color(r, g, b, a);
        }
        return new Color(255, 255, 255, 255);
    }
}
