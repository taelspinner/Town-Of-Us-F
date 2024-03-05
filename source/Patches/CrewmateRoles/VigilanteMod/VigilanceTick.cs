using HarmonyLib;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.VigilanteMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPriority(Priority.Last)]
    public class VigilanceTick
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Vigilante)) return;
            var vigilante = Role.GetRole<Vigilante>(PlayerControl.LocalPlayer);
            if (vigilante.Enabled && !vigilante.ButtonUsable) vigilante.UnVigilance();
            else if (vigilante.InVigilance) vigilante.Vigilance();
            else if (vigilante.Enabled) vigilante.UnVigilance();
        }
    }
}