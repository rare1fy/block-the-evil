using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class MultiFlyAnimation
{
    public Action OnAllStartFly = null;
    public Action OnOneFlyFinish = null;
    public Action OnAllFlyFinish = null;
    public Action OnFirstFlyFinish = null;
    /// <summary>
    /// 是否正在使用
    /// </summary>
    public bool IsUse = false;

    /// <summary>
    /// 飞飞飞
    /// </summary>
    /// <param name="flyObj">生成的图片</param>
    /// <param name="parent">生成的图片的父物体</param>
    /// <param name="target">飞过去的目标</param>
    /// <param name="count">生成几个图片</param>
    /// <param name="scale">图片大小</param>
    /// <param name="randomRange">生成的范围</param>
    /// <param name="stayTime">停留时间</param>
    /// <param name="flyTime">飞行时间</param>
    /// <param name="flyIntervalTime">单个飞行间隔时间</param>
    public void DoMultiFlyAnimation(Image flyObj, RectTransform parent, RectTransform target, int count,
        float scale, Vector4 randomRange,float stayTime = 0f, float flyTime = 1.25f,float flyIntervalTime = 0.04f)
    {
        IsUse = true;
        //const float flyTime = 0.4f;
        //const float randomRange = 80f;
        //潜规则咯
        Vector2 targetSize = flyObj.rectTransform.sizeDelta;
        if (scale > 0)
            targetSize = new Vector2(56, 56) * scale;
        count = count > 15 ? 15 : count;
        RectTransform[] rectTransforms = new RectTransform[count];
        GameObject[] gameObjects = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            var obj = Object.Instantiate(flyObj.gameObject, parent);
            obj.SetActiveEx(true);
            obj.transform.localPosition = Vector3.zero;
            gameObjects[i] = obj;
            var rect = obj.GetComponent<RectTransform>();
            rectTransforms[i] = rect;

            rect.anchorMax = Vector2.one * 0.5f;
            rect.anchorMin = Vector2.one * 0.5f;
            rect.sizeDelta = Vector2.zero;
        }

        int fadeFinishCount = 0;
        int flyFinishCount = 0;
        float startT = 0f;
        for (int i = 0; i < rectTransforms.Length; i++)
        {
            Sequence sequence = DOTween.Sequence();

            sequence.AppendInterval(startT += flyIntervalTime);
            float round1Time = 0.2f;
            var a1 = rectTransforms[i].DOSizeDelta(targetSize, round1Time);
            sequence.Append(a1);

            sequence.InsertCallback(startT + round1Time / 2, null);

            var img = rectTransforms[i].GetComponent<Image>();
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
            var a2 = img.DOFade(1, round1Time);
            sequence.Join(a2);

            var moveBy = new Vector2(Random.Range(randomRange.x, randomRange.y), Random.Range(randomRange.z, randomRange.w));
            var a3 = rectTransforms[i].DOBlendableLocalMoveBy(moveBy, round1Time).SetEase(Ease.Linear);
            sequence.Join(a3);

            sequence.AppendCallback(() =>
            {
                fadeFinishCount++;
                if (fadeFinishCount == count && OnAllStartFly != null)
                {
                    OnAllStartFly();
                }
            });

            //float stayTime = 0.4f;
            sequence.AppendInterval(stayTime);
            var a4 = rectTransforms[i].DOMove(target.position, flyTime).SetEase(Ease.InExpo);
            sequence.Append(a4);

            //var a5 = rectTransforms[i].DOSizeDelta(targetSize, flyTime);
            //sequence.Join(a5);

            var i1 = i;
            sequence.OnComplete(() =>
            {
                if (OnOneFlyFinish != null)
                    OnOneFlyFinish();
                
                flyFinishCount++;
                if (flyFinishCount == 1 && OnFirstFlyFinish != null)
                    OnFirstFlyFinish();

                Object.Destroy(gameObjects[i1]);
                if (flyFinishCount == count)
                {
                    OnAllFlyFinish?.Invoke();
                    IsUse = false;
                    Clear();
                }
            });

            sequence.Play();
        }
    }

    public void Clear()
    {
        OnAllStartFly = null;
        OnFirstFlyFinish = null;
        OnOneFlyFinish = null;
        OnAllFlyFinish = null;
        IsUse = false;
    }
}
