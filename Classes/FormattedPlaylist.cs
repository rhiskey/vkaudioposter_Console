namespace vkaudioposter_Console.Classes
{
    public class FormattedPlaylist : vkaudioposter_ef.parser.Playlist
    {

        public FormattedPlaylist(vkaudioposter_ef.parser.Playlist pl, string trueID)
            : base(pl.Id, pl.PlaylistId, pl.PlaylistName, pl.Mood)
        {
            this.trueID = trueID;
        }
        public FormattedPlaylist(vkaudioposter_ef.parser.Playlist pl)
            : base(pl.Id, pl.PlaylistId, pl.PlaylistName, pl.Mood)
        {

        }

        public string trueID { get; set; }

    }
}
