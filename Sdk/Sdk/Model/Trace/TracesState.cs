using System.Collections.Generic;

namespace Stratumn.Sdk.Model.Trace
{
    public class TracesState<TState, TlinkData> : PaginationResults
    {
        private IList<TraceState<TState, TlinkData>> traces;

        public TracesState()
        {
            this.traces = new List<TraceState<TState, TlinkData>>();
        }

        public TracesState(IList<TraceState<TState, TlinkData>> traces)
        {
            this.traces = traces;
        }

        public virtual IList<TraceState<TState, TlinkData>> Traces
        {
            get
            {
                return this.traces;
            }
            set
            {
                this.traces = value;
            }
        }
    }
}
