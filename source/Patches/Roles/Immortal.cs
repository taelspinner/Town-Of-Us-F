namespace TownOfUs.Roles
{
    public class Immortal : Role
    {
        
        public Immortal(PlayerControl player) : base(player)
        {
            Name = "Immortal";
            ImpostorText = () => "Figure Out Who Killed You";
            TaskText = () => "You cannot die, but you cannot see";
            Color = Patches.Colors.Immortal;
            RoleType = RoleEnum.Immortal;
            AddToRoleHistory(RoleType);
            Utils.Camouflage();
        }
    }
}