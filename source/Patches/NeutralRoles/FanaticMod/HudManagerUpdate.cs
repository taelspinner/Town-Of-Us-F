using AmongUs.GameOptions;
using HarmonyLib;
using System.Linq;
using TownOfUs.Extensions;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.NeutralRoles.FanaticMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdate
    {
        public static Sprite IndoctrinateSprite => TownOfUs.IndoctrinateSprite;

        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Fanatic)) return;
            var role = Role.GetRole<Fanatic>(PlayerControl.LocalPlayer);

            var notFanatic = PlayerControl.AllPlayerControls
                .ToArray()
                .Where(x => !x.Is(RoleEnum.Fanatic) || (Role.GetRole<Fanatic>(x).WasConverted && !role.WasConverted && CustomGameOptions.FanaticLeaderCanKillFollowers))
                .ToList();

            __instance.KillButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);
            __instance.KillButton.SetCoolDown(role.KillTimer(), CustomGameOptions.FanaticKillCd);
            Utils.SetTarget(ref role.ClosestPlayer, __instance.KillButton, targets: notFanatic);

            if (role.WasConverted) return;

            if (role.IndoctrinateButton == null)
            {
                role.IndoctrinateButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.IndoctrinateButton.graphic.enabled = true;
                role.IndoctrinateButton.gameObject.SetActive(false);
            }

            role.IndoctrinateButton.graphic.sprite = IndoctrinateSprite;
            role.IndoctrinateButton.transform.localPosition = new Vector3(-2f, 0f, 0f);

            role.IndoctrinateButton.gameObject.SetActive((__instance.UseButton.isActiveAndEnabled || __instance.PetButton.isActiveAndEnabled)
                    && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead
                    && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started);

            if (role.ConvertingPlayer != null)
            {
                if (!role.ConvertingPlayer.Data.IsDead && !role.ConvertingPlayer.Data.Disconnected)
                {
                    role.ConvertingPlayer.myRend().material.SetFloat("_Outline", 1f);
                    role.ConvertingPlayer.myRend().material.SetColor("_OutlineColor", new Color(0.65f, 0.25f, 0.65f));
                    if (role.ConvertingPlayer.GetCustomOutfitType() != CustomPlayerOutfitType.Camouflage &&
                        role.ConvertingPlayer.GetCustomOutfitType() != CustomPlayerOutfitType.Swooper)
                        role.ConvertingPlayer.nameText().color = new Color(0.65f, 0.25f, 0.65f);
                    else role.ConvertingPlayer.nameText().color = Color.clear;
                }
                else
                {
                    role.ConvertingPlayer = null;
                    Utils.Rpc(CustomRPC.Indoctrinate, PlayerControl.LocalPlayer.PlayerId, byte.MaxValue);
                }
            }

            var aliveFanatics = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(RoleEnum.Fanatic) && !x.Data.IsDead && !x.Data.Disconnected).ToList();
            var renderer = role.IndoctrinateButton.graphic;
            if (role.ConvertingPlayer == null && aliveFanatics.Count == 1)
            {
                renderer.color = Palette.EnabledColor;
                renderer.material.SetFloat("_Desat", 0f);
                Utils.SetTarget(ref role.ClosestPlayer, role.IndoctrinateButton, GameOptionsData.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance], notFanatic);
                role.IndoctrinateButton.SetCoolDown(role.KillTimer(), CustomGameOptions.FanaticKillCd);
            }
            else
            {
                renderer.color = Palette.DisabledClear;
                renderer.material.SetFloat("_Desat", 1f);
                role.IndoctrinateButton.SetCoolDown(0f, CustomGameOptions.FanaticKillCd);
            }
        }
    }
}
