namespace TownOfUs.Roles
{
    public class Engineer : Role
    {
        public Engineer(PlayerControl player) : base(player)
        {
            Name = "Engineer";
            ImpostorText = () => "Maintain Important Systems On The Ship";
            TaskText = () => CustomGameOptions.GameMode == GameMode.Cultist ? "Vent around" : "Vent around and fix sabotages";
            Color = Patches.Colors.Engineer;
            RoleType = RoleEnum.Engineer;
            AddToRoleHistory(RoleType);
        }

        public bool UsedThisRound { get; set; } = false;
    }
}