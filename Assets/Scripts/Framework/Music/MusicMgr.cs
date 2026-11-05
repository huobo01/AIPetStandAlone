using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 音乐音效管理器
/// 管理背景音乐（循环）和音效（一次性/循环），支持音量调节、暂停、缓存池复用
/// </summary>
public class MusicMgr : BaseManager<MusicMgr>
{
    //背景音乐播放组件
    private AudioSource bkMusic;
    //背景音乐音量
    private float bkMusicValue = 0.1f;
    //正在播放的音效列表
    private List<AudioSource> soundList = new List<AudioSource>();
    //音效音量
    private float soundValue = 0.1f;
    //音效是否播放
    private bool soundIsPlay = true;

    private MusicMgr()
    {
        MonoMgr.Instance.AddFixedUpdateListener(Update);
    }

    private void Update()
    {
        if (!soundIsPlay) return;

        //逆向遍历，移除播放完毕的音效并回收到对象池
        for (int i = soundList.Count - 1; i >= 0; --i)
        {
            if (!soundList[i].isPlaying)
            {
                soundList[i].clip = null;
                PoolMgr.Instance.PushObj(soundList[i].gameObject);
                soundList.RemoveAt(i);
            }
        }
    }

    /// <summary>播放背景音乐</summary>
    public void PlayBKMusic(string name)
    {
        if (bkMusic == null)
        {
            GameObject obj = new GameObject("BKMusic");
            GameObject.DontDestroyOnLoad(obj);
            bkMusic = obj.AddComponent<AudioSource>();
        }

        ABResMgr.Instance.LoadResAsync<AudioClip>("music", name, (clip) =>
        {
            bkMusic.clip = clip;
            bkMusic.loop = true;
            bkMusic.volume = bkMusicValue;
            bkMusic.Play();
        });
    }

    /// <summary>停止背景音乐</summary>
    public void StopBKMusic()
    {
        if (bkMusic != null)
            bkMusic.Stop();
    }

    /// <summary>暂停背景音乐</summary>
    public void PauseBKMusic()
    {
        if (bkMusic != null)
            bkMusic.Pause();
    }

    /// <summary>设置背景音乐音量</summary>
    public void ChangeBKMusicValue(float v)
    {
        bkMusicValue = v;
        if (bkMusic != null)
            bkMusic.volume = bkMusicValue;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="name">音效名</param>
    /// <param name="isLoop">是否循环</param>
    /// <param name="isSync">是否同步加载</param>
    /// <param name="callBack">加载完成回调</param>
    public void PlaySound(string name, bool isLoop = false, bool isSync = false, UnityAction<AudioSource> callBack = null)
    {
        ABResMgr.Instance.LoadResAsync<AudioClip>("sound", name, (clip) =>
        {
            AudioSource source = PoolMgr.Instance.GetObj("Sound/soundObj").GetComponent<AudioSource>();
            source.Stop();
            source.clip = clip;
            source.loop = isLoop;
            source.volume = soundValue;
            source.Play();
            //从池中取出的对象可能是之前正在使用的，避免重复添加
            if (!soundList.Contains(source))
                soundList.Add(source);
            callBack?.Invoke(source);
        }, isSync);
    }

    /// <summary>停止指定音效</summary>
    public void StopSound(AudioSource source)
    {
        if (soundList.Contains(source))
        {
            source.Stop();
            soundList.Remove(source);
            source.clip = null;
            PoolMgr.Instance.PushObj(source.gameObject);
        }
    }

    /// <summary>设置所有音效音量</summary>
    public void ChangeSoundValue(float v)
    {
        soundValue = v;
        for (int i = 0; i < soundList.Count; i++)
            soundList[i].volume = v;
    }

    /// <summary>继续播放或暂停所有音效</summary>
    public void PlayOrPauseSound(bool isPlay)
    {
        soundIsPlay = isPlay;
        for (int i = 0; i < soundList.Count; i++)
        {
            if (isPlay)
                soundList[i].Play();
            else
                soundList[i].Pause();
        }
    }

    /// <summary>
    /// 清空所有音效记录
    /// 重要：切场景时必须在清空缓存池之前调用此方法
    /// </summary>
    public void ClearSound()
    {
        for (int i = 0; i < soundList.Count; i++)
        {
            soundList[i].Stop();
            soundList[i].clip = null;
            PoolMgr.Instance.PushObj(soundList[i].gameObject);
        }
        soundList.Clear();
    }
}
