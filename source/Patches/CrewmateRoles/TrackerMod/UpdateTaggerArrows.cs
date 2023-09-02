using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;
using TownOfUs.Extensions;
using System;
using System.Linq;

namespace TownOfUs.CrewmateRoles.TaggerMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class UpdateTaggerArrows
    {
        public static Sprite Sprite => TownOfUs.Arrow;
        private static DateTime _time = DateTime.UnixEpoch;
        private static float Interval => CustomGameOptions.UpdateInterval;
        public static bool CamoedLastTick = false;

        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Tagger)) return;

            var role = Role.GetRole<Tagger>(PlayerControl.LocalPlayer);

            if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                role.TaggerArrows.Values.DestroyAll();
                role.TaggerArrows.Clear();
                return;
            }

            foreach (var arrow in role.TaggerArrows)
            {
                var player = Utils.PlayerById(arrow.Key);
                var playerBody = UnityEngine.Object.FindObjectsOfType<DeadBody>().Where(x => x.ParentId == player.PlayerId).FirstOrDefault();
                if (player == null || player.Data == null
                    || (player.Data.IsDead && (!CustomGameOptions.TaggerPersistBody || playerBody == null))
                    || player.Data.Disconnected)
                {
                    role.DestroyArrow(arrow.Key);
                    continue;
                }

                if (!CamouflageUnCamouflage.IsCamoed)
                {
                    if (RainbowUtils.IsRainbow(player.GetDefaultOutfit().ColorId))
                    {
                        arrow.Value.image.color = RainbowUtils.Rainbow;
                    }
                    else if (CamoedLastTick)
                    {
                        arrow.Value.image.color = Palette.PlayerColors[player.GetDefaultOutfit().ColorId];
                    }
                }
                else if (!CamoedLastTick)
                {
                    arrow.Value.image.color = Color.gray;
                }

                if (_time <= DateTime.UtcNow.AddSeconds(-Interval))
                    arrow.Value.target = player.Data.IsDead
                        ? playerBody.transform.position
                        : player.transform.position;
            }

            CamoedLastTick = CamouflageUnCamouflage.IsCamoed;
            if (_time <= DateTime.UtcNow.AddSeconds(-Interval))
                _time = DateTime.UtcNow;
        }
    }
}