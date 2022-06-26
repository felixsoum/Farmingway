using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Farmingway.RestResponses;

namespace Farmingway
{
    public class DiscordUtils
    {
        /// <summary>
        /// Get a list of Discord users from usernames
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="usernames">List of usernames to search for</param>
        /// <returns>List of users matching usernames</returns>
        /// <exception cref="Exception">If multiple and/or no matches are found for a given user</exception>
        public static async Task<List<IGuildUser>> GetUsersFromUsernames(ICommandContext context, string[] usernames)
        {
            if (usernames.Length == 0)
            {
                throw new Exception("No user specified");
            }

            var multipleResults = new List<string>();
            var noResults = new List<string>();
            var matchedUsers = new List<IGuildUser>();

            if (!(context.User is IGuildUser user))
            {
                // Should never happen
                throw new Exception("Could not get user from context");
            }
            
            foreach (var username in usernames)
            {
                var allUsers = await user.Guild.SearchUsersAsync(username, 5);
                
                // Filter out bots and the user that used the command
                var userList = allUsers.Where(u => !u.IsBot && u.Id != user.Id).ToList();

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

                throw new Exception(sb.ToString());
            }

            return matchedUsers;
        }
        
        /// <summary>
        /// Create an embed with a Discord user's character details
        /// </summary>
        /// <param name="user">The Discord user to build an embed for</param>
        /// <returns>An embed containing a character's name, home server, number of mounts, and mount with the highest ID</returns>
        public static Embed CreateCharacterEmbed(IUser user)
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
        public static Embed CreateErrorEmbed(string message)
        {
            var builder = new EmbedBuilder();

            builder.WithColor(new Color(255, 0, 0))
                .WithTitle("Error")
                .WithDescription(message);

            return builder.Build();
        }
    }
}