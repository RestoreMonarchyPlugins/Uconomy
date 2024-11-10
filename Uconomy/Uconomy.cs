using fr34kyn01535.Uconomy.Helpers;
using fr34kyn01535.Uconomy.Services;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;

namespace fr34kyn01535.Uconomy
{
    public class Uconomy : RocketPlugin<UconomyConfiguration>
    {
        public DatabaseManager Database;
        public static Uconomy Instance;
        public ExperienceService ExperienceService;
        public SalaryService SalaryService;

        public UnityEngine.Color MessageColor { get; set; }

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, UnityEngine.Color.green);

            Database = new DatabaseManager();
            Database.CheckSchema();

            if (Configuration.Instance.SyncExperience)
            {
                ExperienceService = gameObject.AddComponent<ExperienceService>();
            }

            if (Configuration.Instance.EnableSalaries)
            {
                SalaryService = gameObject.AddComponent<SalaryService>();
            }

            U.Events.OnPlayerConnected += Events_OnPlayerConnected;

            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
            Logger.Log("Check out more Unturned plugins at restoremonarchy.com");
        }

        protected override void Unload()
        {
            Destroy(ExperienceService);
            Destroy(SalaryService);

            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;

            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }

        public delegate void PlayerBalanceUpdate(UnturnedPlayer player, decimal amt);
        public event PlayerBalanceUpdate OnBalanceUpdate;
        public delegate void PlayerBalanceCheck(UnturnedPlayer player, decimal balance);
        public event PlayerBalanceCheck OnBalanceCheck;
        public delegate void PlayerPay(UnturnedPlayer sender, UnturnedPlayer receiver, decimal amt);
        public event PlayerPay OnPlayerPay;

        public override TranslationList DefaultTranslations => new TranslationList() 
        {
            {"command_balance_show", "You have [[b]]{0} {1}[[/b]] in your account"},
            {"command_balance_other", "[[b]]{0}[[/b]] has [[b]]{1} {2}[[/b]] in their account"},
            {"command_balance_no_permission", "You don't have permission to check other players' balances"},
            {"command_balance_player_not_found", "Couldn't find that player - try using their Steam64 ID instead"},
            {"command_pay_invalid", "Please use /pay <playerName> <amount> to send money"},
            {"command_pay_error_pay_self", "You can't send money to yourself!"},
            {"command_pay_error_invalid_amount", "Please enter a valid amount greater than 0"},
            {"command_pay_error_cant_afford", "You don't have enough money in your account for this payment"},
            {"command_pay_error_player_not_found", "Couldn't find that player - make sure you typed their name correctly"},
            {"command_pay_private", "You sent [[b]]{1} {2}[[/b]] to [[b]]{0}[[/b]]"},
            {"command_pay_console", "You received [[b]]{0} {1}[[/b]] from the system"},
            {"command_pay_other_private", "[[b]]{2}[[/b]] sent you [[b]]{0} {1}[[/b]]"},
            {"salary_message", "You earned [[b]]{0} {1}[[/b]] for being [[b]]{2}[[/b]]"}
        };

        internal void HasBeenPayed(UnturnedPlayer sender, UnturnedPlayer receiver, decimal amt)
        {
            if (OnPlayerPay != null)
                OnPlayerPay(sender, receiver, amt);
        }

        internal void BalanceUpdated(string SteamID, decimal amt)
        {
            if (OnBalanceUpdate != null)
            {
                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(SteamID)));
                OnBalanceUpdate(player, amt);
            }
        }

        internal void OnBalanceChecked(string SteamID, decimal balance)
        {
            if (OnBalanceCheck != null)
            {
                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(SteamID)));
                OnBalanceCheck(player, balance);
            }
        }

        internal void LogDebug(string message)
        {
            if (Configuration.Instance.Debug)
            {
                Logger.Log($"Debug >> {message}");
            }
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            ThreadHelper.RunAsynchronously(() =>
            {
                //setup account
                Database.CheckSetupAccount(player.CSteamID);
            });            
        }

        internal void SendMessageToPlayer(IRocketPlayer player, string translationKey, params object[] placeholder)
        {
            string msg = Translate(translationKey, placeholder);
            msg = msg.Replace("[[", "<").Replace("]]", ">");
            if (player is ConsolePlayer)
            {
                Logger.Log(msg);
                return;
            }

            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)player;
            if (unturnedPlayer != null)
            {
                ChatManager.serverSendMessage(msg, MessageColor, null, unturnedPlayer.SteamPlayer(), EChatMode.SAY, Configuration.Instance.MessageIconUrl, true);
            }
        }
    }
}
