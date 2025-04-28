using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectView : MonoBehaviour
{
    public SkillEffectViewData info; // 技能特效预设体
    void Start()
    {
        if (info == null)
        {
            Debug.LogError("EffectViewInfo is null, please set it in the inspector.");
            return;
        }
        GameObject effectInstance = Instantiate(info.effectViewPrefab, transform);
        if (info.effectViewType == EffectViewType.Particle)
        {
            var particleSystem = effectInstance.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                Debug.LogError("ParticleSystem component not found on this GameObject.");
                return;
            }
            // 设置粒子系统播放完成后自动销毁
            var main = particleSystem.main;
            if (info.destoryOnEnd == true)
            {
                main.stopAction = ParticleSystemStopAction.Destroy;
            }
            particleSystem.Play();
        }
    }
    public void SetInfo(SkillEffectViewData info, Vector3 position)
    {
        this.info = info;
        transform.position = position;
    }
}
