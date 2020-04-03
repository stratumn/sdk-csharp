namespace Stratumn.Sdk.Model.Misc
{
    /// <summary>
    /// A simple interface to identify objects with an id field.
    /// </summary>
    public interface Identifiable
    {
        /// <summary>
        /// A unique identifier  
        /// </summary>
        string GetId();
    }
}
