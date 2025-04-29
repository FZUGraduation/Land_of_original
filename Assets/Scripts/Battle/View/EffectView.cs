using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectView : MonoBehaviour
{
    public SkillEffectViewData info; // 技能特效预设体
    public Vector3 targetPosition; // 目标位置
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

        }
    }
    void Update()
    {
        if (info.effectViewType == EffectViewType.Bullet)
        {
            // 这里可以添加子弹的移动逻辑
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 5f);
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                Destroy(gameObject);
            }
        }
    }
    public void SetInfo(SkillEffectViewData info, Vector3 position)
    {
        this.info = info;
        targetPosition = position;
    }
}
