using System.Collections;
using Cinemachine;
using UnityEngine;

public class UICameraShake
{
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin perlin;
    
    public UICameraShake(GameObject vCamera)
    {
        virtualCamera = vCamera.GetComponent<CinemachineVirtualCamera>();
        perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }
    
    public float Amplitude = 0.5f;         //相机震幅
    public float Frequency = 1f;           //相机震频
    public float CameraDuration = 0.2f;    //相机震动持续时间
    public float CSDelay = 0f;             //震屏触发 延迟
    
    
    public IEnumerator DoShake()
    {
        var elapsed = 0f;
        perlin.m_AmplitudeGain = Amplitude;
        perlin.m_FrequencyGain = Frequency;

        while (elapsed < CameraDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 恢复默认值
        perlin.m_AmplitudeGain = 0;
        perlin.m_FrequencyGain = 0;
    }
    
}
