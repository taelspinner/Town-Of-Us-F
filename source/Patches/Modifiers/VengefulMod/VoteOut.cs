using HarmonyLib;
using Reactor.Utilities;
using System.Linq;
using TownOfUs.Modifiers.AssassinMod;
using TownOfUs.Patches.NeutralRoles;
using TownOfUs.Roles;
using TownOfUs.Roles.Modifiers;
using UnityEngine;

namespace TownOfUs.NeutralRoles.VengefulMod
{
    public static class Vengeance
    {
        public static PlayerControl FirstVoted = null;
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
    internal class CastVote
    {
        private static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] byte srcPlayerId, [HarmonyArgument(1)] byte suspectPlayerId)
        {
            var votingPlayer = Utils.PlayerById(srcPlayerId);
            var suspectPlayer = Utils.PlayerById(suspectPlayerId);
            if (!suspectPlayer.Is(ModifierEnum.Vengeful)) return;
            if (Vengeance.FirstVoted == null)
            {
                Vengeance.FirstVoted = votingPlayer;
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    internal class MeetingExiledEnd
    {
        private static void Postfix(ExileController __instance)
        {
            var player = __instance.initData.networkedPlayer?.Object;
            if (player == null) return;

            if (player.Is(ModifierEnum.Vengeful))
            {
                if (Vengeance.FirstVoted != null && Vengeance.FirstVoted.PlayerId != player.PlayerId)
                {
                    AssassinKill.RpcMurderPlayer(Vengeance.FirstVoted, player);
                    Role.GetRole(player).CorrectAssassinKills -= 1;
                }
            }
            Vengeance.FirstVoted = null;
        }
    }
}