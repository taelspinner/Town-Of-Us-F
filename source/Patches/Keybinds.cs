using HarmonyLib;
using Rewired;
using Rewired.Data;
using System.Linq;
using TownOfUs.Roles;
using TownOfUs.Roles.Modifiers;
using Ability = TownOfUs.Roles.Modifiers.Ability;

namespace TownOfUs
{
    //thanks to TheOtherRolesAU/TheOtherRoles/pull/347 by dadoum for the patch and extension
    [HarmonyPatch(typeof(InputManager_Base), nameof(InputManager_Base.Awake))]
    public static class Keybinds
    {
        [HarmonyPrefix]
        private static void Prefix(InputManager_Base __instance)
        {
            //change the text shown on the screen for the kill keybind
            __instance.userData.GetAction("ActionSecondary").descriptiveName = "Kill / Primary Ability";
            __instance.userData.RegisterBind("ToU imp/nk", "Secondary Ability");
            __instance.userData.RegisterBind("ToU bb/disperse/mimic", "Mimic");
            __instance.userData.RegisterBind("ToU hack", "Hack");
            __instance.userData.RegisterBind("ToU cycle +", "Cycle Forward");
            __instance.userData.RegisterBind("ToU cycle -", "Cycle Backward");
            __instance.userData.RegisterBind("ToU cycle players", "Cycle Players in Meetings");
            __instance.userData.RegisterBind("ToU confirm", "Confirm Choice");
        }

        private static int RegisterBind(this UserData self, string name, string description, int elementIdentifierId = -1, int category = 0, InputActionType type = InputActionType.Button)
        {
            self.AddAction(category);
            var action = self.GetAction(self.actions.Count - 1)!;

            action.name = name;
            action.descriptiveName = description;
            action.categoryId = category;
            action.type = type;
            action.userAssignable = true;

            var map = new ActionElementMap
            {
                _elementIdentifierId = elementIdentifierId,
                _actionId = action.id,
                _elementType = ControllerElementType.Button,
                _axisContribution = Pole.Positive,
                _modifierKey1 = ModifierKey.None,
                _modifierKey2 = ModifierKey.None,
                _modifierKey3 = ModifierKey.None
            };
            self.keyboardMaps[0].actionElementMaps.Add(map);
            self.joystickMaps[0].actionElementMaps.Add(map);

            return action.id;
        }
    }

    [HarmonyPatch]
    public sealed class AssassinVigilanteKeybinds
    {
        private static PlayerVoteArea HighlightedPlayer;

        private static int PlayerIndex;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        [HarmonyPostfix]
        public static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.Data.IsDead) return;
            if (__instance.state == MeetingHud.VoteStates.Discussion) return;
            var role = Role.GetRole(PlayerControl.LocalPlayer);

            foreach (var player in __instance.playerStates)
            {
                if (!HighlightedPlayer) break;
                if (player.TargetPlayerId == HighlightedPlayer.TargetPlayerId) 
                {
                    player.SetHighlighted(true);
                }
                else player.SetHighlighted(false);
            }
            
            if (role is Vigilante || role.Player.Is(AbilityEnum.Assassin))
            {
                dynamic guesser = role is Vigilante ? Role.GetRole<Vigilante>(role.Player) : Ability.GetAbility<Assassin>(role.Player);
                var players = __instance.playerStates.Where(x => (guesser as IGuesser).Buttons[x.TargetPlayerId] != (null, null, null, null)
                                                                  && x.TargetPlayerId != role.Player.PlayerId)
                                                     .ToList();

                if (ReInput.players.GetPlayer(0).GetButtonDown("ToU cycle players"))
                {
                    HighlightedPlayer = players[PlayerIndex];
                    PlayerIndex = PlayerIndex == players.Count - 1 ? 0 : PlayerIndex + 1;
                }

                if (!HighlightedPlayer) return;
                if (ReInput.players.GetPlayer(0).GetButtonDown("ToU cycle +"))
                {
                    (guesser as IGuesser).Buttons[HighlightedPlayer.TargetPlayerId].Item2.GetComponent<PassiveButton>().OnClick.Invoke();
                }
                else if (ReInput.players.GetPlayer(0).GetButtonDown("ToU cycle -"))
                {
                    (guesser as IGuesser).Buttons[HighlightedPlayer.TargetPlayerId].Item1.GetComponent<PassiveButton>().OnClick.Invoke();
                }
                else if (ReInput.players.GetPlayer(0).GetButtonDown("ToU confirm"))
                {
                    (guesser as IGuesser).Buttons[HighlightedPlayer.TargetPlayerId].Item3.GetComponent<PassiveButton>().OnClick.Invoke();
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
        [HarmonyPostfix]

        public static void Reset()
        {
            HighlightedPlayer = null;
            PlayerIndex = 0;
        }
    }
}