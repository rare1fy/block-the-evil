//@qiuyukun
//2018/07/31 15:43:01

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace UnityEngine.UI
{
    public class CustomOutlineDouble : Shadow
    {
        public Color effectColor2 = Color.black;
        public Vector2 effectDistance2;

        protected CustomOutlineDouble()
        { }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            var verts = CustomListPool<UIVertex>.Get();
            vh.GetUIVertexStream(verts);

            var neededCpacity = verts.Count * 17;
            if (verts.Capacity < neededCpacity)
                verts.Capacity = neededCpacity;

            //先描第二次,需要被第一次覆盖
            var start = 0;
            var end = verts.Count;
            Profiler.BeginSample("outline");
            ApplyShadowZeroAlloc(verts, effectColor2, start, verts.Count, 0, -effectDistance2.y);
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor2, start, verts.Count, 0, effectDistance2.y);
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor2, start, verts.Count, -effectDistance2.x, 0);
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor2, start, verts.Count, effectDistance2.x, 0);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor2, start, verts.Count, effectDistance2.x, effectDistance2.y);
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor2, start, verts.Count, -effectDistance2.x, -effectDistance2.y);
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor2, start, verts.Count, effectDistance2.x, -effectDistance2.y);
            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor2, start, verts.Count, -effectDistance2.x, effectDistance2.y);

            //第一次描边
            start = end;
            end = verts.Count;

            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, -effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, -effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, 0);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, 0);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, 0, effectDistance.y);

            start = end;
            end = verts.Count;
            ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, 0, -effectDistance.y);
            Profiler.EndSample();

            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
            CustomListPool<UIVertex>.Release(verts);
        }
    }
}
