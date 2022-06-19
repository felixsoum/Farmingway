using Discord.Commands;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace Farmingway
{
    // Create a module with no prefix
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        // ~say hello world -> hello world
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder][Summary("The text to echo")] string echo)
            => ReplyAsync(echo);

        // ReplyAsync is a method on ModuleBase 

        // ~char 931843984 -> Xiaofei Li ...
        [Command("char")]
        [Summary("Prints information about a character")]
        public Task CharAsync([Remainder][Summary("The id of your character")] string charId)
        {
            string request = $"https://ffxivcollect.com/api/characters/{charId}?ids=true";

            try
            {
                var client = new RestClient(request);
                var response = client.Execute<CharacterResponse>(new RestRequest());

                int highestMountID = response.Data.Mounts.IDs[response.Data.Mounts.IDs.Length - 1];
                var reply = String.Join
                (
                    "\n",
                    $"Name: {response.Data.Name}",
                    $"Server: {response.Data.Server}",
                    $"Mounts: {response.Data.Mounts.Count}",
                    $"Highest Mount ID: {highestMountID}",
                    response.Data.Portrait
                );

                return ReplyAsync(reply);
            }
            catch(Exception e)
            {
                Console.Write("ERROR: " + e.Message);
                return ReplyAsync("ERROR: " + e.Message);
            }
        }

        // ReplyAsync is a method on ModuleBase 
    }
}
