﻿namespace Stratumn.Sdk.Model.Client
{
    /// <summary>
    /// The graphql options
    /// </summary>
    public class GraphQLOptions
    {
        /// <summary>
        /// The retry count
        /// defaults to 1
        /// </summary>
        private int retry;

        public GraphQLOptions(int? retry)
        {
            if (retry == null)
            {
                throw new System.ArgumentException("retry cannot be null");
            }
            this.retry = retry.Value;
        }

        public int Retry
        {
            get
            {
                return this.retry;
            }
            set
            {
                this.retry = value;
            }
        }
    }
}
