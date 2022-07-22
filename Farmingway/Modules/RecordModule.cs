using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Farmingway.Modules
{
    public class RecordModule : ModuleBase<SocketCommandContext>
    {
        static Dictionary<string, string> discordIDtoLodestoneID = new();
        static string ConnectionString => $"Server=tcp:farmingway.database.windows.net,1433;Initial Catalog=FarmingwayAlphaDB;Persist Security Info=False;User ID=Recordingway;Password={Secret.DatabasePassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public static string MentionToLodestoneID(string mention)
        {
            string discordID = MentionToDiscordID(mention);
            if (discordIDtoLodestoneID.ContainsKey(discordID))
            {
                return discordIDtoLodestoneID[discordID];
            }
            else
            {
                throw new Exception($"Discord ID {discordID} not found in DB.");
            }
        }

        public static string InitDB()
        {
            discordIDtoLodestoneID.Clear();

            var sb = new StringBuilder();

            try
            {
                string connectionString = ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Users";
                    sb.AppendLine(query);
                    String sql = query;
                    int IDCount = 0;

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                discordIDtoLodestoneID.Add(reader.GetString(0), reader.GetString(1));
                                IDCount++;
                            }
                        }
                    }
                    sb.AppendLine($"DB has been initialized with {IDCount} entries.");
                }
            }
            catch (Exception e)
            {
                sb.AppendLine(e.Message);
            }

            return sb.ToString();
        }

        public static string MentionToDiscordID(string mention)
        {
            if (mention.Length < 3)
            {
                return null;
            }
            return mention.Substring(2, mention.Length - 3);
        }

        [Command("foo")]
        public Task FooBarAsync() => ReplyAsync("Bar!");

        [Command("initdb")]
        public async Task InitDBAsync()
        {
            discordIDtoLodestoneID.Clear();

            var sb = new StringBuilder();

            try
            {
                string connectionString = ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Users";
                    sb.AppendLine(query);
                    String sql = query;
                    int IDCount = 0;

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                discordIDtoLodestoneID.Add(reader.GetString(0), reader.GetString(1));
                                IDCount++;
                            }
                        }
                    }
                    sb.AppendLine($"DB has been initialized with {IDCount} entries.");
                }
            }
            catch (Exception e)
            {
                sb.AppendLine(e.Message);
            }

            await ReplyAsync(sb.ToString());
        }

        [Command("selectusers")]
        public async Task SelectUsersAsync()
        {
            var sb = new StringBuilder();

            try
            {
                string connectionString = ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Users";
                    sb.AppendLine(query);
                    String sql = query;

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sb.AppendLine($"DiscordID: {reader.GetString(0)} -> LodeStoneID: {reader.GetString(1)}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                sb.AppendLine(e.Message);
            }

            await ReplyAsync(sb.ToString());
        }

        [Command("selectcharacters")]
        public async Task SelectCharactersAsync()
        {
            var sb = new StringBuilder();

            try
            {
                string connectionString = ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Characters";
                    sb.AppendLine(query);
                    String sql = query;

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sb.AppendLine($"LodestoneID: {reader.GetString(0)} -> FullName: {reader.GetString(1)}, World: {reader.GetString(2)}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                sb.AppendLine(e.Message);
            }

            await ReplyAsync(sb.ToString());
        }

        [Command("delete")]
        public async Task DeleteAsync(string mention)
        {
            var sb = new StringBuilder();

            try
            {
                string connectionString = ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"DELETE FROM Users WHERE DiscordID={MentionToDiscordID(mention)};";
                    sb.AppendLine(query);
                    String sql = query;

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                sb.AppendLine(e.Message);
            }

            await ReplyAsync(sb.ToString());
        }
    }
}
