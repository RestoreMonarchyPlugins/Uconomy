using fr34kyn01535.Uconomy.Helpers;
using fr34kyn01535.Uconomy.Models;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace fr34kyn01535.Uconomy.Services
{
    public class ExperienceService : MonoBehaviour
    {
        private Uconomy pluginInstance => Uconomy.Instance;
        private UconomyConfiguration configuration => pluginInstance.Configuration.Instance;

        private Dictionary<CSteamID, bool> ignoreEvent = [];
        public bool IsSynchronizing { get; private set; } = false;

        void Start()
        {
            InvokeRepeating(nameof(SyncPlayersExperience), 0, configuration.SyncIntervalSeconds);
            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            PlayerSkills.OnExperienceChanged_Global += OnExperienceChanged;
        }

        void OnDestroy()
        {
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            PlayerSkills.OnExperienceChanged_Global -= OnExperienceChanged;
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            ignoreEvent[player.CSteamID] = true;
            player.Experience = 0;

            ThreadHelper.RunAsynchronously(() =>
            {
                decimal balance = Uconomy.Instance.Database.GetBalance(player.CSteamID.ToString());
                ThreadHelper.RunSynchronously(() =>
                {
                    ignoreEvent[player.CSteamID] = true;
                    player.Experience = (uint)balance;
                });
            });
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            ignoreEvent.Remove(player.CSteamID);
        }

        private void OnExperienceChanged(PlayerSkills skills, uint lastKnownExperience)
        {
            CSteamID steamID = skills.player.channel.owner.playerID.steamID;
            uint experience = skills.experience;
                        
            if (!ignoreEvent.TryGetValue(steamID, out bool ignore) || ignore)
            {
                ignoreEvent[steamID] = false;
            } else
            {
                int experienceDifference = (int)experience - (int)lastKnownExperience;

                if (experienceDifference != 0)
                {
                    decimal balanceChange = experienceDifference;
                    ThreadHelper.RunAsynchronously(() =>
                    {
                        Uconomy.Instance.Database.IncreaseBalance(steamID.ToString(), balanceChange);
                    });
                }
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
                List<PlayerBalance> playerBalances = Uconomy.Instance.Database.GetBalances(playerIds);
                pluginInstance.LogDebug($"Synchronizing {playerBalances.Count} player experiences.");

                ThreadHelper.RunSynchronously(() =>
                {
                    try
                    {
                        foreach (PlayerBalance playerBalance in playerBalances)
                        {
                            CSteamID steamID = new(Convert.ToUInt64(playerBalance.SteamId));
                            Player player = PlayerTool.getPlayer(steamID);

                            uint experienceBalance = (uint)playerBalance.Balance;
                            if (player.skills.experience == experienceBalance)
                            {
                                continue;
                            }

                            ignoreEvent[steamID] = true;
                            player.skills.ServerSetExperience(experienceBalance);
                        }

                        IsSynchronizing = false;
                        pluginInstance.LogDebug($"Synchronized {playerBalances.Count} player experiences.");
                    } catch (Exception e)
                    {
                        IsSynchronizing = false;
                        Logger.LogException(e);
                    }                    
                });
            });
        }
    }
}
