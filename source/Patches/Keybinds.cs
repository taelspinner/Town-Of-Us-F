using HarmonyLib;
using Rewired;
using Rewired.Data;

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
            __instance.userData.GetAction("ActionSecondary").descriptiveName = "Kill / Crew & neutral benign abilities / infect & douse";
            __instance.userData.RegisterBind("ToU imp/nk", "Impostor abilities / ignite");
            __instance.userData.RegisterBind("ToU bb/disperse/mimic", "Button barry / disperse / glitch mimic");
            __instance.userData.RegisterBind("ToU hack", "Glitch's hack");
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
}