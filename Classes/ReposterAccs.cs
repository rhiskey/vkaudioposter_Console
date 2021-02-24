using System.Runtime.Serialization;

namespace vkaudioposter.Classes
{

    [DataContract]
    public class ReposterAccs
    {

            [DataMember]
            public string Login { get; set; }

            [DataMember]
            public string Password { get; set; }

            [DataMember]
            public string AccessToken { get; set; }


    }
}
