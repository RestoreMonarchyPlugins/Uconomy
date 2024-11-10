using fr34kyn01535.Uconomy.Helpers;
using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace fr34kyn01535.Uconomy.Commands
{
    public class CommandBalance : IRocketCommand
    {
        private Uconomy pluginInstance => Uconomy.Instance;

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            string targetId;
            string displayName;

            if (command.Length == 0)
            {
                CheckBalance(caller, caller.Id, caller.DisplayName);
                return;
            }

            if (!caller.HasPermission("balance.other"))
            {
                pluginInstance.SendMessageToPlayer(caller, "command_balance_no_permission");
                return;
            }

            if (command[0].Length == 17 && ulong.TryParse(command[0], out ulong steamId))
            {
                targetId = steamId.ToString();
                displayName = steamId.ToString();
            }
            else
            {
                UnturnedPlayer targetPlayer = UnturnedPlayer.FromName(command[0]);
                if (targetPlayer == null)
                {
                    pluginInstance.SendMessageToPlayer(caller, "command_balance_player_not_found");
                    return;
                }
                targetId = targetPlayer.Id;
                displayName = targetPlayer.DisplayName;
            }

            CheckBalance(caller, targetId, displayName);
        }

        private void CheckBalance(IRocketPlayer caller, string targetId, string displayName)
        {
            ThreadHelper.RunAsynchronously(() =>
            {
                decimal balance = pluginInstance.Database.GetBalance(targetId);
                ThreadHelper.RunSynchronously(() =>
                {
                    string moneyName = pluginInstance.Configuration.Instance.MoneyName;
                    if (caller.Id == targetId)
                    {
                        pluginInstance.SendMessageToPlayer(caller, "command_balance_show", balance, moneyName);
                    }
                    else
                    {
                        pluginInstance.SendMessageToPlayer(caller, "command_balance_other", displayName, balance, moneyName);
                    }
                });
            });
        }

        public string Help => "Shows your balance or another player's balance";
        public string Name => "balance";
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Syntax => "[player/steamId]";
        public List<string> Aliases => ["saldo", "bal"];
        public List<string> Permissions => [];
    }
}
