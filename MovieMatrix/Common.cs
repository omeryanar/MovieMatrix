using System.ComponentModel.DataAnnotations;

namespace MovieMatrix
{
    public class CloseDocumentMessage
    {
        public object Document { get; private set; }

        public CloseDocumentMessage(object document)
        {
            Document = document;
        }
    }

    public class CultureChangeMessage
    {
        public string OldCulture { get; private set; }

        public string NewCulture { get; private set; }

        public CultureChangeMessage(string oldCulture, string newCulture)
        {
            OldCulture = oldCulture;
            NewCulture = newCulture;
        }
    }    

    public enum MessageType
    {
        Error,
        Warning,
        Information
    }

    public enum MovieSort
    {
        [Display(Name = "PopularityDesc")]
        PopularityDesc,

        [Display(Name = "PopularityAsc")]
        PopularityAsc,

        [Display(Name = "ReleaseDateDesc")]
        ReleaseDateDesc,

        [Display(Name = "ReleaseDateAsc")]
        ReleaseDateAsc,        

        [Display(Name = "VoteAverageDesc")]
        VoteAverageDesc,

        [Display(Name = "VoteAverageAsc")]
        VoteAverageAsc,

        [Display(Name = "RevenueDesc")]
        RevenueDesc,

        [Display(Name = "RevenueAsc")]
        RevenueAsc
    }

    public enum TvShowSort
    {
        [Display(Name = "PopularityDesc")]
        PopularityDesc,

        [Display(Name = "PopularityAsc")]
        PopularityAsc,

        [Display(Name = "FirstAirDateDesc")]
        FirstAirDateDesc,

        [Display(Name = "FirstAirDateAsc")]
        FirstAirDateAsc,

        [Display(Name = "VoteAverageDesc")]
        VoteAverageDesc,

        [Display(Name = "VoteAverageAsc")]
        VoteAverageAsc
    }

    public enum CombineFilter
    {
        [Display(Name = "AllOf")]
        AllOf,

        [Display(Name = "AnyOf")]
        AnyOf
    }

    public enum YearFilter
    {
        [Display(Name = "IsInYear")]
        IsInYear,

        [Display(Name = "IsBetween")]
        IsBetween
    }

    public enum TvSeriesStatus
    {
        [Display(Name = "Ended")]
        Ended,

        [Display(Name = "Pilot")]
        Pilot,

        [Display(Name = "Planned")]
        Planned,

        [Display(Name = "Canceled")]
        Canceled,

        [Display(Name = "InProduction")]
        InProduction,

        [Display(Name = "ReturningSeries")]
        ReturningSeries
    }
}
