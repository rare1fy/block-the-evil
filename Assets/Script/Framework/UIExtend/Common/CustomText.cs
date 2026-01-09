using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Custom Text", 11)]
[ExecuteInEditMode]
public class CustomText : Text, ILanguageRefresh
{
    /// <summary>
    /// 是否大写
    /// </summary>
    [HideInInspector]
    public bool IsCapital = false;

    readonly UIVertex[] m_TempVerts = new UIVertex[4];
    public string mTextById
    {
        get
        {
            var value = LanguageManager.Instance.AnalysiseLanguageText(text, IsCapital);
            return value;
        }
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;
        m_DisableFontTextureRebuiltCallback = true;
        Vector2 extents = rectTransform.rect.size;
        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.PopulateWithErrors(mTextById, settings, gameObject);

        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        int vertCount = 0;
        vertCount = verts.Count;
        if (vertCount <= 0)
        {
            toFill.Clear();
            return;
        }
        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;

        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();
        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }
        m_DisableFontTextureRebuiltCallback = false;
    }

    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(GetPixelAdjustedRect().size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(mTextById, settings) / pixelsPerUnit;
        }
    }

    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(mTextById, settings) / pixelsPerUnit;
        }
    }

    public void LanguageRefresh()
    {
        SetVerticesDirty();
    }
}