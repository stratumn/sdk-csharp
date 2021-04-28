namespace Stratumn.Sdk.Model.Trace
{
    public class Info
    {
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
        public string StartCursor { get; set; }

        public string EndCursor { get; set; }

        public Info(bool hasNext, bool hasPrevious, string startCursor, string endCursor)
        {
            this.HasNext = hasNext;
            this.HasPrevious = hasPrevious;
            this.StartCursor = startCursor;
            this.EndCursor = endCursor;
        }
    }
}
