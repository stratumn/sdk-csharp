using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
          string Id { get; }
    }

}
