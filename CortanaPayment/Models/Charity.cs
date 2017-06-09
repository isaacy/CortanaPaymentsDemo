using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CortanaPayment.Models
{
    [Serializable]
    public class Charity
    {
        public int EventCode { get; set; }

        public Guid Id { get; set; }
        public string Name { get; set; }

        public string[] Causes { get; set; }

        public string ImageUrl { get; set; }
    
        public string Website { get; set; }

    }
}