namespace TownOfUs.Roles.Modifiers
{
    public class Vengeful : Modifier
    {
        public Vengeful(PlayerControl player) : base(player)
        {
            Name = "Vengeful";
            TaskText = () => "You'll make them regret voting for you";
            Color = Patches.Colors.Vengeful;
            ModifierType = ModifierEnum.Vengeful;
        }
    }
}