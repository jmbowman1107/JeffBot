using System.Text.RegularExpressions;

namespace JeffBot
{
    public static class Extensions
    {
        #region SplitToLines
        /// <summary>
        /// Given a string, and maximum string character length, will return a MatchCollection on lines of maximumLineLenth, while not splitting words.
        /// </summary>
        /// <param name="stringToSplit"></param>
        /// <param name="maximumLineLength"></param>
        /// <returns></returns>
        public static MatchCollection SplitToLines(this string stringToSplit, int maximumLineLength)
        {
            return Regex.Matches(stringToSplit, @"(.{1," + maximumLineLength + @"})(?:\s|$)");
        }
        #endregion
    }
}