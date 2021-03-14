using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using MovieMatrix.Helper;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;

namespace MovieMatrix.Model
{
    public class MediaFileInfo
    {
        public virtual string FileName { get; set; }

        public virtual string FilePath { get; set; }

        public virtual string FolderName { get; set; }

        public virtual string FolderPath { get; set; }

        public virtual string MediaItemName { get; set; }

        public virtual PersonalInfo PersonalInfo { get; set; }

        public virtual SearchBase SelectedMediaItem { get; set; }

        public virtual List<SearchBase> FoundMediaItems { get; set; }


        public static MediaFileInfo Create()
        {
            return ViewModelSource.Create<MediaFileInfo>();
        }

        public static async Task<List<MediaFileInfo>> Create(string path, bool useParentFolderName, MediaType mediaType)
        {
            List<MediaFileInfo> mediaFileInfoList = new List<MediaFileInfo>();

            await Task.Run(() =>
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);                
                FileInfo[] files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
                
                string[] extensions = mediaType == MediaType.Person ? FileHelper.ImageFiles : FileHelper.VideoFiles;
                files = files.Where(x => extensions.Contains(x.Extension.ToUpperInvariant())).ToArray();

                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i];
                    string fileName = Path.GetFileNameWithoutExtension(file.FullName);

                    MediaFileInfo mediaFileInfo = ViewModelSource.Create<MediaFileInfo>();
                    mediaFileInfo.FileName = fileName;
                    mediaFileInfo.FilePath = file.FullName;
                    mediaFileInfo.FolderName = file.Directory.Name;
                    mediaFileInfo.FolderPath = file.Directory.FullName;
                    mediaFileInfo.MediaItemName = useParentFolderName ? mediaFileInfo.FolderName : mediaFileInfo.FileName;

                    List<Match> matchList = new List<Match>
                    {
                        YearRegex.Match(mediaFileInfo.MediaItemName),
                        DiscRegex.Match(mediaFileInfo.MediaItemName),
                        SeasonRegex.Match(mediaFileInfo.MediaItemName),
                        EpisodeRegex.Match(mediaFileInfo.MediaItemName),
                        SeasonEpisodeRegex.Match(mediaFileInfo.MediaItemName),
                        LabelRegex.Match(mediaFileInfo.MediaItemName),
                        SourceRegex.Match(mediaFileInfo.MediaItemName),
                        WholeWordRegex.Match(mediaFileInfo.MediaItemName),
                        ResolutionRegex.Match(mediaFileInfo.MediaItemName)
                    };

                    matchList.Sort((x, y) => x.Index.CompareTo(y.Index));
                    foreach (Match match in matchList)
                    {
                        if (match.Index > 3)
                        {
                            mediaFileInfo.MediaItemName = mediaFileInfo.MediaItemName.Remove(match.Index);
                            break;
                        }
                    }

                    mediaFileInfo.MediaItemName = Regex.Replace(mediaFileInfo.MediaItemName, @"[\W_]", " ");
                    mediaFileInfo.MediaItemName = Regex.Replace(mediaFileInfo.MediaItemName, @"\s+", " ");
                    mediaFileInfo.MediaItemName = mediaFileInfo.MediaItemName.Trim();

                    if (mediaFileInfoList.FirstOrDefault(x => x.MediaItemName == mediaFileInfo.MediaItemName) == null)
                        mediaFileInfoList.Add(mediaFileInfo);

                    mediaFileInfo.PersonalInfo = new PersonalInfo();
                    mediaFileInfo.PersonalInfo.LocalPath = mediaFileInfo.FolderPath;
                }
            });

            return mediaFileInfoList;
        }

        #region ReadOnly

        public static readonly string[] Resolutions =
        {
            "240p",
            "360p",
            "480p",
            "480i",
            "720p",
            "720i",
            "1080p",
            "1080i"
        };

        public static readonly string[] Labels =
        {
            "COMPLETE",
            "CUSTOM",
            "DUBBED",
            "LiMiTED",
            "MULTiSUBS",
            "PROPER",
            "REPACK",
            "RETAIL",
            "SUBBED",
            "UNRATED"
        };

        public static readonly string[] Sources =
        {
            "BD25",
            "BD5",
            "BD50",
            "BD9",
            "BDRip",
            "BDSCR",
            "BluRay",
            "Blu-Ray",
            "BRRip",
            "CAMRip",
            "DSRip",
            "DTHRip",
            "DVBRip",
            "DVD-5",
            "DVD-9",
            "DVDR",
            "DVDRip",
            "DVDSCR",
            "DVDSCREENER",
            "HDTV",
            "HDTVRip",
            "KVCD",
            "PDTV",
            "PDVD",
            "PPV",
            "PPVRip",
            "R5",
            "SCREENER",
            "SVCD",
            "TELECINE",
            "TELESYNC",
            "TVRip",
            "VCD",
            "VODR",
            "VODRip",
            "WEBRIP",
            "WEB-Rip",
            "WORKPRINT",
            "x264",
            "h264"
        };

        public static readonly string[] WholeWords =
        {
            "\bBDR\b",
            "\bCAM\b",
            "\bDDC\b",
            "\bDSR\b",
            "\bSCR\b",
            "\bTC\b",
            "\bTS\b",
            "\bWP\b"
        };

        public static readonly Regex YearRegex = new Regex(@"(19|20)\d{2}", RegexOptions.Compiled);
        public static readonly Regex DiscRegex = new Regex(@"CD\d{1}", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static readonly Regex SeasonRegex = new Regex(@"Se?\d+|Season.\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        public static readonly Regex EpisodeRegex = new Regex(@"Ep?\d+|Episode.\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        public static readonly Regex SeasonEpisodeRegex = new Regex(@"S\d+x?E\d+|\d+x\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static readonly Regex LabelRegex = new Regex(String.Join("|", Labels), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        public static readonly Regex SourceRegex = new Regex(String.Join("|", Sources), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        public static readonly Regex WholeWordRegex = new Regex(String.Join("|", WholeWords), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        public static readonly Regex ResolutionRegex = new Regex(String.Join("|", Resolutions), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        #endregion
    }

    public class PersonalInfo
    {
        public bool Seen { get; set; }

        public bool Favorite { get; set; }

        public bool Watchlist { get; set; }        

        public string LocalPath { get; set; }

        public double PersonalRating { get; set; }
    }
}
