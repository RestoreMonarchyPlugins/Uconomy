using fr34kyn01535.Uconomy.Helpers;
using fr34kyn01535.Uconomy.Models;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace fr34kyn01535.Uconomy.Services
{
    public class ExperienceService : MonoBehaviour
    {
        private const uint MAX_EXPERIENCE = uint.MaxValue;
        private const uint MIN_EXPERIENCE = 0;

        private Uconomy pluginInstance => Uconomy.Instance;
        private UconomyConfiguration configuration => pluginInstance.Configuration.Instance;
        public bool IsSynchronizing { get; private set; } = false;

        void Start()
        {
            InvokeRepeating(nameof(SyncPlayersExperience), 0, configuration.SyncIntervalSeconds);
            U.Events.OnPlayerConnected += OnPlayerConnected;
            PlayerSkills.OnExperienceChanged_Global += OnExperienceChanged;
        }

        void OnDestroy()
        {
            CancelInvoke(nameof(SyncPlayersExperience));
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            PlayerSkills.OnExperienceChanged_Global -= OnExperienceChanged;
        }

        private uint ClampExperience(decimal balance)
        {
            if (balance < MIN_EXPERIENCE)
                return MIN_EXPERIENCE;
            if (balance > MAX_EXPERIENCE)
                return MAX_EXPERIENCE;
            return (uint)balance;
        }

        public void SetPlayerExperience(Player player, uint experience)
        {
            PlayerSkills.OnExperienceChanged_Global -= OnExperienceChanged;
            try
            {
                player.skills.ServerSetExperience(experience);
            }
            finally
            {
                PlayerSkills.OnExperienceChanged_Global += OnExperienceChanged;
            }
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            SetPlayerExperience(player.Player, player.Experience);

            ThreadHelper.RunAsynchronously(() =>
            {
                decimal balance = Uconomy.Instance.Database.GetBalance(player.CSteamID.ToString());
                ThreadHelper.RunSynchronously(() =>
                {
                    SetPlayerExperience(player.Player, ClampExperience(balance));
                });
            });
        }

        private void OnExperienceChanged(PlayerSkills skills, uint lastKnownExperience)
        {
            try
            {
                uint experience = skills.experience;
                long experienceDifference = (long)experience - (long)lastKnownExperience;

                if (experienceDifference != 0)
                {
                    decimal balanceChange = experienceDifference;
                    ThreadHelper.RunAsynchronously(() =>
                    {
                        string steamId = skills.player.channel.owner.playerID.steamID.ToString();
                        Uconomy.Instance.Database.IncreaseBalance(steamId, balanceChange);
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void SyncPlayersExperience()
        {
            if (IsSynchronizing)
            {
                pluginInstance.LogDebug("ExperienceService is already synchronizing.");
                return;
            }

            SteamPlayer[] players = PlayerTool.getSteamPlayers();
            if (players.Length == 0)
            {
                return;
            }

            IsSynchronizing = true;
            List<string> playerIds = players.Select(x => x.playerID.steamID.ToString()).ToList();

            ThreadHelper.RunAsynchronously(() =>
            {
                try
                {
                    List<PlayerBalance> playerBalances = Uconomy.Instance.Database.GetBalances(playerIds);
                    pluginInstance.LogDebug($"Synchronizing {playerBalances.Count} player experiences.");

                    ThreadHelper.RunSynchronously(() =>
                    {
                        PlayerSkills.OnExperienceChanged_Global -= OnExperienceChanged;
                        try
                        {
                            foreach (PlayerBalance playerBalance in playerBalances)
                            {
                                CSteamID steamID = new(Convert.ToUInt64(playerBalance.SteamId));
                                Player player = PlayerTool.getPlayer(steamID);

                                if (player == null)
                                    continue;

                                uint experienceBalance = ClampExperience(playerBalance.Balance);
                                if (player.skills.experience == experienceBalance)
                                    continue;

                                player.skills.ServerSetExperience(experienceBalance);
                            }

                            pluginInstance.LogDebug($"Synchronized {playerBalances.Count} player experiences.");
                        }
                        catch (Exception e)
                        {
                            Logger.LogException(e);
                        }
                        finally
                        {
                            PlayerSkills.OnExperienceChanged_Global += OnExperienceChanged;
                            IsSynchronizing = false;
                        }
                    });
                }
                catch (Exception e)
                {
                    IsSynchronizing = false;
                    Logger.LogException(e);
                }
            });
        }
    }
}