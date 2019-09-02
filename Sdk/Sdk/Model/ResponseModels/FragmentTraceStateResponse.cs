using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratumn.Sdk.Model.ResponseModels
{
    public class FragmentTraceStateResponse
    {

        public string UpdatedAt
        {
            get
            {
                return updatedAt;
            }
            set
            {
                this.updatedAt = value;
            }
        }


        public object State
        {
            get
            {
                return state;
            }
            set
            {
                this.state = value;
            }
        }


        public HeadResponse Head
        {
            get
            {
                return head;
            }
            set
            {
                this.head = value;
            }
        }


        private string updatedAt;
        private object state;
        private HeadResponse head;
    }



    public class HeadResponse
    {

        public object Raw
        {
            get
            {
                return raw;
            }
            set
            {
                this.raw = value;
            }
        }


        public object Data
        {
            get
            {
                return data;
            }
            set
            {
                this.data = value;
            }
        }


        private object raw;
        private object data;
    }

}
