using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EffectView : MonoBehaviour
{
    public SkillEffectViewData info; // 技能特效预设体
    private Vector3 targetPosition; // 目标位置
    void Start()
    {
        if (info == null)
        {
            Debug.LogError("EffectViewInfo is null, please set it in the inspector.");
            return;
        }
        GameObject effectInstance = Instantiate(info.effectViewPrefab, transform);
        effectInstance.transform.position += info.offset; // 设置特效位置偏移
        if (info.effectViewType == EffectViewType.Particle)
        {
            transform.position = targetPosition;
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
        else if (info.effectViewType == EffectViewType.Bullet)
        {
            // 使用 DOTween 实现子弹移动逻辑
            transform.DOMove(targetPosition, Vector3.Distance(transform.position, targetPosition) / 20f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    Destroy(gameObject); // 移动完成后销毁对象
                });
        }
    }

    public void SetInfo(SkillEffectViewData info, Vector3 position)
    {
        this.info = info;
        targetPosition = position;
    }
}
