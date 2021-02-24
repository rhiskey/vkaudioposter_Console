using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
