using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Farmingway.RestResponses;

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
        public Task FindByUserAsync([Remainder][Summary("The user requested")] IUser user)
        {
            try
            {
                return ReplyAsync(embed: CreateCharacterEmbed(user));
            }
            catch(Exception e)
            {
                Console.Write("ERROR: " + e.Message);
                return ReplyAsync(embed: CreateErrorEmbed(e.Message));
            }
        }
        
        [Command("find")]
        [Summary("Prints information about a character by Discord username")]
        public async Task FindByUsernameAsync([Remainder][Summary("The user requested")] string username)
        {
            var userList = await (Context.User as IGuildUser).Guild.SearchUsersAsync(username);

            if (userList.Count != 1)
            { 
                await ReplyAsync(embed: CreateErrorEmbed(
                    userList.Count == 0
                        ? "Could not find user"
                        : "Found multiple users (this will be turned into a prompt eventually)"
                ));
                return;
            }
            
            try
            {
                var user = userList.First();
                await ReplyAsync(embed: CreateCharacterEmbed(user));
            }
            catch(Exception e)
            {
                Console.Write("ERROR: " + e.Message);
                await ReplyAsync(embed: CreateErrorEmbed(e.Message));
            }
        }

        /// <summary>
        /// Create an embed with a Discord user's character details
        /// </summary>
        /// <param name="user">The Discord user to build an embed for</param>
        /// <returns>An embed containing a character's name, home server, number of mounts, and mount with the highest ID</returns>
        private static Embed CreateCharacterEmbed(IUser user)
        {
            var builder = new EmbedBuilder();
            
            var character = CollectService.GetCharacterFromDiscord(user.Id);

            MountResponse mount;
            try
            {
                var highestMountID = character.Mounts.IDs.Last();
                mount = CollectService.GetMount(highestMountID);
            }
            catch (InvalidOperationException)
            {
                // User has no mounts
                mount = null;
            }
            
            builder.WithColor(new Color(0, 255, 0))
                .WithAuthor(
                    $"Character data for Discord user {user.Username}#{user.DiscriminatorValue:D4}",
                    user.GetAvatarUrl()
                )
                .AddField("Name", character.Name)
                .AddField("Server", character.Server)
                .AddField("Mounts", character.Mounts.Count)
                .AddField(
                    "Highest Mount",  
                    mount == null 
                        ? "No mounts found" 
                        : $"{mount.Name} (ID {mount.Id})"
                )
                .WithImageUrl(character.Portrait);

            return builder.Build();
        }

        /// <summary>
        /// Create an embed with error details
        /// </summary>
        /// <param name="message">The error message to include in the embed</param>
        /// <returns>An embed with details about an internal error</returns>
        private static Embed CreateErrorEmbed(string message)
        {
            var builder = new EmbedBuilder();

            builder.WithColor(new Color(255, 0, 0))
                .WithTitle("Error")
                .WithDescription(message);

            return builder.Build();
        }

        // ReplyAsync is a method on ModuleBase 
    }
}
