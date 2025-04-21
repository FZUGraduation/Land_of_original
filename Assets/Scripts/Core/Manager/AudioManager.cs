using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using DG.Tweening;
public enum AudioType
{
    SE,
    BGM,
    Voice
}
public class AudioManager : SingletonMono<AudioManager>
{
    //Start is called before the first frame update
    private readonly AudioSource[] BGMSource = new AudioSource[4];
    private readonly AudioSource[] SESources = new AudioSource[2];
    private AudioSource VoiceSource;
    private string playingBgmName = null;
    private AudioMixer mixer;
    private AudioMixerGroup[] masterGroup;
    private const int SAMPLE_SIZE = 128;
    private float[] samples = new float[SAMPLE_SIZE];

    private int activeBgmIndex = 0;

    //private analyser AnalyserNode ;
    //private fadeCo: Coroutine<void> = null;
    //private fadeAttenuation: number  =1.0;
    //private isDucking : boolean = false;
    //private dataArray: Uint8Array;
    // private IEnumerator Fadeco = null;


    public void MuteBGM(bool mute)
    {

    }

    protected override void Awake()
    {
        base.Awake();
        mixer = Resources.Load<AudioMixer>("Audio/Mixer");
        masterGroup = mixer.FindMatchingGroups("Master");
        for (int i = 0; i < BGMSource.Length; ++i)
        {
            BGMSource[i] = gameObject.AddComponent<AudioSource>();
            BGMSource[i].outputAudioMixerGroup = masterGroup[1];
            BGMSource[i].playOnAwake = false;
        }

        activeBgmIndex = 0;

        for (int i = 0; i < SESources.Length; ++i)
        {
            SESources[i] = gameObject.AddComponent<AudioSource>();
            SESources[i].loop = false;
            SESources[i].playOnAwake = false;
            SESources[i].outputAudioMixerGroup = masterGroup[2];
        }
        VoiceSource = gameObject.AddComponent<AudioSource>();

        //BGMSource[activeBgmIndex].loop = true;

        VoiceSource.loop = false;
        VoiceSource.outputAudioMixerGroup = masterGroup[3];

        // PlayBGM("Time");
    }
    //public float GetPlayingBGMLength() {
    //    if (!(BGMSource[activeBgmIndex].clip == null || BGMSource == null))
    //        return BGMSource[activeBgmIndex].time;
    //    else
    //        return 0;
    //}
    //public float GetPlayingBGMRemainTime() {
    //    if (!(BGMSource[activeBgmIndex].clip == null || BGMSource == null))
    //        return BGMSource[activeBgmIndex].clip.length - BGMSource[activeBgmIndex].time;
    //    else
    //        return 0;
    //}
    //bool BGMIsPlaying {
    //    get { return BGMSource[activeBgmIndex].isPlaying; }
    //}
    public IEnumerator Prepare()
    {
        yield return null;
    }

    public void PlaySE(string file, int channel = 0, bool loop = false, float pitch = 1.0f, float volume = 1.0f)//两个channel轮流播放
    {
        AudioClip clip = LoadAudioClip(AudioType.SE, file);
        if (clip != null)
        {
            var src = SESources[channel];
            src.clip = clip;
            src.loop = loop;
            src.volume = volume;
            src.Play();
            src.pitch = pitch;
        }
    }

    public void PlaySEOneShot(string file, int channel = 0, bool loop = false)
    {
        AudioClip clip = LoadAudioClip(AudioType.SE, file);
        if (clip != null)
        {
            var src = SESources[channel];
            src.loop = loop;
            src.PlayOneShot(clip);
        }
    }

    public void PlayBGM(string file, float fadein = 1.5f, bool loop = true)
    {
        if (file != null && file == playingBgmName)
            return;
        if (string.IsNullOrEmpty(file))
        {
            StopBGM(fadein);
            return;
        }
        AudioClip intro = null;
        AudioClip main = null;
        var nextSources = new AudioSource[2];
        var stopSources = new AudioSource[2];
        var newIndex = activeBgmIndex == 2 ? 0 : 2;
        nextSources[0] = BGMSource[newIndex];
        nextSources[1] = BGMSource[newIndex + 1];
        stopSources[0] = BGMSource[activeBgmIndex];
        stopSources[1] = BGMSource[activeBgmIndex + 1];
        var hasIntro = false;
        //并根据是否有 intro 设置音量和播放时间。
        //如果有 intro，则先播放 intro，然后在 intro 播放完毕后播放 main。如果没有 intro，则直接播放 main 并淡入音量。
        if (Datalib.Instance.bgmConfig.ContainsKey(file))
        {
            var introFile = Datalib.Instance.bgmConfig[file].intro;
            var mainFile = Datalib.Instance.bgmConfig[file].main;
            intro = LoadAudioClip(AudioType.BGM, introFile);
            main = LoadAudioClip(AudioType.BGM, mainFile);
            if (intro == false || main == false)
            {
                return;
            }
            hasIntro = true;
            nextSources[0].clip = intro;
            nextSources[1].clip = main;
        }
        else
        {
            hasIntro = false;
            main = LoadAudioClip(AudioType.BGM, file);
            if (main == null)
            {
                return;
            }
            nextSources[1].clip = main;
        }
        stopSources[0].DOKill();
        stopSources[0].DOFade(0f, fadein).OnComplete(stopSources[0].Stop);
        stopSources[1].DOKill();
        stopSources[1].DOFade(0f, fadein).OnComplete(stopSources[1].Stop);

        nextSources[0].DOKill();
        nextSources[1].DOKill();
        nextSources[0].loop = false;
        nextSources[1].loop = loop;
        if (hasIntro)
        {
            nextSources[0].volume = 0f;
            nextSources[1].volume = 1f;
            var dspTime = 0.1 + AudioSettings.dspTime;
            nextSources[0].PlayScheduled(dspTime);
            nextSources[0].DOFade(1f, fadein);
            nextSources[1].PlayScheduled(dspTime + nextSources[0].clip.length);
        }
        else
        {
            nextSources[0].volume = 0f;
            nextSources[1].volume = 0f;
            nextSources[1].Play();
            nextSources[1].DOFade(1f, fadein);
        }

        activeBgmIndex = newIndex;
        playingBgmName = file;
    }

    public void PlayVoice(string file, bool loop = false)
    {
        // if (GlobalRuntimeData.Instance.DubbingLanguage == LanguageSupported.JP)
        // {
        //     file = file + "_JP";
        // }
        var clip = LoadAudioClip(AudioType.Voice, file);
        //var volume = GlobalData.Instance.VoiceVolum;
        if (clip != null)
        {
            VoiceSource.clip = clip;
            //VoiceSource.volume = volume;
            VoiceSource.loop = loop;
            VoiceSource.Play();
        }
    }

    public void StopSE(int channel, float fadeout)
    {
        var src = SESources[channel];
        src.Stop();
    }

    public void PauseBGM(float fadeout)
    {
        for (int i = 0; i < 2; i++)
        {
            var sourceToPause = BGMSource[activeBgmIndex + i];
            if (fadeout > 0)
            {
                if (sourceToPause.isPlaying)
                {
                    sourceToPause.DOKill();
                    sourceToPause.DOFade(0f, fadeout).OnComplete(sourceToPause.Pause);
                }
            }
            else
            {
                sourceToPause.Pause();
            }
        }
    }
    public void UnpauseBGM(float fadein)
    {
        for (int i = 0; i < 2; i++)
        {
            var sourceToUnpause = BGMSource[activeBgmIndex + i];
            if (sourceToUnpause.clip != null)
            {
                sourceToUnpause.UnPause();
                if (fadein > 0)
                {
                    sourceToUnpause.volume = 0;
                    sourceToUnpause.DOFade(1, fadein);
                }
                else
                {
                    sourceToUnpause.volume = 1;
                }
            }
        }
    }

    public void StopBGM(float fadeout)
    {
        for (int i = 0; i < 2; i++)
        {
            var sourceToStop = BGMSource[activeBgmIndex + i];
            if (!sourceToStop.isPlaying)
            {
                continue;
            }
            if (fadeout > 0)
            {
                sourceToStop.DOKill();
                sourceToStop.DOFade(0, fadeout).OnComplete(sourceToStop.Stop);
            }
            else
            {
                sourceToStop.Stop();
            }
        }
        playingBgmName = null;
    }

    public void SetVolumeBGM(float vol)
    {
        GlobalRuntimeData.Instance.bgmVolum = vol;
        masterGroup[1].audioMixer.SetFloat("BGMVolume", Mathf.Log10(vol) * 20);
    }
    public void SetVolumeSE(float vol)
    {
        GlobalRuntimeData.Instance.seVolum = vol;
        masterGroup[2].audioMixer.SetFloat("SEVolume", Mathf.Log10(vol) * 20);
    }

    public void SetVolumeVoice(float vol)
    {
        GlobalRuntimeData.Instance.voiceVolum = vol;
        masterGroup[3].audioMixer.SetFloat("VoiceVolume", Mathf.Log10(vol) * 20);
    }
    public float GetBusAnalyserPeak()
    {
        if (VoiceSource.isPlaying)
        {
            int pos = VoiceSource.timeSamples;
            float sum = 0;

            VoiceSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                float wavePeak = samples[i];
                //Debug.Log(wavePeak);
                sum += wavePeak;
                //if (wavePeak>peak) {
                //    peak = wavePeak;
                //}
            }
            return sum / SAMPLE_SIZE;
        }
        return 0;
    }
    //获取音频长度(ms)
    public float GetPlayingVoiceLength()
    {
        if (VoiceSource.clip != null)
            return VoiceSource.clip.length;
        else
        {
            return -1.0f;
        }
    }

    //获取音频播放的剩余时间（s）
    public float GetPlayingVoiceRemainTime()
    {
        if (VoiceSource.clip != null)
            return VoiceSource.clip.length - VoiceSource.time;
        else
        {
            return -1.0f;
        }

    }

    public void StopVoice()
    {
        VoiceSource.Stop();
    }
    public bool IsPlayingVoice()
    {
        return VoiceSource.isPlaying;
    }
    //public string GetPlayingBGMName() {
    //    if (BGMSource[activeBgmIndex].isPlaying)
    //        return playingBgmName;
    //    else {
    //        return null;
    //    }
    //}

    public static AudioClip LoadAudioClip(AudioType type, string name)
    {

        string abName = "";

        if (type == AudioType.BGM)
        {
            abName = "Audio/BGM";

        }
        else if (type == AudioType.SE)
        {
            abName = "Audio/SE";
        }
        else if (type == AudioType.Voice)
        {
            abName = "Audio/Voice";
        }

        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        string filePath = string.Format("{0}/{1}", abName, name);
        return Resources.Load<AudioClip>(filePath);
    }
}