using System.Runtime.Serialization;

namespace vkaudioposter_Console.Classes
{
    class Music
    {

        public class Genre
        {
            public string Name { get; set; }
        }

        public class SpotifyToken
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
        }

        [DataContract]
        public class SpotyClass
        {
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string Author { get; set; }
            [DataMember]
            public string Id { get; set; }
        }
    }
}
