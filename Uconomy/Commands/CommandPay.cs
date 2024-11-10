using fr34kyn01535.Uconomy.Helpers;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace fr34kyn01535.Uconomy.Commands
{
    public class CommandPay : IRocketCommand
    {
        private Uconomy pluginInstance => Uconomy.Instance;

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (command.Length != 2)
            {
                pluginInstance.SendMessageToPlayer(caller, "command_pay_invalid");
                return;
            }

            UnturnedPlayer otherPlayer = UnturnedPlayer.FromName(command[0]);
            string steamId = null;
            string displayName = null;

            if (otherPlayer == null)
            {
                if (ulong.TryParse(command[0], out ulong steamIdLong))
                {
                    steamId = steamIdLong.ToString();
                    displayName = steamIdLong.ToString();
                }                
            } else
            {
                steamId = otherPlayer.Id;
                displayName = otherPlayer.DisplayName;
            }

            if (steamId != null)
            {
                if (caller.Id == steamId)
                {
                    pluginInstance.SendMessageToPlayer(caller, "command_pay_error_pay_self");
                    return;
                }

                decimal amount = 0;
                if (!decimal.TryParse(command[1], out amount) || amount <= 0)
                {
                    pluginInstance.SendMessageToPlayer(caller, "command_pay_error_invalid_amount");
                    return;
                }

                ThreadHelper.RunAsynchronously(() =>
                {
                    if (caller is ConsolePlayer)
                    {
                        pluginInstance.Database.IncreaseBalance(steamId, amount);
                        ThreadHelper.RunSynchronously(() =>
                        {
                            string moneyName = pluginInstance.Configuration.Instance.MoneyName;
                            Logger.Log($"{caller.DisplayName} paid {displayName} {amount} {moneyName}.");
                            if (otherPlayer != null)
                            {
                                pluginInstance.SendMessageToPlayer(otherPlayer, "command_pay_console", amount, moneyName);
                            }                                
                        });
                    }
                    else
                    {
                        decimal myBalance = pluginInstance.Database.GetBalance(caller.Id);
                        if (myBalance - amount <= 0)
                        {
                            ThreadHelper.RunSynchronously(() =>
                            {
                                pluginInstance.SendMessageToPlayer(caller, "command_pay_error_cant_afford");
                            });
                            return;
                        }
                        else
                        {
                            pluginInstance.Database.IncreaseBalance(caller.Id, -amount);
                            pluginInstance.Database.IncreaseBalance(steamId, amount);

                            ThreadHelper.RunSynchronously(() =>
                            {
                                string moneyName = pluginInstance.Configuration.Instance.MoneyName;
                                pluginInstance.SendMessageToPlayer(caller, "command_pay_private", displayName, amount, moneyName);
                                if (otherPlayer != null)
                                {
                                    pluginInstance.SendMessageToPlayer(otherPlayer, "command_pay_other_private", amount, moneyName, caller.DisplayName);
                                    pluginInstance.HasBeenPayed((UnturnedPlayer)caller, otherPlayer, amount);
                                }                                
                            });
                        }
                    }
                });
            }
            else
            {
                pluginInstance.SendMessageToPlayer(caller, "command_pay_error_player_not_found");
            }
        }

        public string Help => "Pays a specific player money from your account";

        public string Name => "pay";

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Syntax => "<player> <amount>";

        public List<string> Aliases => new List<string> { "pagar" };

        public List<string> Permissions => new List<string>() { "uconomy.pay" };
    }
}
