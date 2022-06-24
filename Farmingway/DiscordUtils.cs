using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

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

            foreach (var username in usernames)
            {
                var userList = await (context.User as IGuildUser).Guild.SearchUsersAsync(username, 5);

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
    }
}