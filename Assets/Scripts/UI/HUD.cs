using UnityEngine;
using UnityEngine.UI;

namespace MB
{
    /// In-stage HUD: 28-tick health bar (left), weapon energy bar beside it
    /// when a special weapon is equipped, boss bar (right) during boss fights,
    /// and a lives counter.
    public class HUD : MonoBehaviour
    {
        Image hpFill;
        Image weaponFill;
        GameObject weaponRoot;
        Image bossFill;
        GameObject bossRoot;
        Text livesText;

        void Start()
        {
            var canvas = UIBuilder.NewCanvas("HUDCanvas", 10);
            canvas.transform.SetParent(transform, false);

            hpFill = UIBuilder.AddBar(canvas.transform, new Vector2(0, 1),
                new Vector2(12, -12), new Color(1f, 0.95f, 0.7f));

            weaponRoot = new GameObject("WeaponBar");
            var wrt = weaponRoot.AddComponent<RectTransform>();
            wrt.SetParent(canvas.transform, false);
            wrt.anchorMin = wrt.anchorMax = wrt.pivot = new Vector2(0, 1);
            wrt.anchoredPosition = new Vector2(26, -12);
            wrt.sizeDelta = Vector2.zero;
            weaponFill = UIBuilder.AddBar(weaponRoot.transform, new Vector2(0, 1),
                Vector2.zero, Color.white);

            livesText = UIBuilder.AddText(canvas.transform, "REST 2", 10,
                new Vector2(0, 0), new Vector2(10, 8), new Vector2(120, 14),
                TextAnchor.LowerLeft, Color.white);

            bossRoot = new GameObject("BossBar");
            var brt = bossRoot.AddComponent<RectTransform>();
            brt.SetParent(canvas.transform, false);
            brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(1, 1);
            brt.anchoredPosition = new Vector2(-12, -12);
            brt.sizeDelta = Vector2.zero;
            bossFill = UIBuilder.AddBar(bossRoot.transform, new Vector2(1, 1),
                Vector2.zero, new Color(1f, 0.5f, 0.4f));
            bossRoot.SetActive(false);
        }

        void Update()
        {
            var sc = StageController.I;
            if (sc == null || hpFill == null) return;

            if (sc.PlayerHealth != null)
                hpFill.fillAmount = (float)sc.PlayerHealth.HP / GameConfig.MaxHealth;

            var weapons = sc.Player != null ? sc.Player.GetComponent<WeaponSystem>() : null;
            if (weapons != null)
            {
                bool special = weapons.Current != WeaponId.Buster;
                if (weaponRoot.activeSelf != special) weaponRoot.SetActive(special);
                if (special)
                {
                    weaponFill.fillAmount = (float)weapons.CurrentEnergy / GameConfig.MaxWeaponEnergy;
                    weaponFill.color = weapons.CurrentTint;
                }
            }

            livesText.text = "REST " + Mathf.Max(0, GameManager.I.Lives);

            bool showBoss = sc.ActiveBoss != null;
            if (bossRoot.activeSelf != showBoss) bossRoot.SetActive(showBoss);
            if (showBoss)
                bossFill.fillAmount = (float)sc.ActiveBoss.DisplayHP / sc.ActiveBoss.MaxHP;
        }
    }
}
