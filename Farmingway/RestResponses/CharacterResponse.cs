using Newtonsoft.Json;

namespace Farmingway.RestResponses
{
    public class CharacterResponse
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "server")]
        public string Server { get; set; }

        [JsonProperty(PropertyName = "portrait")]
        public string Portrait { get; set; }

        [JsonProperty(PropertyName = "avatar")]
        public string Avatar { get; set; }

        [JsonProperty(PropertyName = "achievements")]
        public CharacterResponseAchievements Achievements { get; set; }

        [JsonProperty(PropertyName = "mounts")]
        public CharacterResponseCount Mounts { get; set; }

        [JsonProperty(PropertyName = "orchestrions")]
        public CharacterResponseCount Orchestrions { get; set; }

        [JsonProperty(PropertyName = "spells")]
        public CharacterResponseCount Spells { get; set; }

        [JsonProperty(PropertyName = "emotes")]
        public CharacterResponseCount Emotes { get; set; }

        [JsonProperty(PropertyName = "bardings")]
        public CharacterResponseCount Bardings { get; set; }

        [JsonProperty(PropertyName = "hairstyles")]
        public CharacterResponseCount Hairstyles { get; set; }

        [JsonProperty(PropertyName = "armoires")]
        public CharacterResponseCount Armoires { get; set; }

        [JsonProperty(PropertyName = "fashions")]
        public CharacterResponseCount Fashions { get; set; }

        [JsonProperty(PropertyName = "triad")]
        public CharacterResponseCount Triad { get; set; }
    }

    public class CharacterResponseAchievements
    {
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "total")]
        public int Total { get; set; }

        [JsonProperty(PropertyName = "points")]
        public int Points { get; set; }

        [JsonProperty(PropertyName = "points_total")]
        public int PointsTotal { get; set; }

        [JsonProperty(PropertyName = "public")]
        public bool Public { get; set; }

    }

    public class CharacterResponseCount
    {
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "total")]
        public int Total { get; set; }

        [JsonProperty(PropertyName = "ids")]
        public int[] IDs { get; set; }
    }
}
