using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkaudioposter
{
    class Chart
    {
        private string Author { get; set; }
        private string Trackname { get; set; }
        private string Remix { get; set; }

        public Chart()
        {
            Author = null;
            Trackname = null;
            Remix = null;
            return;
        }

        public Chart(Chart toCopy)
        {
            this.Author = toCopy.Author;
            this.Trackname = toCopy.Trackname;
            this.Remix = toCopy.Remix;
            return;
        }

        public  Chart(string Trackname, string Remix, string Author)
        {
            SetTrackname(Trackname);
            SetRemix(Remix);
            SetAuthor(Author);
            return;
        }

        public Chart(string Trackname, string Authors)
        {
            SetTrackname(Trackname);
            SetAuthor(Authors);
            return;
        }

        ~Chart()
        {

        }

        public void SetTrackname(string CurrentTrackname)
        {
            Trackname = CurrentTrackname;
            return;
        }
        public void SetRemix(string CurrentRemix)
        {
            Remix = CurrentRemix;
            return;
        }

        public void SetAuthor(string CurrentAuthor)
        {
            Author = CurrentAuthor;
            return;
        }
        public string GetTrackname()
        {
            return Trackname;
        }

        public string GetAuthor()
        {
            return Author;
        }

        public string GetRemix()
        {
            return Remix;
        }

        public string GetTrackAndAuthors()
        {
            return Trackname + " " + Author;
        }
    }
}
