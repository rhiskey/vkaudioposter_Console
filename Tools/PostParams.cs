using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model.Attachments;

namespace vkaudioposter
{
    class Post
    {
        public List<MediaAttachment> Atts { get; set; }
        public long Owner_Id { get; set; }
        public string Message_To_Attach { get; set; }
        public DateTime Publication_Date { get; set; }
    }
    class PostParams : IEnumerable
    {
        public List<MediaAttachment> Atts { get; set; }
        public long Owner_Id { get; set; }
        public string Message_To_Attach { get; set; }
        public DateTime Publication_Date { get; set; }
        public IEnumerator GetEnumerator()
        {
            return Atts.GetEnumerator();
        }

        public void Add(IEnumerable <MediaAttachment> attachments)
        {
            Atts.AddRange(attachments);
        }
        
    }


}
