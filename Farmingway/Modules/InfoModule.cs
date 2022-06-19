using Discord.Commands;
using System;
using System.Threading.Tasks;
using Discord;

namespace Farmingway.Modules
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
        public Task CharAsync([Remainder][Summary("The id of your character")] int charId)
        {
            var embed = new EmbedBuilder();
            
            try
            {
                var character = CollectService.GetCharacter(charId);
                var highestMountID = character.Mounts.IDs[character.Mounts.IDs.Length - 1];

                var mount = CollectService.GetMount(highestMountID);

                embed.WithColor(new Color(0, 255, 0))
                    .WithTitle($"Character data for ID {charId}")
                    .AddField("Name", character.Name)
                    .AddField("Server", character.Server)
                    .AddField("Mounts", character.Mounts.Count)
                    .AddField("Highest Mount", $"{mount.Name} (ID {mount.Id})")
                    .WithImageUrl(character.Portrait);
            }
            catch(Exception e)
            {
                Console.Write("ERROR: " + e.Message);
                embed.WithColor(new Color(255, 0, 0))
                    .WithTitle("ERROR")
                    .WithDescription(e.Message);
            }

            return ReplyAsync(embed: embed.Build());
        }

        [Command("find")]
        [Summary("Prints information about a character by Discord username")]
        public Task FindAsync([Remainder][Summary("The user requested")] IUser user)
        {
            var embed = new EmbedBuilder();
            
            try
            {
                var character = CollectService.GetCharacterFromDiscord(user.Id);
                var highestMountID = character.Mounts.IDs[character.Mounts.IDs.Length - 1];

                var mount = CollectService.GetMount(highestMountID);

                embed.WithColor(new Color(0, 255, 0))
                    .WithAuthor(
                        $"Character data for Discord user {user.Username}#{user.DiscriminatorValue:D4}",
                        user.GetAvatarUrl(size: 256)
                    )
                    .AddField("Name", character.Name)
                    .AddField("Server", character.Server)
                    .AddField("Mounts", character.Mounts.Count)
                    .AddField("Highest Mount", $"{mount.Name} (ID {mount.Id})")
                    .WithImageUrl(character.Portrait);
            }
            catch(Exception e)
            {
                Console.Write("ERROR: " + e.Message);
                embed.WithColor(new Color(255, 0, 0))
                    .WithTitle("ERROR")
                    .WithDescription(e.Message);
            }

            return ReplyAsync(embed: embed.Build());
        }

        // ReplyAsync is a method on ModuleBase 
    }
}
