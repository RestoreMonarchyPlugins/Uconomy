using Rocket.API;

namespace fr34kyn01535.Uconomy
{
    public class UconomyConfiguration : IRocketPluginConfiguration
    {
        public bool Debug;
        public bool ShouldSerializeDebug() => Debug;
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public int DatabasePort;

        public decimal InitialBalance;
        public string MoneyName;
        public bool SyncExperience = false;
        public float SyncIntervalSeconds = 5;

        public void LoadDefaults()
        {
            Debug = false;
            DatabaseAddress = "localhost";
            DatabaseUsername = "unturned";
            DatabasePassword = "password";
            DatabaseName = "unturned";
            DatabaseTableName = "uconomy";
            DatabasePort = 3306;
            InitialBalance = 30;
            MoneyName = "Credits";
            SyncExperience = false;
            SyncIntervalSeconds = 5;
        }
    }
}
