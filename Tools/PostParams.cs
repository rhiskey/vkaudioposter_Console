﻿using System;
using System.Collections;
using System.Collections.Generic;
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

        public void Add(System.Collections.Generic.IEnumerable<MediaAttachment> attachments)
        {
            Atts.AddRange(attachments);
        }

    }


}
