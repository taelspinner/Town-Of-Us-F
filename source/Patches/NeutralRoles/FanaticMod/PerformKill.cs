using System;
using HarmonyLib;
using TownOfUs.Roles;
using AmongUs.GameOptions;
using static UnityEngine.GraphicsBuffer;
using TownOfUs.Extensions;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace TownOfUs.NeutralRoles.FanaticMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class PerformKill
    {
        public static bool Prefix(KillButton __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Fanatic)) return true;
            if (PlayerControl.LocalPlayer.Data.IsDead) return false;
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            var role = Role.GetRole<Fanatic>(PlayerControl.LocalPlayer);
            if (role.Player.inVent) return false;
            if (role.KillTimer() != 0) return false;
            if (!__instance.isActiveAndEnabled || __instance.isCoolingDown) return false;
            if (role.ClosestPlayer == null) return false;
            var target = role.ClosestPlayer;

            if (__instance == role.IndoctrinateButton)
            {
                var aliveFanatics = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Is(RoleEnum.Fanatic) && !x.Data.IsDead && !x.Data.Disconnected).ToList();
                bool canIndoctrinate = (role.ClosestPlayer.Is(Faction.Crewmates) || (role.ClosestPlayer.Is(Faction.NeutralBenign)
                    && CustomGameOptions.CanConvertNeutralBenign) || (role.ClosestPlayer.Is(Faction.NeutralEvil)
                    && CustomGameOptions.CanConvertNeutralEvil)) && aliveFanatics.Count == 1 && !role.WasConverted;
                var indoctrinate = Utils.Interact(PlayerControl.LocalPlayer, target);
                role.IndoctrinateButton.SetCoolDown(0.01f, 1f);
                if (indoctrinate[4] && canIndoctrinate)
                {
                    role.ConvertingPlayer = target;
                    Utils.Rpc(CustomRPC.Indoctrinate, PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
                    role.LastKilled = DateTime.UtcNow;
                }
                else if (indoctrinate[1])
                {
                    role.LastKilled = DateTime.UtcNow;
                    role.LastKilled = role.LastKilled.AddSeconds(CustomGameOptions.ProtectKCReset - CustomGameOptions.FanaticKillCd);
                }
                else if (indoctrinate[2])
                {
                    role.LastKilled = DateTime.UtcNow;
                    role.LastKilled = role.LastKilled.AddSeconds(CustomGameOptions.VestKCReset - CustomGameOptions.FanaticKillCd);
                }
                else if (!indoctrinate[3])
                {
                    role.LastKilled = DateTime.UtcNow;
                }
                return false;
            }

            if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            var distBetweenPlayers = Utils.GetDistBetweenPlayers(PlayerControl.LocalPlayer, role.ClosestPlayer);
            var flag3 = distBetweenPlayers <
                        GameOptionsData.KillDistances[GameOptionsManager.Instance.currentNormalGameOptions.KillDistance];
            if (!flag3) return false;

            var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer, true);
            if (interact[4] == true && role.ConvertingPlayer != null)
            {
                // indoctrinated player has to witness the kill to be converted
                var switchSystem = GameOptionsManager.Instance.currentNormalGameOptions.MapId == 5 ? null : ShipStatus.Instance.Systems[SystemTypes.Electrical]?.TryCast<SwitchSystem>();
                var lightRadius = switchSystem == null ? 5.25f : (switchSystem.Value/255f) * 5f;
                var sightMod = GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
                if (role.ConvertingPlayer.Is(RoleEnum.Glitch) ||
                    role.ConvertingPlayer.Is(RoleEnum.Juggernaut) || role.ConvertingPlayer.Is(RoleEnum.Pestilence) ||
                    (role.ConvertingPlayer.Is(RoleEnum.Jester) && CustomGameOptions.JesterImpVision) ||
                    (role.ConvertingPlayer.Is(RoleEnum.Arsonist) && CustomGameOptions.ArsoImpVision) ||
                    (role.ConvertingPlayer.Is(RoleEnum.Vampire) && CustomGameOptions.VampImpVision) ||
                    (role.ConvertingPlayer.Is(RoleEnum.Fanatic) && CustomGameOptions.FanaticImpVision))
                {
                    lightRadius = 5.25f;
                    sightMod = GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;
                }
                else if (role.ConvertingPlayer.Is(RoleEnum.Werewolf))
                {
                    var ww = Role.GetRole<Werewolf>(role.ConvertingPlayer);
                    if (ww.Rampaged)
                    {
                        lightRadius = 5.25f;
                        sightMod = GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod;
                    }
                }
                var distFromTarget = Utils.GetDistBetweenPlayers(role.ClosestPlayer, role.ConvertingPlayer);
                var vector = role.ConvertingPlayer.GetTruePosition() - role.ClosestPlayer.GetTruePosition();
                bool canConvert = distFromTarget < lightRadius * sightMod && !PhysicsHelpers.AnyNonTriggersBetween(
                    role.Player.GetTruePosition(), vector.normalized, vector.magnitude, Constants.ShipAndObjectsMask
                );
                if (canConvert)
                {
                    Utils.Rpc(CustomRPC.FanaticConvert, role.Player.PlayerId, role.ConvertingPlayer.PlayerId);
                    role.ConvertingPlayer.myRend().material.SetFloat("_Outline", 0f);
                    role.ConvertPlayer(role.ConvertingPlayer);
                }
            }
            else if (interact[0] == true)
            {
                role.LastKilled = DateTime.UtcNow;
            }
            else if (interact[1] == true)
            {
                role.LastKilled = DateTime.UtcNow;
                role.LastKilled = role.LastKilled.AddSeconds(CustomGameOptions.ProtectKCReset - CustomGameOptions.FanaticKillCd);
            }
            else if (interact[2] == true)
            {
                role.LastKilled = DateTime.UtcNow;
                role.LastKilled = role.LastKilled.AddSeconds(CustomGameOptions.VestKCReset - CustomGameOptions.FanaticKillCd);
            }
            return false;
        }
    }
}
