using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace GameShell.Models
{
    [DataContract]
    public class GameInfo
    {
        [DataMember]
        [JsonProperty("name")]
        public string Title { get; set; }

        [DataMember]
        [JsonProperty("background_image")]
        public string BackgroundImageUrl { get; set; }

        public string Description { get; set; }
    }
}
