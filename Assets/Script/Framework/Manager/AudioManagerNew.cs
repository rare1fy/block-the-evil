using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Framework;
using UnityEngine;

public class AudioManagerNew : MonoSingleton<AudioManagerNew>
{
    //是否在后台
    public bool IsInBackground = false;
    
    [Header("音乐/音效音量")]
    public float MusicVolume = 1f;
    public float VolumeBlend = 1f;

    [HideInInspector]
    public float FadeDuration = 1f; //背景音乐淡出时长
    [HideInInspector]
    public int MaxEffectSources = 10; //音效槽位数量

    private AudioSource _musicSource;
    private AudioSource[] _effectSources;
    private AudioLowPassFilter _lowPassFilter;

    private Dictionary<string, List<AudioSource>> _playingSounds = new Dictionary<string, List<AudioSource>>();

    private void Awake()
    {
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;

        _lowPassFilter = gameObject.GetOrAddComponent<AudioLowPassFilter>();
        _lowPassFilter.enabled = false;

        _effectSources = new AudioSource[MaxEffectSources];
        for (int i = 0; i < MaxEffectSources; i++)
        {
            _effectSources[i] = gameObject.AddComponent<AudioSource>();
            _effectSources[i].playOnAwake = false;
        }
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_LOAD_PLAYERDATA_FINISH, SetVolumeBlend);
    }
    
    private void SetVolumeBlend(object obj)
    {
        VolumeBlend = PlayerDataManager.instance.PlayerData.VolumeBlend;
        ChangeVolumeBlend(VolumeBlend);
    }

    #region 背景音乐
    public void PlayMusic(string clipName, float volume = 1f)
    {
        if(IsInBackground)
            return;
        
        LoadClip(clipName, clip =>
        {
            if (clip == null) return;
            _musicSource.clip = clip;
            _musicSource.volume = volume * MusicVolume * VolumeBlend ;
            _musicSource.loop = true;
            _musicSource.Play();
        });
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        _musicSource.clip = null;
    }

    public void PauseMusic() => _musicSource.Pause();
    public void ResumeMusic() => _musicSource.Play();

    public void FadeStopMusic(Action onComplete = null)
    {
        StartCoroutine(FadeOutCoroutine(onComplete));
    }

    private IEnumerator FadeOutCoroutine(Action onComplete)
    {
        float startVolume = _musicSource.volume;
        float time = 0f;

        while (time < FadeDuration)
        {
            _musicSource.volume = Mathf.Lerp(startVolume, 0, time / FadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        _musicSource.volume = 0;
        StopMusic();
        onComplete?.Invoke();
    }
    
    #endregion

    #region 音效
    
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="clipName"></param>
    /// <param name="volume"></param>
    /// <param name="allowOverlap">是否可以叠加播放</param>
    public void PlayAudio(string clipName, float volume = 1f, bool allowOverlap = false)
    {
        if(IsInBackground)
            return;
        
        LoadClip(clipName, clip =>
        {
            if (clip == null) return;

            string simpleName = Path.GetFileNameWithoutExtension(clipName);

            if (!allowOverlap && _playingSounds.ContainsKey(simpleName) && _playingSounds[simpleName].Count > 0)
                return;

            var source = GetAvailableSource();
            source.clip = clip;
            source.loop = false;
            source.volume = volume * VolumeBlend;
            source.Play();

            if (!_playingSounds.ContainsKey(simpleName))
                _playingSounds[simpleName] = new List<AudioSource>();
            _playingSounds[simpleName].Add(source);

            StartCoroutine(ReleaseWhenFinish(source, simpleName));
        });
    }

    public void StopAudio(string clipName)
    {
        string simpleName = Path.GetFileNameWithoutExtension(clipName);

        if (_playingSounds.TryGetValue(simpleName, out var sources))
        {
            foreach (var src in sources)
            {
                if (src != null)
                {
                    src.Stop();
                    src.clip = null;
                }
            }
            _playingSounds.Remove(simpleName);
        }
    }

    public void StopAllAudio(bool includeMusic = false)
    {
        foreach (var src in _effectSources)
        {
            src.Stop();
            src.clip = null;
        }

        _playingSounds.Clear();

        if (includeMusic)
            StopMusic();
    }

    public void HideAudio()
    {
        foreach (var src in _effectSources)
        {
            src.Stop();
            src.clip = null;
        }
        _playingSounds.Clear();

        PauseMusic();
    }


    #endregion

    #region 工具
    private AudioSource GetAvailableSource()
    {
        foreach (var src in _effectSources)
        {
            if (!src.isPlaying)
                return src;
        }
        
        return _effectSources[^1];
    }

    private void LoadClip(string clipName, Action<AudioClip> callback)
    {
        if (string.IsNullOrEmpty(clipName))
        {
            callback?.Invoke(null);
            return;
        }

        ResourceManagerNew.instance.LoadAssetAsync(clipName, (AudioClip clip) =>
        {
            callback?.Invoke(clip);
        });
    }

    private IEnumerator ReleaseWhenFinish(AudioSource source, string clipName)
    {
        yield return new WaitUntil(() => !source.isPlaying);

        if (_playingSounds.ContainsKey(clipName))
        {
            _playingSounds[clipName].Remove(source);
            if (_playingSounds[clipName].Count == 0)
                _playingSounds.Remove(clipName);
        }

        source.clip = null;
    }

    public void ChangeVolumeBlend(float value)
    {
        VolumeBlend = Mathf.Max(0f, value);

        if (_musicSource != null && _musicSource.clip != null)
            _musicSource.volume = MusicVolume * VolumeBlend;

        foreach (var kvp in _playingSounds)
        {
            foreach (var src in kvp.Value)
            {
                if (src != null && src.isPlaying)
                    src.volume = VolumeBlend;
            }
        }
    }

    public void SetPassFilter(bool enable)
    {
        _lowPassFilter.enabled = enable;
    }
    #endregion

    private void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_LOAD_PLAYERDATA_FINISH, SetVolumeBlend);
        StopAllCoroutines();
        StopAllAudio(true);

        _playingSounds.Clear();
        _musicSource = null;
        _effectSources = null;
        _lowPassFilter = null;
    }
}
