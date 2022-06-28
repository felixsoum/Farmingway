using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using NetStone;

namespace Farmingway
{
    public class NetstoneService
    {
        // private LodestoneClient _client;
        public bool isInit { get; set; }
        
        public async Task Init()
        {
            throw new NotImplementedException("Netstone has not been updated yet");
            // _client = await LodestoneClient.GetClientAsync();
            // isInit = true;
        }

        public async Task<string> GetName(int id)
        {
            throw new NotImplementedException("Netstone has not been updated yet");
            // var character = await _client.GetCharacter(id.ToString());
            // return character.Name;
        }
        
        public async Task<HashSet<int>> GetMountIDs(int id)
        {
            throw new NotImplementedException("Netstone has not been updated yet");
            // var mounts = await _client.GetCharacterMount(id.ToString());
            // return mounts
            //     .Collectables
            //     .Select(c => MountDatabase.GetMountId(c.Name))
            //     .ToHashSet();
        }
    }
}