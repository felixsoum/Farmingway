using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Farmingway.Modules
{
    public class RecordModule : ModuleBase<SocketCommandContext>
    {
        [Command("foo")]
        public Task FooBarAsync() => ReplyAsync("Bar!");

        [Command("testdb")]
        public async Task TestDBAsync()
        {
            var sb = new StringBuilder();

            try
            {
                string connectionString = $"Server=tcp:farmingway.database.windows.net,1433;Initial Catalog=FarmingwayAlphaDB;Persist Security Info=False;User ID=Recordingway;Password={Secret.DatabasePassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

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
    }
}
