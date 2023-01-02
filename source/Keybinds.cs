using System.IO;
using System.Text.Json;
using Rewired.UI.ControlMapper;
using HarmonyLib;
using UnityEngine;

namespace TownOfUs
{
    public class Keybindsjson
    {
        public string Kill { get; set; }

        public string RoleAbility { get; set; }
    }

    [HarmonyPatch]

    public class KeybindPatches
    {
        public static readonly string Jsonpath = Application.persistentDataPath + "\\ToUKeybinds.json";

        public static readonly JsonSerializerOptions opts = new()
        {
            WriteIndented = true
        };

        [HarmonyPatch(typeof(ControlMapper), nameof(ControlMapper.OnKeyboardElementAssignmentPollingWindowUpdate))]
        [HarmonyPostfix]
        public static void Postfix(ControlMapper __instance)
        {
            var json = ReadJson();
            if (__instance.pendingInputMapping.actionName == "Kill")
            {
                string newbind = __instance.pendingInputMapping.elementName;
                if (newbind != "None")
                {
                    newbind = newbind.Replace(" ", string.Empty);
                    WriteJson(newbind, json.RoleAbility);
                }
            }
            else if (__instance.pendingInputMapping.actionName == "Role Ability")
            {
                string newbind = __instance.pendingInputMapping.elementName;
                if (newbind != "None")
                {
                    newbind = newbind.Replace(" ", string.Empty);
                    WriteJson(json.Kill, newbind);
                }
            }
        }

        public static Keybindsjson ReadJson()
        {
            var file = File.ReadAllText(Jsonpath);
            var json = (Keybindsjson) JsonSerializer.Deserialize
                                                (file,
                                                typeof(Keybindsjson),
                                                opts);

            return json;
        }

        public static void WriteJson(string killkey, string Abilitykey)
        {
            Keybindsjson json = new()
            {
                Kill = killkey,
                RoleAbility = Abilitykey
            };
            string data = JsonSerializer.Serialize(json, opts);
            File.WriteAllText(Jsonpath, data);
        }
    }
}