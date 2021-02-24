using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkaudioposter.Classes
{
    class TrackToDB
    {
        public TrackToDB(string NameTo, string StyleTo, DateTime Public_DateTo)
        {
            Name = NameTo;
            Style = StyleTo;
            Publication_Date = Public_DateTo;
            return;
        }

        public string Name;

        public string Style;

        public DateTime Publication_Date;
    }
}
