using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TownOfUs.Modifiers.AssassinMod;
using TownOfUs.Roles;
using TownOfUs.Roles.Modifiers;
using UnityEngine;

namespace TownOfUs.CrewmateRoles.HunterMod
{
    public static class Retribution
    {
        public static PlayerControl LastVoted = null;
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    internal class MeetingExiledEnd
    {
        private static void Postfix(ExileController __instance)
        {
            var exiled = __instance.exiled;
            if (exiled == null) return;
            var player = exiled.Object;

            if (player.Is(RoleEnum.Hunter))
            {
                if (Retribution.LastVoted != null && Retribution.LastVoted.PlayerId != player.PlayerId)
                {
                    Debug.Log(Retribution.LastVoted.CurrentOutfit.PlayerName + " was the last vote");
                    AssassinKill.MurderPlayer(Retribution.LastVoted);
                }
            }
            Retribution.LastVoted = null;
        }
    }
}