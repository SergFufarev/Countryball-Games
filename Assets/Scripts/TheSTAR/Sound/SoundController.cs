using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunnyBlox;

namespace TheSTAR.Sound
{
    public sealed class SoundController : MonoBehaviour, IController
    {
        public static SoundController Instance;

        #region Fields
        [SerializeField] private SoundConfig soundConfig;

        private List<CreatedSound> createdSounds = new ();
        private List<CreatedMusic> createdMusics = new ();

        private CreatedMusic currentMusic;

        private static float _timeClear = 1;
        private static float _timeMusicChange = 1;
        public static float TimeMusicChange => _timeMusicChange;

        private int currentMusicTransitionID;
        private MusicType previousMusicType;


        public CreatedMusic CurrentMusic => currentMusic;
        public MusicType PreviousMusicType => previousMusicType;

        #endregion // Fields

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _timeClear = soundConfig.TimeClear;
            _timeMusicChange = soundConfig.TimeMusicChange;
            StartCoroutine(ClearSources());
        }

        public void PlayMusic(
            MusicType type, 
            MusicChangeType mutePreviousMusicType = MusicChangeType.None, 
            MusicChangeType startNextMusicType = MusicChangeType.None)
        {
            if (!CommonData.MusicOn) return;

            var data = soundConfig.GetData(type);

            if (data == null) return;

            Action createNextMusicAction = () =>
            {
                var obj = new GameObject(type.ToString());
                obj.transform.parent = transform;
                var source = obj.AddComponent<AudioSource>();

                source.clip = data.Clip;
                source.volume = data.Volume;
                source.loop = data.Loop;
                source.Play();

                var createdMusic = obj.AddComponent<CreatedMusic>();
                createdMusic.Init(source, data.Type, source.volume);
                createdMusics.Add(createdMusic);
                currentMusic = createdMusic;

                currentMusicTransitionID = -1;
            };

            if (currentMusicTransitionID != -1)
            {
                LeanTween.cancel(currentMusicTransitionID, true);
                
                currentMusicTransitionID = -1;
            }

            if (createdMusics.Count > 0)
            {
                // ищем такую же музыку уже созданную
                for (int i = 0; i < createdMusics.Count; i++)
                {
                    var music = createdMusics[i];

                    if (music != null && music.Type == type)
                    {
                        if (music.IsPlaying) return;
                        else
                        {
                            // пытаемся запустить (если сперва должно произойти затухание предыдущей музыки, то передаём логику как экшен)
                        }
                    }
                }

                for (int i = 0; i < createdMusics.Count; i++)
                {
                    var music = createdMusics[i];

                    if (music != null && music.IsPlaying)
                    {
                        if (music == currentMusic)
                        {
                            music.Stop(mutePreviousMusicType, out currentMusicTransitionID, createNextMusicAction);
                        }
                        else
                        {
                            music.Stop(MusicChangeType.None, out int id);
                        }
                        
                        return;
                    }
                }
            }

            createNextMusicAction.Invoke();

            previousMusicType = type;
        }

        public SoundConfig.MusicData GetMusicData(MusicType musicType)
        {
            return soundConfig.GetData(musicType);
        }

        public void PlaySound(SoundType type, float customPitch = 1)
        {
            if (!CommonData.SoundsOn) return;

            var data = soundConfig.GetData(type);

            if (data == null) return;

            if (!data.CanMultiply)
            {
                // ищем такой же звук среди созданных и запускаем повторно

                for (int i = 0; i < createdSounds.Count; i++)
                {
                    if (createdSounds[i].Type == type)
                    {
                        createdSounds[i].Replay(customPitch);
                        return;
                    }
                }
            }

            var obj = new GameObject(type.ToString());
            obj.transform.parent = transform;

            var source = obj.AddComponent<AudioSource>();

            source.clip = data.Clip;
            source.volume = data.Volume;
            source.loop = data.Loop;
            source.pitch = customPitch;
            source.Play();

            var createdSound = obj.AddComponent<CreatedSound>();
            createdSound.Init(source, data.Type, source.volume);
            createdSounds.Add(createdSound);
        }

        public void Stop(MusicType type, MusicChangeType mutePreviousMusicType = MusicChangeType.None)
        {
            for (int i = 0; i < createdMusics.Count; i++)
            {
                var music = createdMusics[i];

                if (music != null) music.Stop(mutePreviousMusicType, out currentMusicTransitionID);
            }
        }

        public void Stop(SoundType type)
        {
            for (int i = 0; i < createdSounds.Count; i++)
            {
                var sound = createdSounds[i];

                if (sound != null) sound.Stop();
            }
        }

        public void StopMusic(MusicChangeType stopType = MusicChangeType.None)
        {
            if (currentMusic != null) currentMusic.Stop(stopType, out currentMusicTransitionID);
        }

        private IEnumerator ClearSources()
        {
            while (gameObject.activeSelf)
            {
                yield return new WaitForSeconds(_timeClear);

                for (int i = 0; i < createdSounds.Count; i++)
                {
                    if (!createdSounds[i].IsPlaying)
                    {
                        createdSounds[i].Destroy();
                        createdSounds.RemoveAt(i);
                        i--;
                    }
                }

                for (int i = 0; i < createdMusics.Count; i++)
                {
                    if (!createdMusics[i].IsPlaying)
                    {
                        createdMusics[i].Destroy();
                        createdMusics.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        [Serializable]
        public class CreatedSound : MonoBehaviour
        {
            [SerializeField] private AudioSource source;
            [SerializeField] private SoundType type;
            [SerializeField] private float originalVolume;
            public SoundType Type => type;

            public bool IsPlaying => source.isPlaying;

            public CreatedSound(AudioSource source, SoundType type, float originalVolume)
            {
                this.source = source;
                this.type = type;
                this.originalVolume = originalVolume;
            }

            public void Init(AudioSource source, SoundType type, float originalVolume)
            {
                this.source = source;
                this.type = type;
                this.originalVolume = originalVolume;
            }

            public void Replay(float customPitch = 1)
            {
                source.pitch = customPitch;

                source.Stop();
                source.Play();
            }

            public void Stop()
            {
                source.Stop();
            }

            public void Destroy()
            {
                Destroy(source.gameObject);
            }
        }

        [Serializable]
        public class CreatedMusic : MonoBehaviour
        {
            [SerializeField] private AudioSource source;
            [SerializeField] private MusicType type;
            [SerializeField] private float originalVolume;
            public MusicType Type => type;
            public bool IsPlaying => source.isPlaying;


            public CreatedMusic(AudioSource source, MusicType type, float originalVolume)
            {
                this.source = source;
                this.type = type;
                this.originalVolume = originalVolume;
            }

            public void Init(AudioSource source, MusicType type, float originalVolume)
            {
                this.source = source;
                this.type = type;
                this.originalVolume = originalVolume;
            }

            public void Stop(MusicChangeType effectType, out int transitionID, Action endStopAction = null)
            {
                transitionID = -1;

                float startValue = source.volume;
                float finalValue = 0;

                Action finalStop = () =>
                {
                    source.volume = finalValue;
                    source.Stop();

                    endStopAction?.Invoke();
                };

                switch (effectType)
                {
                    case MusicChangeType.None:
                        finalStop?.Invoke();
                        break;
                    
                    case MusicChangeType.Volume:
                        
                        var descr = LeanTween.value(source.gameObject, startValue, finalValue, TimeMusicChange).setOnUpdate( (float val)=>
                        { 
                            source.volume = val;
                        }).setOnComplete(() =>
                        {
                            finalStop?.Invoke();
                        });
                        transitionID = descr.id;
                        break;

                    case MusicChangeType.Pitch:
                        break;
                }
            }

            public void Destroy()
            {
                Destroy(source.gameObject);
            }
        }

        #region Comparable

        public int CompareTo(IController other) => ToString().CompareTo(other.ToString());
        public int CompareToType<T>() where T : IController => ToString().CompareTo(typeof(T).ToString());

        #endregion
    }

    public enum MusicChangeType
    {
        None,
        Volume, 
        Pitch,
    }
}