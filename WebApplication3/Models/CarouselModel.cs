namespace WebApplication3.Models
{
    public class CarouselModel
    {
        public bool NeedsSeminario { get; set; }
        public bool NeedsEnergeiesTouMina { get; set; }
        public bool NeedsProsforesExoplismou { get; set; }

        public int SeminarioImageCount { get; set; }
        public int EnergeiesTouMinaImageCount { get; set; }
        public int ProsforesExoplismouImageCount { get; set; }

        public List<ImageInfo> SeminarioImages { get; set; }
        public List<ImageInfo> EnergeiesImages { get; set; }
        public List<ImageInfo> ProsforesImages { get; set; }
        public List<ImageInfo> OldSeminarioImages { get; set; }
        public List<ImageInfo> OldEnergeiesImages { get; set; }
        public List<ImageInfo> OldProsforesImages { get; set; }

        public string CustomerId { get; set; }

    }

    public class ImageInfo
    {
        public string ImageId { get; set; }
        public string Url { get; set; }
        public bool KeepDefault { get; set; }
        public string Query { get; set; }
        public string EventLabel { get; set; }
        public string CustomerId { get; set; }
        public string OldImageSelection { get; set; }
        public bool IncludePostMessage { get; set; }
    }


}
