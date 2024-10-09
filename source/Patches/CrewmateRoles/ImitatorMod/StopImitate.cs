using HarmonyLib;
using TownOfUs.Roles;
using UnityEngine;
using TownOfUs.CrewmateRoles.InvestigatorMod;
using TownOfUs.CrewmateRoles.TrapperMod;
using System.Collections.Generic;

namespace TownOfUs.CrewmateRoles.ImitatorMod
{

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
    class StartMeetingPatch
    {
        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo meetingTarget)
        {
            if (__instance == null)
            {
                return;
            }
            if (StartImitate.ImitatingPlayer != null && !StartImitate.ImitatingPlayer.Is(RoleEnum.Traitor))
            {
                List<RoleEnum> trappedPlayers = null;
                PlayerControl confessingPlayer = null;
                bool isImmortal = PlayerControl.LocalPlayer.Is(RoleEnum.Immortal);
                Dictionary<string, int> LimitedRoleUses = StartImitate.LimitedRoleUses;

                if (PlayerControl.LocalPlayer == StartImitate.ImitatingPlayer)
                {
                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Investigator)) Footprint.DestroyAll(Role.GetRole<Investigator>(PlayerControl.LocalPlayer));

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Engineer))
                    {
                        var engineerRole = Role.GetRole<Engineer>(PlayerControl.LocalPlayer);
                        LimitedRoleUses["Engineer"] = engineerRole.UsesLeft;
                        Object.Destroy(engineerRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Tagger))
                    {
                        var taggerRole = Role.GetRole<Tagger>(PlayerControl.LocalPlayer);
                        taggerRole.TaggerArrows.Values.DestroyAll();
                        taggerRole.TaggerArrows.Clear();
                        Object.Destroy(taggerRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.VampireHunter))
                    {
                        var vhRole = Role.GetRole<VampireHunter>(PlayerControl.LocalPlayer);
                        LimitedRoleUses["VampireHunter"] = vhRole.UsesLeft;
                        Object.Destroy(vhRole.UsesText);
                    }
                    
                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Aurial))
                    {
                        var aurialRole = Role.GetRole<Aurial>(PlayerControl.LocalPlayer);
                        aurialRole.SenseArrows.Values.DestroyAll();
                        aurialRole.SenseArrows.Clear();
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Hunter))
                    {
                        var hunterRole = Role.GetRole<Hunter>(PlayerControl.LocalPlayer);
                        LimitedRoleUses["Hunter"] = hunterRole.UsesLeft;
                        StartImitate.ImitatedCaughtPlayers.Clear();
                        StartImitate.ImitatedCaughtPlayers.AddRange(hunterRole.CaughtPlayers);
                        hunterRole.ClosestStalkPlayer = null;
                        hunterRole.StalkButton.gameObject.SetActive(false);
                        Object.Destroy(hunterRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Mystic))
                    {
                        var mysticRole = Role.GetRole<Mystic>(PlayerControl.LocalPlayer);
                        mysticRole.BodyArrows.Values.DestroyAll();
                        mysticRole.BodyArrows.Clear();
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Transporter))
                    {
                        var transporterRole = Role.GetRole<Transporter>(PlayerControl.LocalPlayer);
                        LimitedRoleUses["Transporter"] = transporterRole.UsesLeft;
                        Object.Destroy(transporterRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Veteran))
                    {
                        var veteranRole = Role.GetRole<Veteran>(PlayerControl.LocalPlayer);
                        LimitedRoleUses["Veteran"] = veteranRole.UsesLeft;
                        Object.Destroy(veteranRole.UsesText);
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Trapper))
                    {
                        var trapperRole = Role.GetRole<Trapper>(PlayerControl.LocalPlayer);
                        LimitedRoleUses["Trapper"] = trapperRole.UsesLeft;
                        Object.Destroy(trapperRole.UsesText);
                        trapperRole.traps.ClearTraps();
                        trappedPlayers = trapperRole.trappedPlayers;
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Oracle))
                    {
                        var oracleRole = Role.GetRole<Oracle>(PlayerControl.LocalPlayer);
                        oracleRole.ClosestPlayer = null;
                        confessingPlayer = oracleRole.Confessor;
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Warden))
                    {
                        var wardenRole = Role.GetRole<Warden>(PlayerControl.LocalPlayer);
                        wardenRole.ClosestPlayer = null;
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Detective))
                    {
                        var detecRole = Role.GetRole<Detective>(PlayerControl.LocalPlayer);
                        StartImitate.ImitatedDetectiveScenes.Clear();
                        StartImitate.ImitatedDetectiveScenes.AddRange(detecRole.CrimeScenes);
                        StartImitate.ImitatedInvestigatedPlayers.Clear();
                        StartImitate.ImitatedInvestigatedPlayers.AddRange(detecRole.InvestigatedPlayers);
                        StartImitate.ImitatedInvestigatingScene = detecRole.InvestigatingScene;
                        foreach(GameObject scene in StartImitate.ImitatedDetectiveScenes)
                            scene.SetActive(false);
                        detecRole.ClosestPlayer = null;
                        detecRole.ExamineButton.gameObject.SetActive(false);
                        foreach (GameObject scene in detecRole.CrimeScenes)
                        {
                            UnityEngine.Object.Destroy(scene);
                        }
                    }

                    if (PlayerControl.LocalPlayer.Is(RoleEnum.Hunter))
                    {
                        var hunterRole = Role.GetRole<Hunter>(PlayerControl.LocalPlayer);
                        Object.Destroy(hunterRole.UsesText);
                        hunterRole.ClosestPlayer = null;
                        hunterRole.ClosestStalkPlayer = null;
                        hunterRole.StalkButton.SetTarget(null);
                        hunterRole.StalkButton.gameObject.SetActive(false);
                    }

                    if (!PlayerControl.LocalPlayer.Is(RoleEnum.Investigator) && !PlayerControl.LocalPlayer.Is(RoleEnum.Mystic)
                        && !PlayerControl.LocalPlayer.Is(RoleEnum.Spy)) DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                }

                if (StartImitate.ImitatingPlayer.Is(RoleEnum.Medium))
                {
                    var medRole = Role.GetRole<Medium>(StartImitate.ImitatingPlayer);
                    medRole.MediatedPlayers.Values.DestroyAll();
                    medRole.MediatedPlayers.Clear();
                }

                var role = Role.GetRole(StartImitate.ImitatingPlayer);
                var killsList = (role.Kills, role.CorrectKills, role.IncorrectKills, role.CorrectAssassinKills, role.IncorrectAssassinKills);
                Role.RoleDictionary.Remove(StartImitate.ImitatingPlayer.PlayerId);
                var imitator = new Imitator(StartImitate.ImitatingPlayer);
                imitator.trappedPlayers = trappedPlayers;
                imitator.confessingPlayer = confessingPlayer;
                imitator.LimitedRoleUses = LimitedRoleUses;
                var newRole = Role.GetRole(StartImitate.ImitatingPlayer);
                newRole.RemoveFromRoleHistory(newRole.RoleType);
                newRole.Kills = killsList.Kills;
                newRole.CorrectKills = killsList.CorrectKills;
                newRole.IncorrectKills = killsList.IncorrectKills;
                newRole.CorrectAssassinKills = killsList.CorrectAssassinKills;
                newRole.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
                if (isImmortal && StartImitate.ImitatingPlayer.AmOwner) Utils.UnCamouflage();
                Role.GetRole<Imitator>(StartImitate.ImitatingPlayer).ImitatePlayer = null;
                StartImitate.ImitatingPlayer = null;
            }
            return;
        }
    }
}
