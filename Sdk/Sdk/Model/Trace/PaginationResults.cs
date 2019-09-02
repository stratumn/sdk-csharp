using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.Trace
{
    public class PaginationResults
    {

        private int totalCount;
        private Info info;

        public PaginationResults()
        {

        }
        public PaginationResults(int totalCount, Info info)
        {

            if (info == null)
            {
                throw new System.ArgumentException("info cannot be null in PaginationResults");
            }

            this.totalCount = totalCount;
        }

        public int TotalCount
        {
            get
            {
                return this.totalCount;
            }
            set
            {
                this.totalCount = value;
            }
        }


        public Info Info
        {
            get
            {
                return this.info;
            }
            set
            {
                this.info = value;
            }
        }


    }

}
