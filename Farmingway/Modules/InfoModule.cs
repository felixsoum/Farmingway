using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
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
        public async Task FindByUserAsync([Summary("The user requested")] params SocketGuildUser[] users)
        {
            foreach (var user in users)
            {
                try
                {
                    await ReplyAsync(embed: CreateCharacterEmbed(user));
                }
                catch(Exception e)
                {
                    Console.Write("ERROR: " + e.Message);
                    await ReplyAsync(embed: CreateErrorEmbed(e.Message));
                }
            }
            
        }
        
        [Command("find")]
        [Summary("Prints information about a character by Discord username")]
        public async Task FindByUsernameAsync([Summary("The user requested")] params string[] usernames)
        {
            if (usernames.Length == 0)
            {
                await ReplyAsync(embed: CreateErrorEmbed("No user specified"));
                return;
            }

            var multipleResults = new List<string>();
            var noResults = new List<string>();
            var matchedUsers = new List<IGuildUser>();

            foreach (var username in usernames)
            {
                var userList = await (Context.User as IGuildUser).Guild.SearchUsersAsync(username, 5);

                if (userList.Count == 0)
                {
                    noResults.Add(username);
                }
                else if (userList.Count > 1)
                {
                    multipleResults.Add(username);
                }
                else
                {
                    matchedUsers.Add(userList.First());
                }
            }

            if (noResults.Count > 0 || multipleResults.Count > 0)
            {
                var sb = new StringBuilder();
                if (noResults.Count > 0)
                {
                    sb.AppendLine(
                        $"I couldn't find a matching user for the following search: {string.Join(", ", noResults)}"
                    );
                }

                if (multipleResults.Count > 0)
                {
                    sb.AppendLine(
                        $"I found multiple users matching the following search. Please use their full username: {string.Join(", ", multipleResults)}"
                    );
                }

                if (matchedUsers.Count > 0)
                {
                    var matchedUsernames = matchedUsers.Select(u => $"{u.Username}#{u.Discriminator}");
                    sb.AppendLine(
                        $"I was able to find a match for the following users: {string.Join(", ", matchedUsernames)}"
                    );
                }

                await ReplyAsync(embed: CreateErrorEmbed(sb.ToString()));
                return;
            }
            
            foreach (var user in matchedUsers)
            {
                try
                {
                    await ReplyAsync(embed: CreateCharacterEmbed(user));
                }
                catch(Exception e)
                {
                    Console.Write("ERROR: " + e.Message);
                    await ReplyAsync(embed: CreateErrorEmbed(e.Message));
                }
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
            
            var character = CollectService.GetCharacterFromDiscord(user);

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
                    $"Character data for Discord user {user.Username}#{user.Discriminator}",
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

        // !whoshere
        [Command("whoshere")]
        [Summary("Print list of users in thread.")]
        public async Task WhosHereASync()
        {
            var users = Context.Channel.GetUsersAsync();
            var flatUsers = await AsyncEnumerableExtensions.FlattenAsync(users);
            var filteredUsers = flatUsers.Where(x => !x.IsBot).ToList();
            var sb = new StringBuilder();
            sb.AppendLine($"I can find {filteredUsers.Count} users here.");
            foreach (var user in filteredUsers)
            {
                sb.AppendLine(user.Username);
            }
            await ReplyAsync(sb.ToString());
        }

    }
}
