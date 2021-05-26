using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkaudioposter
{
    class Track
    {
        private string Title;
        //private string FullId;//1235467_1234567
        private long OwnerId; //1231467
        private long MediaId; //_21313414

        public Track()
        {
            Title = null;
            OwnerId = 0;
            MediaId = 0;
            return;
        }

        public Track(Track toCopy)
        {
            this.Title = toCopy.Title;
            this.OwnerId = toCopy.OwnerId;
            this.MediaId = toCopy.MediaId;
            return;
        }

        public Track(string CurrentTitle, string CurrentFullId)
        {
            SetTitle(CurrentTitle);
            SetFullId(CurrentFullId);
            return;
        }

        public Track(string CurrentTitle, long? CurrentMediaId, long? CurrentOwnerId)
        {
            SetTitle(CurrentTitle);
            long mID = Convert.ToInt64(CurrentMediaId);
            long oID = Convert.ToInt64(CurrentOwnerId);
            SetMediaId(mID);
            SetOwnerId(oID);
            return;
        }

        public void SetTrack(string CurrentTitle, string CurrentFullId)
        {
            SetTitle(CurrentTitle);
            SetFullId(CurrentFullId);
            return;
        }

        public void SetTitle(string CurrentTitle)
        {
            Title = CurrentTitle.Replace("%20", " ");

            return;
        }
        public void SetFullId(string CurrentFullId)
        {
            int ind3 = CurrentFullId.IndexOf("_");
            string temp_string = CurrentFullId.Substring(0, ind3);

            SetOwnerId(temp_string);

            int dlina = CurrentFullId.Length - 1;
            temp_string = CurrentFullId.Remove(0, ind3 + 1);
            SetMediaId(temp_string);
            return;
        }
        public void SetOwnerId(long CurrentOwnerId)
        {
            OwnerId = CurrentOwnerId;
            return;
        }

        public void SetMediaId(long CurrentMediaId)
        {
            MediaId = CurrentMediaId;
            return;
        }

        public void SetOwnerId(string CurrentOwnerId)
        {
            SetOwnerId(Convert.ToInt64(CurrentOwnerId));
            return;
        }

        public void SetMediaId(string CurrentMediaId)
        {
            SetMediaId(Convert.ToInt64(CurrentMediaId));
            return;
        }

        public string GetTitle()
        {
            return Title;
        }

         public string GetFullId()
        {
           string FullId = OwnerId + "_" + MediaId;
           return FullId;
        }

        public long GetOwnerId()
        {
            return OwnerId;
        }
        public long GetMediaId()
        {
            return MediaId;
        }

    }

    sealed class SpotyVKTrack : Track
    {
        public Dictionary<string, string> Urls { get; set; }
        public string PreviewUrl { get; set; }

        public SpotyVKTrack(string CurrentTitle, long? CurrentMediaId, long? CurrentOwnerId, Dictionary<string, string> CurrUrls, string CurrPreviewUrl) : base (CurrentTitle, CurrentMediaId, CurrentOwnerId)
        {
            Urls = CurrUrls;
            PreviewUrl = CurrPreviewUrl;
        }

        public SpotyVKTrack(string CurrentTitle, long? CurrentMediaId, long? CurrentOwnerId) : base(CurrentTitle, CurrentMediaId, CurrentOwnerId)
        {
        }
    }
}
