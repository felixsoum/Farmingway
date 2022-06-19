using Discord.Commands;
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
            if (!int.TryParse(charId, out var id))
            {
                return ReplyAsync("Character ID was not a number");
            }
            
            try
            {
                var character = CollectService.GetCharacter(id);
                var highestMountID = character.Mounts.IDs[character.Mounts.IDs.Length - 1];

                var mount = CollectService.GetMount(highestMountID);
                var reply = string.Join
                (
                    "\n",
                    $"Name: {character.Name}",
                    $"Server: {character.Server}",
                    $"Mounts: {character.Mounts.Count}",
                    $"Highest Mount ID: {highestMountID}",
                    character.Portrait,
                    mount.Image
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
