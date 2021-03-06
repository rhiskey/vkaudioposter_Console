﻿namespace vkaudioposter_Console.Classes
{
    public class FormattedPlaylist : vkaudioposter_ef.parser.Playlist
    {
        /// <summary>
        /// Extend playlist with id
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="trueID"></param>
        public FormattedPlaylist(vkaudioposter_ef.parser.Playlist pl, string trueID)
            : base(pl.Id, pl.PlaylistId, pl.PlaylistName, pl.ImageUrl)
        {
            this.trueID = trueID;
        }

        public string trueID { get; set; }

    }
}
