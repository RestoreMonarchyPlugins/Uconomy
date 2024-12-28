using fr34kyn01535.Uconomy.Helpers;
using fr34kyn01535.Uconomy.Models;
using Rocket.API.Serialisation;
using Rocket.Core;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace fr34kyn01535.Uconomy.Services
{
    public class SalaryService : MonoBehaviour
    {
        private Uconomy pluginInstance => Uconomy.Instance;
        private readonly Dictionary<CSteamID, Coroutine> coroutines = new();

        void Start()
        {
            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
        }

        void OnDestroy()
        {
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            StopAllCoroutines();
        }


        private void OnPlayerConnected(UnturnedPlayer player)
        {
            if (Uconomy.Instance.Configuration.Instance.EnableSalaries)
            {
                var coroutine = StartCoroutine(GiveSalary(player.CSteamID));
                coroutines[player.CSteamID] = coroutine;
            }
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            if (coroutines.ContainsKey(player.CSteamID))
            {
                StopCoroutine(coroutines[player.CSteamID]);
                coroutines.Remove(player.CSteamID);
            }
        }

        private IEnumerator GiveSalary(CSteamID steamID)
        {
            while (true)
            {
                yield return new WaitForSeconds(Uconomy.Instance.Configuration.Instance.SalaryIntervalSeconds);

                Player player = PlayerTool.getPlayer(steamID);
                if (player == null)
                {
                    coroutines.Remove(steamID);
                    yield break;
                }

                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromPlayer(player);
                List<RocketPermissionsGroup> groups = R.Permissions.GetGroups(unturnedPlayer, true);

                foreach (SalaryGroup salaryGroup in pluginInstance.Configuration.Instance.SalaryGroups.OrderByDescending(x => x.Amount))
                {
                    RocketPermissionsGroup group = groups.FirstOrDefault(g => g.Id.Equals(salaryGroup.GroupId));
                    if (group != null)
                    {
                        ThreadHelper.RunAsynchronously(() =>
                        {
                            pluginInstance.Database.IncreaseBalance(unturnedPlayer.Id, salaryGroup.Amount);
                            ThreadHelper.RunSynchronously(() =>
                            {
                                string amount = salaryGroup.Amount.ToString("N");
                                string moneyName = pluginInstance.Configuration.Instance.MoneyName;
                                string groupName = group.DisplayName;
                                pluginInstance.SendMessageToPlayer(unturnedPlayer, "salary_message", amount, moneyName, groupName);
                            });
                        });
                        break;
                    }
                }
            }
        }
    }
}
