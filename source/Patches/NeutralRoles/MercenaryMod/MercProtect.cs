using HarmonyLib;
using Hazel;
using Reactor.Utilities;
using TownOfUs.Roles;

namespace TownOfUs.CrewmateRoles.MercenaryMod
{
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    public class MercProtect
    {
        public static bool Prefix(KillButton __instance)
        {
            if (__instance != DestroyableSingleton<HudManager>.Instance.KillButton) return true;
            var flag = PlayerControl.LocalPlayer.Is(RoleEnum.Mercenary);
            if (!flag) return true;
            var role = Role.GetRole<Mercenary>(PlayerControl.LocalPlayer);
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (__instance == role.ArmorButton)
            {
                if (!__instance.isActiveAndEnabled) return false;
                if (__instance.isCoolingDown) return false;
                if (role.ArmorTimer() != 0) return false;
                role.TimeRemaining = CustomGameOptions.VestDuration;
                role.DonArmor();
                Utils.Rpc(CustomRPC.DonArmor, PlayerControl.LocalPlayer.PlayerId);
                return false;
            }
            if (role.ShieldedPlayer != null || role.ClosestPlayer == null) return false;
            if (role.StartTimer() > 0) return false;

            var interact = Utils.Interact(PlayerControl.LocalPlayer, role.ClosestPlayer);
            if (interact[4] == true)
            {
                Utils.Rpc(CustomRPC.MercProtect, PlayerControl.LocalPlayer.PlayerId, role.ClosestPlayer.PlayerId);

                role.ShieldedPlayer = role.ClosestPlayer;
                return false;
            }
            if (interact[5] == true)
            {
                role.StartTimer();
            }
            return false;
        }
    }
}
