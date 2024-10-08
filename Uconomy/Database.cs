﻿using fr34kyn01535.Uconomy.Helpers;
using fr34kyn01535.Uconomy.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fr34kyn01535.Uconomy
{
    public class DatabaseManager
    {
        private MySqlConnection createConnection()
        {
            MySqlConnection connection = null;
            if (Uconomy.Instance.Configuration.Instance.DatabasePort == 0) Uconomy.Instance.Configuration.Instance.DatabasePort = 3306;
            connection = new MySqlConnection(String.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", Uconomy.Instance.Configuration.Instance.DatabaseAddress, Uconomy.Instance.Configuration.Instance.DatabaseName, Uconomy.Instance.Configuration.Instance.DatabaseUsername, Uconomy.Instance.Configuration.Instance.DatabasePassword, Uconomy.Instance.Configuration.Instance.DatabasePort));

            return connection;
        }

        /// <summary>
        /// returns the current balance of an account
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public decimal GetBalance(string id)
        {
            decimal output = 0;
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "select `balance` from `" + Uconomy.Instance.Configuration.Instance.DatabaseTableName + "` where `steamId` = '" + id.ToString() + "';";
            connection.Open();
            object result = command.ExecuteScalar();
            if (result != null) Decimal.TryParse(result.ToString(), out output);
            connection.Close();
            Uconomy.Instance.OnBalanceChecked(id, output);

            return output;
        }

        /// <summary>
        /// Increasing balance to increaseBy (can be negative)
        /// </summary>
        /// <param name="steamId">steamid of the accountowner</param>
        /// <param name="increaseBy">amount to change</param>
        /// <returns>the new balance</returns>
        public decimal IncreaseBalance(string id, decimal increaseBy)
        {
            decimal output = 0;
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "update `" + Uconomy.Instance.Configuration.Instance.DatabaseTableName + "` set `balance` = balance + (" + increaseBy + ") where `steamId` = '" + id.ToString() + "'; select `balance` from `" + Uconomy.Instance.Configuration.Instance.DatabaseTableName + "` where `steamId` = '" + id.ToString() + "'";
            connection.Open();
            object result = command.ExecuteScalar();
            if (result != null) Decimal.TryParse(result.ToString(), out output);
            connection.Close();
            ThreadHelper.RunSynchronously(() =>
            {
                Uconomy.Instance.BalanceUpdated(id, increaseBy);
            });
            return output;
        }

        
        public void CheckSetupAccount(Steamworks.CSteamID id)
        {
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            int exists = 0;
            command.CommandText = "SELECT EXISTS(SELECT 1 FROM `" + Uconomy.Instance.Configuration.Instance.DatabaseTableName + "` WHERE `steamId` ='" + id + "' LIMIT 1);";
            connection.Open();
            object result = command.ExecuteScalar();
            if (result != null) Int32.TryParse(result.ToString(), out exists);
            connection.Close();

            if (exists == 0)
            {
                command.CommandText = "insert ignore into `" + Uconomy.Instance.Configuration.Instance.DatabaseTableName + "` (balance,steamId,lastUpdated) values(" + Uconomy.Instance.Configuration.Instance.InitialBalance + ",'" + id.ToString() + "',now())";
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public List<PlayerBalance> GetBalances(List<string> ids)
        {
            List<PlayerBalance> balances = new List<PlayerBalance>();
            using (MySqlConnection connection = createConnection())
            {
                string placeholders = string.Join(",", ids.Select(id => "?id" + id));
                string query = $"SELECT `steamId`, `balance` FROM `{Uconomy.Instance.Configuration.Instance.DatabaseTableName}` WHERE `steamId` IN ({placeholders})";

                MySqlCommand command = new MySqlCommand(query, connection);

                for (int i = 0; i < ids.Count; i++)
                {
                    command.Parameters.AddWithValue("?id" + ids[i], ids[i]);
                }

                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        balances.Add(new PlayerBalance
                        {
                            SteamId = reader.GetString("steamId"),
                            Balance = reader.GetDecimal("balance")
                        });
                    }
                }
            }

            ThreadHelper.RunSynchronously(() =>
            {
                // Trigger the event for each balance checked
                foreach (var balance in balances)
                {
                    Uconomy.Instance.OnBalanceChecked(balance.SteamId, balance.Balance);
                }
            });

            return balances;
        }

        internal void CheckSchema()
        {
            MySqlConnection connection = createConnection();
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "show tables like '" + Uconomy.Instance.Configuration.Instance.DatabaseTableName + "'";
            connection.Open();
            object test = command.ExecuteScalar();

            if (test == null)
            {
                command.CommandText = "CREATE TABLE `" + Uconomy.Instance.Configuration.Instance.DatabaseTableName + "` (`steamId` varchar(32) NOT NULL,`balance` decimal(15,2) NOT NULL DEFAULT '25.00',`lastUpdated` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,PRIMARY KEY (`steamId`)) ";
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
}
