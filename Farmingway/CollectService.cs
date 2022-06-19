using Farmingway.Exceptions;
using Farmingway.RestResponses;
using RestSharp;

namespace Farmingway
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
    }
}
