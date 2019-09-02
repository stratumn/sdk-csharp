using Newtonsoft.Json;

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
        
            if (string.ReferenceEquals(startCursor, null))
            {
                throw new System.ArgumentException("startCursor cannot be null in Info");
            }
            if (string.ReferenceEquals(endCursor, null))
            {
                throw new System.ArgumentException("endCursor cannot be null in Info");
            }

            this.HasNext = hasNext;
            this.HasPrevious = hasPrevious;
            this.StartCursor = startCursor;
            this.EndCursor = endCursor;
        }

    }

}
