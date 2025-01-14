using fr34kyn01535.Uconomy.Models;
using Rocket.API;

namespace fr34kyn01535.Uconomy
{
    public class UconomyConfiguration : IRocketPluginConfiguration
    {
        public bool Debug;
        public bool ShouldSerializeDebug() => Debug;
        public string MessageColor = "green";
        public string MessageIconUrl = null;
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public int DatabasePort;
        public decimal InitialBalance;
        public string MoneyName;
        public string Comment = "Use SyncExperience only with stable local/network MySQL connections. Otherwise use UconomyXP plugin.";
        public bool SyncExperience = false;
        public float SyncIntervalSeconds = 5;
        public bool EnableSalaries = false;
        public float SalaryIntervalSeconds = 900;
        public SalaryGroup[] SalaryGroups =
        [
            new SalaryGroup("default", 10),
            new SalaryGroup("vip", 30),
            new SalaryGroup("moderator", 50)
        ];

        public void LoadDefaults()
        {
            Debug = false;
            MessageColor = "yellow";
            MessageIconUrl = "https://i.imgur.com/dMDcc9J.png";
            DatabaseAddress = "localhost";
            DatabaseUsername = "unturned";
            DatabasePassword = "password";
            DatabaseName = "unturned";
            DatabaseTableName = "uconomy";
            DatabasePort = 3306;
            InitialBalance = 30;
            MoneyName = "credits";
            SyncExperience = false;
            SyncIntervalSeconds = 5;
            EnableSalaries = false;
            SalaryIntervalSeconds = 900;
            SalaryGroups =
            [
                new SalaryGroup("default", 10),
                new SalaryGroup("vip", 30),
                new SalaryGroup("moderator", 50)
            ];
        }
    }
}