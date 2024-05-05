using System;
using System.Collections;
using System.Linq;
using Reactor.Utilities.Extensions;
using TownOfUs.CrewmateRoles.MedicMod;
using TownOfUs.Roles;
using UnityEngine;
using AmongUs.GameOptions;
using Reactor.Utilities;

namespace TownOfUs.CrewmateRoles.ImmortalMod
{
    public class Coroutine
    {

        public static IEnumerator ImmortalRevive(Immortal role)
        {
            var startTime = DateTime.UtcNow;
            while (true)
            {
                var now = DateTime.UtcNow;
                var seconds = (now - startTime).TotalSeconds;
                if (seconds < CustomGameOptions.ImmortalReviveDuration || (MeetingHud.Instance && CustomGameOptions.ImmortalMeetingRevive))
                    yield return null;
                else break;

                if (MeetingHud.Instance && !CustomGameOptions.ImmortalMeetingRevive) yield break;
            }

            Vector2? bodyPos = null;
            foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
            {
                if (deadBody.ParentId == role.Player.PlayerId)
                {
                    bodyPos = deadBody.TruePosition;
                    deadBody.gameObject.Destroy();
                }
            }

            if (bodyPos == null) yield break;
            Vector2 position = (Vector2)bodyPos;

            role.Player.Revive();
            RoleManager.Instance.SetRole(role.Player, RoleTypes.Crewmate);
            Murder.KilledPlayers.Remove(
                Murder.KilledPlayers.FirstOrDefault(x => x.PlayerId == role.Player.PlayerId));
            if (role.Player.AmOwner)
            {
                Minigame.Instance.Close();
                yield return Utils.FlashCoroutine(role.Color, 1f, 0.5f);
            }
            role.Player.NetTransform.SnapTo(new Vector2(position.x, position.y + 0.3636f));

            if (Patches.SubmergedCompatibility.isSubmerged() && PlayerControl.LocalPlayer.PlayerId == role.Player.PlayerId)
            {
                Patches.SubmergedCompatibility.ChangeFloor(role.Player.transform.position.y > -7);
            }
        }
    }
}