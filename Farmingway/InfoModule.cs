using Discord.Commands;
using Farmingway.RestResponses;
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
            string characterRequest = $"https://ffxivcollect.com/api/characters/{charId}?ids=true";

            try
            {
                var characterClient = new RestClient(characterRequest);
                var characterResponse = characterClient.Execute<CharacterResponse>(new RestRequest());
                int highestMountID = characterResponse.Data.Mounts.IDs[characterResponse.Data.Mounts.IDs.Length - 1];

                string mountRequest = $"https://ffxivcollect.com/api/mounts/{highestMountID}";
                var mountClient = new RestClient(mountRequest);
                var mountResponse = mountClient.Execute<MountResponse>(new RestRequest());
                var reply = String.Join
                (
                    "\n",
                    $"Name: {characterResponse.Data.Name}",
                    $"Server: {characterResponse.Data.Server}",
                    $"Mounts: {characterResponse.Data.Mounts.Count}",
                    $"Highest Mount ID: {highestMountID}",
                    characterResponse.Data.Portrait,
                    mountResponse.Data.Image

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
