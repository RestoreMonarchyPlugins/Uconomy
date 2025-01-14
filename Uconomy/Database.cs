using fr34kyn01535.Uconomy.Helpers;
using fr34kyn01535.Uconomy.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace fr34kyn01535.Uconomy
{
    public class DatabaseManager
    {
        private readonly string _connectionString;
        private const string TABLE_PLACEHOLDER = "Uconomy";

        public DatabaseManager()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
            {
                Server = Uconomy.Instance.Configuration.Instance.DatabaseAddress,
                Database = Uconomy.Instance.Configuration.Instance.DatabaseName,
                UserID = Uconomy.Instance.Configuration.Instance.DatabaseUsername,
                Password = Uconomy.Instance.Configuration.Instance.DatabasePassword,
                Port = (uint)(Uconomy.Instance.Configuration.Instance.DatabasePort == 0 ? 3306 : Uconomy.Instance.Configuration.Instance.DatabasePort),
                ConnectionTimeout = 30,
                DefaultCommandTimeout = 30
            };

            _connectionString = builder.ConnectionString;
        }

        private string Query(string sql)
        {
            if (Uconomy.Instance.Configuration.Instance.Debug)
            {
                int random = new Random().Next(0, 1000);
                Thread.Sleep(random);
            }

            return sql.Replace(TABLE_PLACEHOLDER, Uconomy.Instance.Configuration.Instance.DatabaseTableName);
        }

        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public decimal GetBalance(string id)
        {
            const string sql = "SELECT `balance` FROM `Uconomy` WHERE `steamId` = @steamId";

            MySqlConnection connection = CreateConnection();
            MySqlCommand command = new MySqlCommand(Query(sql), connection);
            command.Parameters.Add("@steamId", MySqlDbType.VarChar, 32).Value = id;

            connection.Open();
            object result = command.ExecuteScalar();
            connection.Close();

            decimal balance = result != null && decimal.TryParse(result.ToString(), out decimal parsed)
                ? parsed
                : 0m;

            Uconomy.Instance.OnBalanceChecked(id, balance);
            return balance;
        }

        public decimal IncreaseBalance(string id, decimal increaseBy)
        {
            const string sql = @"
                UPDATE `Uconomy` 
                SET `balance` = balance + @increaseAmount 
                WHERE `steamId` = @steamId;
                
                SELECT `balance` 
                FROM `Uconomy` 
                WHERE `steamId` = @steamId";

            MySqlConnection connection = CreateConnection();
            MySqlCommand command = new MySqlCommand(Query(sql), connection);
            command.Parameters.Add("@steamId", MySqlDbType.VarChar, 32).Value = id;
            command.Parameters.Add("@increaseAmount", MySqlDbType.Decimal).Value = increaseBy;

            connection.Open();
            object result = command.ExecuteScalar();
            connection.Close();

            decimal balance = result != null && decimal.TryParse(result.ToString(), out decimal parsed)
                ? parsed
                : 0m;

            ThreadHelper.RunSynchronously(() =>
            {
                Uconomy.Instance.BalanceUpdated(id, increaseBy);
            });

            return balance;
        }

        public void CheckSetupAccount(Steamworks.CSteamID id)
        {
            const string sql = @"
                INSERT IGNORE INTO `Uconomy` 
                    (balance, steamId, lastUpdated) 
                VALUES 
                    (@initialBalance, @steamId, NOW())";

            MySqlConnection connection = CreateConnection();
            MySqlCommand command = new MySqlCommand(Query(sql), connection);
            command.Parameters.Add("@steamId", MySqlDbType.VarChar, 32).Value = id.ToString();
            command.Parameters.Add("@initialBalance", MySqlDbType.Decimal).Value =
                Uconomy.Instance.Configuration.Instance.InitialBalance;

            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        public List<PlayerBalance> GetBalances(List<string> ids)
        {
            if (ids == null || !ids.Any())
                return new List<PlayerBalance>();

            List<PlayerBalance> balances = new List<PlayerBalance>();
            List<string> parameters = ids.Select((id, index) => $"@id{index}").ToList();

            string sql = $@"
                SELECT `steamId`, `balance` 
                FROM `Uconomy` 
                WHERE `steamId` IN ({string.Join(",", parameters)})";

            MySqlConnection connection = CreateConnection();
            MySqlCommand command = new MySqlCommand(Query(sql), connection);

            for (int i = 0; i < ids.Count; i++)
            {
                command.Parameters.Add($"@id{i}", MySqlDbType.VarChar, 32).Value = ids[i];
            }

            connection.Open();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                balances.Add(new PlayerBalance
                {
                    SteamId = reader.GetString("steamId"),
                    Balance = reader.GetDecimal("balance")
                });
            }

            reader.Close();
            connection.Close();

            ThreadHelper.RunSynchronously(() =>
            {
                foreach (PlayerBalance balance in balances)
                {
                    Uconomy.Instance.OnBalanceChecked(balance.SteamId, balance.Balance);
                }
            });

            return balances;
        }

        public void CheckSchema()
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS `Uconomy` (
                    `steamId` VARCHAR(32) NOT NULL,
                    `balance` DECIMAL(15,2) NOT NULL DEFAULT '25.00',
                    `lastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    PRIMARY KEY (`steamId`)
                )";

            MySqlConnection connection = CreateConnection();
            MySqlCommand command = new MySqlCommand(Query(sql), connection);
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}