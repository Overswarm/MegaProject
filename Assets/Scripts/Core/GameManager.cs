using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MB
{
    /// Persistent game state: lives, tanks, unlocked weapons, weapon energy,
    /// cleared stages, current stage + checkpoint. Survives scene loads.
    public class GameManager : MonoBehaviour
    {
        static GameManager _i;
        public static GameManager I
        {
            get
            {
                if (_i == null)
                {
                    var go = new GameObject("GameManager");
                    _i = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                    _i.LoadSave();
                }
                return _i;
            }
        }

        public int Lives = GameConfig.StartingLives;
        public int ETanks;
        public int WTanks;
        public HashSet<WeaponId> Unlocked = new HashSet<WeaponId> { WeaponId.Buster };
        public Dictionary<WeaponId, int> Energy = new Dictionary<WeaponId, int>();
        public HashSet<int> Cleared = new HashSet<int>();

        public StageData CurrentStage;
        public int CheckpointIndex;

        void Awake()
        {
            if (_i != null && _i != this) { Destroy(gameObject); return; }
            _i = this;
            DontDestroyOnLoad(gameObject);
        }

        public void NewGame()
        {
            Lives = GameConfig.StartingLives;
            ETanks = 0; WTanks = 0;
            Unlocked = new HashSet<WeaponId> { WeaponId.Buster };
            Energy = new Dictionary<WeaponId, int>();
            Cleared = new HashSet<int>();
            CurrentStage = null;
            CheckpointIndex = 0;
            Save();
        }

        public void BeginStage(StageData s)
        {
            CurrentStage = s;
            CheckpointIndex = 0;
            SceneManager.LoadScene(s.sceneName);
        }

        public void OnStageCleared()
        {
            if (CurrentStage == null) return;
            Cleared.Add(CurrentStage.slot);
            Unlocked.Add(CurrentStage.reward);
            Energy[CurrentStage.reward] = GameConfig.MaxWeaponEnergy;
            Save();
        }

        public int GetEnergy(WeaponId w)
        {
            if (w == WeaponId.Buster) return GameConfig.MaxWeaponEnergy;
            return Energy.TryGetValue(w, out var e) ? e : GameConfig.MaxWeaponEnergy;
        }

        public void SetEnergy(WeaponId w, int v)
        {
            if (w == WeaponId.Buster) return;
            Energy[w] = Mathf.Clamp(v, 0, GameConfig.MaxWeaponEnergy);
        }

        public void AddEnergy(WeaponId w, int amt) => SetEnergy(w, GetEnergy(w) + amt);

        public void RefillAllWeapons()
        {
            foreach (var w in Unlocked) SetEnergy(w, GameConfig.MaxWeaponEnergy);
        }

        // ---- persistence (PlayerPrefs) ----

        const string KeySave = "mb_save";

        public static bool HasSave => PlayerPrefs.HasKey(KeySave);

        public void Save()
        {
            int mask = 0;
            foreach (var s in Cleared) mask |= 1 << s;
            int weapons = 0;
            foreach (var w in Unlocked) weapons |= 1 << (int)w;
            PlayerPrefs.SetString(KeySave, $"{mask}|{weapons}|{ETanks}|{WTanks}");
            PlayerPrefs.Save();
        }

        public void LoadSave()
        {
            if (!HasSave) return;
            var parts = PlayerPrefs.GetString(KeySave).Split('|');
            if (parts.Length < 4) return;
            int.TryParse(parts[0], out var mask);
            int.TryParse(parts[1], out var weapons);
            int.TryParse(parts[2], out var et);
            int.TryParse(parts[3], out var wt);
            Cleared = new HashSet<int>();
            for (int i = 0; i < 8; i++) if ((mask & (1 << i)) != 0) Cleared.Add(i);
            Unlocked = new HashSet<WeaponId> { WeaponId.Buster };
            for (int i = 0; i < 8; i++)
                if ((weapons & (1 << i)) != 0) Unlocked.Add((WeaponId)i);
            ETanks = et; WTanks = wt;
        }

        public static void WipeSave()
        {
            PlayerPrefs.DeleteKey(KeySave);
            PlayerPrefs.Save();
        }
    }
}
