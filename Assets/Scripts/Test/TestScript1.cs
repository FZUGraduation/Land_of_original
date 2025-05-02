using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestScript1 : MonoBehaviour
{
    public AudioClip[] FootstepAudioClips;
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.position, 0.5f);
            }
        }
    }

}
