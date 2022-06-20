using Discord.Commands;
using System;
using System.Linq;
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
        
        [Command("findbyusername")]
        [Summary("Prints information about a character by Discord username")]
        public Task FindByUsernameAsync([Summary("The user requested")] string username)
        {
            var embed = new EmbedBuilder();
            //(Context.Channel as IGuildChannel).Guild.GetUserAsync() looking into getting users a different way
            var userList = Context.Guild.Users 
                .Where(u => 
                    string.Equals(u.Username, username, StringComparison.CurrentCultureIgnoreCase)
                    || string.Equals(u.Nickname, username, StringComparison.CurrentCultureIgnoreCase)
                )
                .ToList();

            if (userList.Count() != 1)
            {
                embed.WithColor(new Color(255, 0, 0))
                    .WithTitle("ERROR");

                embed.WithDescription(
                    !userList.Any()
                    ? "Could not find user"
                    : "Found multiple users (this will be turned into a prompt eventually"
                );

                return ReplyAsync(embed: embed.Build());
            }
            
            try
            {
                var user = userList.First();
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
