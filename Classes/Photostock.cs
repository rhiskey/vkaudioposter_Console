using System.Runtime.Serialization;

namespace vkaudioposter.Classes
{
    [DataContract]
    public class Photostock_class
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string URL { get; set; }
        [DataMember]
        public int Page { get; set; }
    }
}
