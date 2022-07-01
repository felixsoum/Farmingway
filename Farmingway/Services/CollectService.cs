using Discord;
using Farmingway.Exceptions;
using Farmingway.RestResponses;
using RestSharp;

namespace Farmingway.Services
{
    /**
     * Easy interaction with the FFXIV Collect API
     */
    public class CollectService
    {
        private static readonly RestClient Client = new RestClient("https://ffxivcollect.com/api");
        
        public static CharacterResponse GetCharacter(int id)
        {
            var request = new RestRequest($"characters/{id}").AddParameter("ids", "true");
            var response = Client.Execute<CharacterResponse>(request);

            if (response.Data == null || response.Data.Id != id)
            {
                throw new NotFoundException(
                    "The specified character was not found. Please ensure it is registered with FFXIV Collect."
                );
            }
            
            return response.Data;
        }

        public static CharacterResponse GetCharacterFromDiscord(IUser user)
        {
            var request = new RestRequest($"users/{user.Id}").AddParameter("ids", "true");
            var response = Client.Execute<CharacterResponse>(request);

            if (response.Data?.Mounts == null)
            {
                throw new NotFoundException(
                    $"{user.Username}#{user.Discriminator} does not have a character registered with FFXIV Collect."
                );
            }
            
            return response.Data;
        }

        public static MountResponse GetMount(int id)
        {
            var request = new RestRequest($"mounts/{id}");
            var response = Client.Execute<MountResponse>(request);
            
            if (response.Data == null || response.Data.Id != id)
            {
                throw new NotFoundException(
                    "The specified mount was not found. Please double-check the mount ID."
                );
            }

            return response.Data;
        }

        public static MountResponse[] GetMounts()
        {
            var request = new RestRequest($"mounts");
            var response = Client.Execute<AllMountResponse>(request);

            if (response.Data == null || response.Data.Results.Length == 0)
            {
                throw new NotFoundException(
                    "Unable to fetch mount data."
                );
            }

            return response.Data.Results;
        }
    }
}
