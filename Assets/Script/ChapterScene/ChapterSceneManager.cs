using DG.Tweening;
using Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ChapterSceneManager : MonoBehaviour
{
    public static ChapterSceneManager _Instance;
    public GameObject Mask;
    public Camera _camera;
    public Animator animator;

    private Dictionary<string,ChapterScene> LoadedScenes = new();
    private ChapterScene curScene;
    private Vector2 boundaryX = new Vector2(-3,3);
    private Vector2 boundaryY = new Vector2(-0, 0);

    private float _cameraHight = -10;

    public void Awake()
    {
        _Instance = this;
        animator.enabled = false;
        EventDispatchCenter.Instance.Registry(SDEvents.BUILD_ARCHITECTURE, OnBuilding);
    }

    public void OnEnable()
    {
    }

    public void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.BUILD_ARCHITECTURE, OnBuilding);
    }


    public void RefreshCurrChapter(Action call = null)
    {
        return;
        int chapter = GameManager.Instance.ChapterControl.Model.curChapter;
        ChangeChapter(chapter, call);
    }
  
    public void MoveCamare(float moveX,float moveY)
    {
        Vector3 pos = _camera.transform.position;
        float practicalX = moveX + pos.x;
        float practicalY = moveY + pos.y;

        if (moveX + pos.x > boundaryX.y)
        {
            practicalX = boundaryX.y;
        }
        else if (moveX + pos.x < boundaryX.x)
        {
            practicalX = boundaryX.x;
        }

        if (moveY + pos.y > boundaryY.y)
        {
            practicalY = boundaryY.y;
        }
        else if (moveY + pos.y < boundaryY.x)
        {
            practicalY = boundaryY.x;
        }
        _camera.transform.position = new Vector3(practicalX, practicalY, pos.z);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chapterId"></param>
    /// <param name="call">���ؽ����ص�</param>
    public void ChangeChapter(int chapterId, Action call= null)
    {
        var cfg = Config.GetConfig<Config_ChapterBase>().GetConfigById(chapterId);
        Mask.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 255);
        animator.enabled = false;
        if (curScene != null&& cfg.NamePrefab == curScene.name)
        {
            GameManager.Instance.PerformControl.SetAwaitEnd();
            EventDispatchCenter.Instance.Dispatch(SDEvents.PERFORM_AWAIT_END);
            call?.Invoke();
        }
        else
        {
            Mask.SetActiveEx(curScene!=null);
            if (!LoadedScenes.TryGetValue(cfg.NamePrefab, out var scene))
            {
                ResourceManagerNew.instance.LoadAssetAsync<GameObject>(cfg.NamePrefab, (obj) =>
                {
                    curScene?.gameObject.SetActiveEx(false);
                    var chapter = Instantiate(obj, this.transform);
                    chapter.name = cfg.NamePrefab;
                    curScene = chapter.GetComponent<ChapterScene>();
                    curScene.transform.SetAsFirstSibling();
                    curScene.RefreshView();
                    LoadedScenes.TryAdd(chapter.name, curScene);
                    this.gameObject.SetActiveEx(false);
                    this.gameObject.SetActiveEx(true);
                    call?.Invoke();
                    Mask.GetComponent<SpriteRenderer>().DOColor(new Color(0, 0, 0, 100), 1f)
                    .OnComplete(() =>
                    {
                        GameManager.Instance.PerformControl.SetAwaitEnd();
                        Mask.SetActiveEx(false);
                    });

                });
            }
            else
            {
                curScene?.gameObject.SetActive(false);
                curScene = scene;
                scene.gameObject.SetActive(true);
                scene.RefreshView();
                call?.Invoke();
                Mask.GetComponent<SpriteRenderer>().DOColor(new Color(0, 0, 0, 100), 1.2f)
                    .OnComplete(() =>
                    {
                        GameManager.Instance.PerformControl.SetAwaitEnd();
                        Mask.SetActiveEx(false);
                    });
            }
        }
    }

    public void OnPlayAnimator(string name)
    {
        foreach (var item in curScene.Others)
        {
            item.SetActiveEx(true);
        }

        animator.enabled = true;
        animator.Play(name, 0, 0f);
    }

    public float AnimatorLength()
    {
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }

    public void PerformOver()
    {
        animator.enabled = false;
        foreach (var item in curScene.Others)
        {
            item.SetActiveEx(false);
        }
        _camera.transform.DOLocalMove(new Vector3(0, 0, -10), 0.5f);
        _camera.DOFieldOfView(80, 0.5f);
    }


    public void OnBuilding(object obj = null)
    {
        var buildId = (int)obj;
        var cfg = Config.GetConfig<Config_BuildBase>().GetConfigById(buildId);
        if (!string.IsNullOrEmpty(cfg.CameraSet) && !string .IsNullOrEmpty(cfg.CameraZoom))
        {
            var posXY = cfg.CameraSet.Split("#");
            var end = new Vector3(float.Parse(posXY[0]), float.Parse(posXY[1]), float.Parse(cfg.CameraZoom));
            _camera.transform.DOLocalMove(end,1f)
                .OnComplete(() =>
                {
                    curScene?.OnBuilding(buildId);
                    DOVirtual.DelayedCall(1f, BuildEnd);
                });
        }
        else
        {
            curScene?.OnBuilding(buildId);
            DOVirtual.DelayedCall(1f, BuildEnd);
        }
    }

    public void BuildEnd()
    {
        _camera.transform.DOLocalMove(new Vector3(0,0,-10), 0.5f);
        _camera.DOFieldOfView(80, 0.5f);
    }

}
