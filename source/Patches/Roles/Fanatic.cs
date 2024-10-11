using System;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using Reactor.Utilities;
using TownOfUs.CrewmateRoles.AurialMod;
using TownOfUs.CrewmateRoles.ImitatorMod;
using TownOfUs.CrewmateRoles.InvestigatorMod;
using TownOfUs.CrewmateRoles.TrapperMod;
using TownOfUs.Extensions;
using TownOfUs.Patches.ScreenEffects;
using TownOfUs.Roles.Modifiers;
using UnityEngine;

namespace TownOfUs.Roles
{
    public class Fanatic : Role
    {
        public Fanatic(PlayerControl player) : base(player)
        {
            Name = "Fanatic";
            ImpostorText = () => "Grow Your Cult And Kill The Rest";
            TaskText = () => "Indoctrinate and kill\nFake Tasks:";
            Color = Patches.Colors.Fanatic;
            LastKilled = DateTime.UtcNow;
            RoleType = RoleEnum.Fanatic;
            Faction = Faction.NeutralKilling;
            AddToRoleHistory(RoleType);
        }

        private KillButton _indoctrinateButton;
        public PlayerControl ClosestPlayer;
        public PlayerControl ConvertingPlayer;
        public DateTime LastKilled { get; set; }
        public bool WasConverted { get; set; }

        public KillButton IndoctrinateButton
        {
            get => _indoctrinateButton;
            set
            {
                _indoctrinateButton = value;
                ExtraButtons.Clear();
                ExtraButtons.Add(value);
            }
        }

        public float KillTimer()
        {
            var utcNow = DateTime.UtcNow;
            var timeSpan = utcNow - LastKilled;
            var num = CustomGameOptions.FanaticKillCd * 1000f;
            var flag2 = num - (float) timeSpan.TotalMilliseconds < 0f;
            if (flag2) return 0;
            return (num - (float) timeSpan.TotalMilliseconds) / 1000f;
        }
        
        internal override bool GameEnd(LogicGameFlowNormal __instance)
        {
            if (Player.Data.IsDead || Player.Data.Disconnected) return true;

            var fanaticsAlive = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => !x.Data.IsDead && !x.Data.Disconnected && x.Is(RoleEnum.Fanatic)).ToList();

            if (PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected) <= 2 &&
                    PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected &&
                    (x.Data.IsImpostor() || x.Is(Faction.NeutralKilling) || x.IsCrewKiller())) == 1)
            {
                VampWin();
                Utils.EndGame();
                return false;
            }
            else if (PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected) <= 4 &&
                    PlayerControl.AllPlayerControls.ToArray().Count(x => !x.Data.IsDead && !x.Data.Disconnected &&
                    (x.Data.IsImpostor() || x.Is(Faction.NeutralKilling) || x.IsCrewKiller()) && !x.Is(RoleEnum.Fanatic)) == 0)
            {
                if (fanaticsAlive.Count == 1) return false;
                foreach (var fanatic in fanaticsAlive)
                {
                    if (fanatic.IsLover()) return false;
                }
                VampWin();
                Utils.EndGame();
                return false;
            }
            else
            {
                if (fanaticsAlive.Count == 1 || fanaticsAlive.Count == 2) return false;
                var alives = PlayerControl.AllPlayerControls.ToArray()
                    .Where(x => !x.Data.IsDead && !x.Data.Disconnected).ToList();
                var killersAlive = PlayerControl.AllPlayerControls.ToArray()
                    .Where(x => !x.Data.IsDead && !x.Data.Disconnected && !x.Is(RoleEnum.Fanatic) && (x.Is(Faction.Impostors) || x.Is(Faction.NeutralKilling) || x.IsCrewKiller())).ToList();
                if (killersAlive.Count > 0) return false;
                foreach (var fanatic in fanaticsAlive)
                {
                    if (fanatic.IsLover()) return false;
                }
                if (alives.Count <= 6)
                {
                    VampWin();
                    Utils.EndGame();
                    return false;
                }
                return false;
            }
        }

        protected override void IntroPrefix(IntroCutscene._ShowTeam_d__38 __instance)
        {
            var fanaticTeam = new List<PlayerControl>();
            fanaticTeam.Add(PlayerControl.LocalPlayer);
            __instance.teamToShow = fanaticTeam;
        }

        public void ConvertPlayer(PlayerControl newFanatic)
        {
            var oldRole = GetRole(newFanatic);
            var killsList = (oldRole.CorrectKills, oldRole.IncorrectKills, oldRole.CorrectAssassinKills, oldRole.IncorrectAssassinKills);

            if (newFanatic.Is(RoleEnum.Snitch))
            {
                var snitch = GetRole<Snitch>(newFanatic);
                snitch.SnitchArrows.Values.DestroyAll();
                snitch.SnitchArrows.Clear();
                snitch.ImpArrows.DestroyAll();
                snitch.ImpArrows.Clear();
            }

            if (newFanatic == StartImitate.ImitatingPlayer) StartImitate.ImitatingPlayer = null;

            if (newFanatic.Is(RoleEnum.GuardianAngel))
            {
                var ga = GetRole<GuardianAngel>(newFanatic);
                ga.UnProtect();
            }

            if (newFanatic.Is(RoleEnum.Medium))
            {
                var medRole = GetRole<Medium>(newFanatic);
                medRole.MediatedPlayers.Values.DestroyAll();
                medRole.MediatedPlayers.Clear();
            }

            if (PlayerControl.LocalPlayer == newFanatic)
            {
                Coroutines.Start(Utils.FlashCoroutine(Color));
                if (PlayerControl.LocalPlayer.Is(RoleEnum.Investigator)) Footprint.DestroyAll(GetRole<Investigator>(PlayerControl.LocalPlayer));

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Sheriff)) HudManager.Instance.KillButton.buttonLabelText.gameObject.SetActive(false);

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Engineer))
                {
                    var engineerRole = GetRole<Engineer>(PlayerControl.LocalPlayer);
                    UnityEngine.Object.Destroy(engineerRole.UsesText);
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Tagger))
                {
                    var trackerRole = GetRole<Tagger>(PlayerControl.LocalPlayer);
                    trackerRole.TaggerArrows.Values.DestroyAll();
                    trackerRole.TaggerArrows.Clear();
                    UnityEngine.Object.Destroy(trackerRole.UsesText);
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Mystic))
                {
                    var mysticRole = GetRole<Mystic>(PlayerControl.LocalPlayer);
                    mysticRole.BodyArrows.Values.DestroyAll();
                    mysticRole.BodyArrows.Clear();
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Transporter))
                {
                    var transporterRole = GetRole<Transporter>(PlayerControl.LocalPlayer);
                    UnityEngine.Object.Destroy(transporterRole.UsesText);
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Veteran))
                {
                    var veteranRole = GetRole<Veteran>(PlayerControl.LocalPlayer);
                    UnityEngine.Object.Destroy(veteranRole.UsesText);
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Trapper))
                {
                    var trapperRole = GetRole<Trapper>(PlayerControl.LocalPlayer);
                    UnityEngine.Object.Destroy(trapperRole.UsesText);
                    trapperRole.traps.ClearTraps();
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Detective))
                {
                    var detecRole = GetRole<Detective>(PlayerControl.LocalPlayer);
                    detecRole.ExamineButton.gameObject.SetActive(false);
                    foreach (GameObject scene in detecRole.CrimeScenes)
                    {
                        UnityEngine.Object.Destroy(scene);
                    }
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Aurial))
                {
                    var aurialRole = GetRole<Aurial>(PlayerControl.LocalPlayer);
                    aurialRole.SenseArrows.Clear();
                    CameraEffect.singleton.materials.Clear();
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Survivor))
                {
                    var survRole = GetRole<Survivor>(PlayerControl.LocalPlayer);
                    UnityEngine.Object.Destroy(survRole.UsesText);
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.GuardianAngel))
                {
                    var gaRole = GetRole<GuardianAngel>(PlayerControl.LocalPlayer);
                    UnityEngine.Object.Destroy(gaRole.UsesText);
                }

                if (PlayerControl.LocalPlayer.Is(RoleEnum.Hunter))
                {
                    var hunterRole = Role.GetRole<Hunter>(PlayerControl.LocalPlayer);
                    hunterRole.StalkButton.gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(hunterRole.UsesText);
                }
            }

            RoleDictionary.Remove(newFanatic.PlayerId);

            var role = new Fanatic(newFanatic);
            role.CorrectKills = killsList.CorrectKills;
            role.IncorrectKills = killsList.IncorrectKills;
            role.CorrectAssassinKills = killsList.CorrectAssassinKills;
            role.IncorrectAssassinKills = killsList.IncorrectAssassinKills;
            role.WasConverted = true;
            if (PlayerControl.LocalPlayer == newFanatic)
            {
                role.RegenTask();
                if (Minigame.Instance)
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }

                if (MapBehaviour.Instance)
                {
                    MapBehaviour.Instance.Close();
                    MapBehaviour.Instance.Close();
                }
                if (oldRole.RoleType == RoleEnum.Immortal && (!CamouflageUnCamouflage.CommsEnabled || !CustomGameOptions.ColourblindComms))
                {
                    Utils.UnCamouflage();
                }
            }

            if (CustomGameOptions.NewFanaticCanAssassin) new Assassin(newFanatic);
            ConvertingPlayer = null;
        }
        internal override bool RoleCriteria()
        {
            if (RoleType == RoleEnum.Fanatic && PlayerControl.LocalPlayer.Is(RoleEnum.Fanatic)) return true;
            return false;
        }
    }
}