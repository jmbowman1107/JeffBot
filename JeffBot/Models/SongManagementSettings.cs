using System.Collections.Generic;

namespace JeffBot
{
    public class SongManagementSettings
    {
        #region MessageBeforeSong
        public string MessageBeforeSong { get; set; } = "The current song is: ";
        #endregion
        #region MessageAfterSong
        public string MessageAfterSong { get; set; }
        #endregion
    }
}