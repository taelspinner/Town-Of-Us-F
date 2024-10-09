using System.Linq;
using UnityEngine;
using TownOfUs.Extensions;

namespace TownOfUs.Roles
{
    public class Mayor : Role
    {
        public bool Enabled;
        public DateTime LastBodyguarded;
        public float TimeRemaining;

        public int UsesLeft;
        public TextMeshPro UsesText;

        public bool ButtonUsable => UsesLeft != 0;
        public Mayor(PlayerControl player) : base(player)
        {
            Name = "Mayor";
            TaskText = () => "Lead the town to victory";
            Color = Patches.Colors.Mayor;
            LastBodyguarded = DateTime.UtcNow;
            RoleType = RoleEnum.Mayor;
            AddToRoleHistory(RoleType);
            Revealed = false;
            UsesLeft = 0;
        }
        public bool Revealed { get; set; }

        public GameObject RevealButton = new GameObject();
        public bool Bodyguarded => TimeRemaining > 0f;

        public float BodyguardTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - LastBodyguarded;
            ;
            var num = CustomGameOptions.BodyguardCd * 1000f;
            var flag2 = num - (float)timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float)timeSpan.TotalMilliseconds) / 1000f;
        }

        public void Bodyguard()
        {
            Enabled = true;
            TimeRemaining -= Time.deltaTime;
        }

        public void UnBodyguard()
        {
            Enabled = false;
            LastBodyguarded = DateTime.UtcNow;
        }

        internal override bool Criteria()
        {
            return (Revealed && !Player.Data.IsDead && (Utils.ImmortalFullyDead() || MeetingHud.Instance)) || base.Criteria();
        }

        internal override bool RoleCriteria()
        {
            if (!Player.Data.IsDead && (Utils.ImmortalFullyDead() || MeetingHud.Instance)) return Revealed || base.RoleCriteria();
            return false || base.RoleCriteria();
        }

        internal override bool GameEnd(LogicGameFlowNormal __instance)
        {
            if (Player.Data.IsDead || Player.Data.Disconnected || !CustomGameOptions.CrewKillersContinue) return true;

            if (PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected && x.Data.IsImpostor()) > 0) return false;

            return true;
        }
    }
}