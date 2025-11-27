using UnityEngine;
using System.Collections.Generic;

namespace DiceSurvivor.Audio
{
    /// <summary>
    /// 효과음 종류 (원하는 대로 추가 가능)
    /// </summary>
    public enum SfxType
    {
        // 근접 무기
        Scythe,
        Staff,
        Spear,
        GreatSword,
        Hammer,
        Whip,

        // 원거리 무기
        BoomerangAndChakram,
        Fireball,
        PoisonFlask,
        Laser,

        // 피격 효과음
        PlayerAttacked,
        PlayerDeath,
        EnemyAttacked,
        EnemyDeath,
        BoomerangHit,
        ChakramHit,

        // Splash
        KillingAura,
        Icicle,
        LightningStaff,
        Asteroid,

        // 기타 SFX
        StageClear,
        Defeat,
        CollectItem
    }

    /// <summary>
    /// 범용 오디오 매니저 (SFX 전용)
    /// - 어디서든 복붙해서 바로 사용 가능
    /// - Inspector에서 효과음 종류와 클립만 추가하면 됨
    /// - PlaySfx(SfxType.XXX) 로 호출
    /// </summary>
    public class SfxManager : MonoBehaviour
    {
        #region Variables
        public static SfxManager Instance;

        [Header("SFX Audio Source")]
        public AudioSource sfxSource;

        [Header("SFX List")]
        public List<SfxEntry> sfxEntries = new List<SfxEntry>();

        private Dictionary<SfxType, AudioClip[]> sfxDict;
        #endregion

        #region Unity Event Method
        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitDictionary();
        }
        #endregion

        #region Custom Method
        private void InitDictionary()
        {
            sfxDict = new Dictionary<SfxType, AudioClip[]>();
            foreach (var entry in sfxEntries)
            {
                if (entry == null || entry.clips == null) continue;
                if (!sfxDict.ContainsKey(entry.type))
                    sfxDict.Add(entry.type, entry.clips);
            }
        }

        /// <summary>
        /// 지정된 효과음을 재생합니다 (랜덤 지원)
        /// </summary>
        public void PlaySfx(SfxType type)
        {
            if(!sfxDict.TryGetValue(type, out var clips) || clips.Length == 0)
            {
                Debug.LogWarning($"[AudioManager] {type} SFX not set!");
                return;
            }

            int index = Random.Range(0, clips.Length);
            sfxSource.PlayOneShot(clips[index]);
        }
        #endregion
    }

    [System.Serializable]
    public class SfxEntry
    {
        public SfxType type;
        public AudioClip[] clips;
    }
}