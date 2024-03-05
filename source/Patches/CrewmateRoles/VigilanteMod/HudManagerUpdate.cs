using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.VigilanteMod
{
    [HarmonyPatch(typeof(HudManager))]
    public class HudManagerUpdate
    {
        [HarmonyPatch(nameof(HudManager.Update))]
        public static void Postfix(HudManager __instance)
        {
            UpdateVigilanceButton(__instance);
        }

        public static void UpdateVigilanceButton(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Vigilante)) return;
            var vigilanceButton = __instance.KillButton;

            var role = Role.GetRole<Vigilante>(PlayerControl.LocalPlayer);

            vigilanceButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            if (role.InVigilance) vigilanceButton.SetCoolDown(role.TimeRemaining, CustomGameOptions.VigilanceDuration);
            else vigilanceButton.SetCoolDown(role.VigilanceTimer(), CustomGameOptions.VigilanceCd);

            var renderer = vigilanceButton.graphic;
            if (role.InVigilance || (!vigilanceButton.isCoolingDown && role.ButtonUsable))
            {
                renderer.color = Palette.EnabledColor;
                renderer.material.SetFloat("_Desat", 0f);
            }
            else
            {
                renderer.color = Palette.DisabledClear;
                renderer.material.SetFloat("_Desat", 1f);
            }
        }
    }
}