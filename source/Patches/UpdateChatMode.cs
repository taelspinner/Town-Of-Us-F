using System.Linq;
using HarmonyLib;
using InnerNet;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.Patches
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.UpdateChatMode))]
    class UpdateChatMode
    {
        private static bool Prefix(ChatController __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Glitch) && !PlayerControl.LocalPlayer.Is(RoleEnum.Transporter)) return true;
            if (PlayerControl.LocalPlayer.Is(RoleEnum.Glitch))
                return __instance != Role.GetRole<Glitch>(PlayerControl.LocalPlayer).MimicList;
            if (Role.GetRole<Transporter>(PlayerControl.LocalPlayer).TransportList != null)
                Debug.Log("Chat mode prefix " + (__instance != Role.GetRole<Transporter>(PlayerControl.LocalPlayer).TransportList).ToString());
            return __instance != Role.GetRole<Transporter>(PlayerControl.LocalPlayer).TransportList;
        }
    }

}