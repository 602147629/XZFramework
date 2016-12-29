/*
 * FileName:    SoundManager
 * Author:      熊哲
 * CreateTime:  11/23/2016 4:22:45 PM
 * Description:
 * 
*/
using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace XZFramework
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : ManagerTemplate<SoundManager>
    {
        private AudioSource audioSource { get { return GetComponent<AudioSource>(); } }
        public float SpatialBlend
        {
            get { return audioSource.spatialBlend; }
            set { audioSource.spatialBlend = value; }
        }

        private bool _BgmActive;
        public bool BgmActive
        {
            get { return _BgmActive; }
            set
            {
                _BgmActive = value;
                audioSource.mute = !value;
                PlayerPrefs.SetInt("BgmActive", value ? 1 : 0);
            }
        }
        private float _BgmVolume;
        public float BgmVolume
        {
            get { return _BgmVolume; }
            set
            {
                _BgmVolume = value;
                audioSource.volume = value;
                PlayerPrefs.SetFloat("BgmVolume", value);
            }
        }
        private bool _EfxActive;
        public bool EfxActive
        {
            get { return _EfxActive; }
            set
            {
                _EfxActive = value;
                PlayerPrefs.SetInt("EfxActive", value ? 1 : 0);
            }
        }
        private float _EfxVolume;
        public float EfxVolume
        {
            get { return _EfxVolume; }
            set
            {
                _EfxVolume = value;
                PlayerPrefs.SetFloat("EfxVolume", value);
            }
        }

        /// <summary>
        /// 初始化读取玩家音效选项
        /// </summary>
        public override void Initialize()
        {
            BgmActive = PlayerPrefs.GetInt("IsBgmActive", 1) == 1;
            EfxActive = PlayerPrefs.GetInt("IsEfxAcitve", 1) == 1;
            BgmVolume = PlayerPrefs.GetFloat("BgmVolume", 1);
            EfxVolume = PlayerPrefs.GetFloat("EfxVolume", 1);
        }

        /// <summary>
        /// 播放或更换背景音乐，指定是否循环，淡出时间
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="loop"></param>
        /// <param name="fadeOutTime"></param>
        public void PlayBgm(AudioClip audio, bool loop = true, float fadeOutTime = 0.2f)
        {
            audioSource.DOFade(0, fadeOutTime).OnComplete(delegate
            {
                audioSource.clip = audio;
                audioSource.loop = loop;
                audioSource.Play();
            });
        }
        /// <summary>
        /// 停止播放背景音乐，指定淡出时间
        /// </summary>
        /// <param name="fadeOut"></param>
        public void StopBgm(float fadeOutTime = 0.2f)
        {
            audioSource.DOFade(0, fadeOutTime).OnComplete(delegate
            {
                audioSource.Stop();
                audioSource.volume = BgmVolume;
            });
        }
        /// <summary>
        /// 播放2D音效
        /// </summary>
        /// <param name="soundName"></param>
        public void PlayEfx(AudioClip audio)
        {
            if (EfxActive)
            {
                audioSource.PlayOneShot(audio, EfxVolume);
            }
        }
        /// <summary>
        /// 在指定地点播放3D音效
        /// </summary>
        /// <param name="soundName"></param>
        /// <param name="position"></param>
        public void PlayEfx(AudioClip audio, Vector3 position)
        {
            if (EfxActive)
            {
                AudioSource.PlayClipAtPoint(audio, position, EfxVolume);
            }
        }

        /// <summary>
        /// 在物体上附加音效并返回（不提供在多音效物体中获取指定音效的方法，多音效物体请保存返回值）
        /// </summary>
        /// <param name="go"></param>
        /// <param name="audio"></param>
        /// <returns></returns>
        public AudioSource AddGameObjectEfx(GameObject go, AudioClip audio)
        {
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.spatialBlend = SpatialBlend;
            audioSource.clip = audio;
            return audioSource;
        }
        /// <summary>
        /// 改变物体上附加的音效（单音效或首个音效）
        /// </summary>
        /// <param name="go"></param>
        /// <param name="soundName"></param>
        /// <param name="volume"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public AudioSource SetGameObjectEfx(GameObject go, AudioClip audio)
        {
            AudioSource audioSource = go.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                return AddGameObjectEfx(go, audio);
            }
            audioSource.clip = audio;
            return audioSource;
        }
        /// <summary>
        /// 得到物体上附加的音效（单音效或首个音效）
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public AudioSource GetGameObjectEfx(GameObject go)
        {
            return go.GetComponent<AudioSource>();
        }
        /// <summary>
        /// 移除物体上附加的音效
        /// </summary>
        /// <param name="go"></param>
        public void RemoveGameObjectEfx(GameObject go)
        {
            AudioSource[] audios = go.GetComponents<AudioSource>();
            for (int i = 0; i < audios.Length; i++)
            {
                Destroy(audios[i]);
            }
        }

        /// <summary>
        /// 播放物体上附加的音效（单音效或首个音效）
        /// </summary>
        /// <param name="go"></param>
        public void PlayGameObjectEfx(GameObject go)
        {
            if (EfxActive)
            {
                AudioSource audioSource = GetGameObjectEfx(go);
                if (audioSource == null) return;
                audioSource.volume = EfxVolume;
                audioSource.Play();
            }
        }
        /// <summary>
        /// 在物体上播放指定音效
        /// </summary>
        /// <param name="go"></param>
        /// <param name="audio"></param>
        public void PlayGameObjectEfx(GameObject go, AudioClip audio)
        {
            if (EfxActive)
            {
                AudioSource audioSource = SetGameObjectEfx(go, audio);
                audioSource.volume = EfxVolume;
                audioSource.Play();
            }
        }

#if USING_LUA
        public void PlayBgm(string bundleName, string assetName, bool loop, float fadeOutTime)
        {
            AudioClip audio = ResourceManager.Instance.LoadAsset<AudioClip>(bundleName, assetName);
            PlayBgm(audio, loop, fadeOutTime);
        }
        public void PlayEfx(string bundleName, string assetName)
        {
            AudioClip audio = ResourceManager.Instance.LoadAsset<AudioClip>(bundleName, assetName);
            PlayEfx(audio);
        }
        public void PlayEfx(string bundleName, string assetName, Vector3 position)
        {
            AudioClip audio = ResourceManager.Instance.LoadAsset<AudioClip>(bundleName, assetName);
            PlayEfx(audio, position);
        }
#endif

        #region 以下方法不建议使用
        /// <summary>
        /// 在指定地点播放临时音效，并指定音量大小（生成临时物体播放音效后销毁，该方法会产生很多垃圾）
        /// </summary>
        /// <param name="soundName"></param>
        /// <param name="position"></param>
        /// <param name="volume"></param>
        public void PlayTempEfx(AudioClip audio, Vector3 position, float volume)
        {
            if (EfxActive)
            {
                StartCoroutine(TempEfxGameObject(audio, position, volume));
            }
        }
        /// <summary>
        /// 生成临时物体播放音效后销毁
        /// </summary>
        /// <param name="position"></param>
        /// <param name="soundName"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        protected IEnumerator TempEfxGameObject(AudioClip audio, Vector3 position, float volume)
        {
            GameObject go = new GameObject("SoundEfx"); go.transform.position = position;
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.spatialBlend = SpatialBlend;
            audioSource.clip = audio;
            audioSource.volume = volume;
            audioSource.loop = false;
            audioSource.Play();
            while (audioSource.isPlaying)
                yield return null;
            Destroy(go);
        }
        /// <summary>
        /// 在物体上播放一个临时的音效（生成临时音效播放后销毁，该方法会产生很多垃圾）
        /// </summary>
        /// <param name="go"></param>
        /// <param name="soundName"></param>
        /// <param name="volume"></param>
        public void PlayTempEfx(GameObject go, AudioClip audio, float volume)
        {
            if (EfxActive)
            {
                StartCoroutine(GameObjectTempEfx(go, audio, volume));
            }
        }
        /// <summary>
        /// 在物体上生成临时的音效播放后销毁
        /// </summary>
        /// <param name="go"></param>
        /// <param name="soundName"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        protected IEnumerator GameObjectTempEfx(GameObject go, AudioClip audio, float volume)
        {
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.spatialBlend = SpatialBlend;
            audioSource.clip = audio;
            audioSource.volume = volume;
            audioSource.loop = false;
            audioSource.Play();
            while (audioSource.isPlaying)
                yield return null;
            Destroy(audioSource);
        }
        #endregion
    }
}