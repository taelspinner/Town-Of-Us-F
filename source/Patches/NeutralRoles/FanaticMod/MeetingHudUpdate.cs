using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.NeutralRoles.FanaticMod
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    public static class MeetingHudUpdate
    {
        public static void Postfix(MeetingHud __instance)
        {
            var localPlayer = PlayerControl.LocalPlayer;
            var _role = Role.GetRole(localPlayer);
            if (_role?.RoleType != RoleEnum.Fanatic) return;
            if (localPlayer.Data.IsDead) return;
            var role = (Fanatic)_role;
            foreach (var state in __instance.playerStates)
            {
                var targetId = state.TargetPlayerId;
                var playerData = Utils.PlayerById(targetId)?.Data;
                if (playerData == null || playerData.Disconnected)
                {
                    continue;
                }
                if (role.ConvertingPlayer?.PlayerId == targetId) state.NameText.color = Patches.Colors.Fanatic;
            }
        }
    }
}