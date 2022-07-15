using Farmingway.RestResponses;

namespace Farmingway.Modules.Mounts;

public class MountCount
{
    public int count { get; set; }
    public MountResponse mount { get; set; }
}
