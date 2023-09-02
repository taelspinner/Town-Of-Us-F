using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
using TMPro;

namespace TownOfUs.Roles
{
    public class Tagger : Role
    {
        public Dictionary<byte, ArrowBehaviour> TaggerArrows = new Dictionary<byte, ArrowBehaviour>();
        public PlayerControl ClosestPlayer;
        public DateTime LastTracked { get; set; }

        public int UsesLeft;
        public TextMeshPro UsesText;

        public bool ButtonUsable => UsesLeft != 0;

        public Tagger(PlayerControl player) : base(player)
        {
            Name = "Tagger";
            ImpostorText = () => "Track Everyone's Movement";
            TaskText = () => "Tag suspicious players and track them";
            Color = Patches.Colors.Tagger;
            LastTracked = DateTime.UtcNow;
            RoleType = RoleEnum.Tagger;
            AddToRoleHistory(RoleType);

            UsesLeft = CustomGameOptions.MaxTracks;
        }

        public float TaggerTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - LastTracked;
            var num = CustomGameOptions.TrackCd * 1000f;
            var flag2 = num - (float) timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }

        public bool IsTracking(PlayerControl player)
        {
            return TaggerArrows.ContainsKey(player.PlayerId);
        }

        public void DestroyArrow(byte targetPlayerId)
        {
            var arrow = TaggerArrows.FirstOrDefault(x => x.Key == targetPlayerId);
            if (arrow.Value != null)
                Object.Destroy(arrow.Value);
            if (arrow.Value.gameObject != null)
                Object.Destroy(arrow.Value.gameObject);
            TaggerArrows.Remove(arrow.Key);
        }
    }
}