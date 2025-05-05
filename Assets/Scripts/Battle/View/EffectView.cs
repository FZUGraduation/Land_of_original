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
            transform.position = transform.position + (targetPosition - transform.position) / 3; // 设置子弹位置为起始位置和目标位置的中点
            // 使用 DOTween 实现子弹移动逻辑
            transform.DOMove(targetPosition, Vector3.Distance(transform.position, targetPosition) / 15f)
                .SetEase(Ease.InOutSine)//
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
