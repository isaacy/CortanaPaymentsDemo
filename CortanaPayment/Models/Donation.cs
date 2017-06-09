using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CortanaPayment.Models
{
    [Serializable]
    public class Donation
    {
        public string Description { get; set; }
        public Guid Id { get; set; }
        public Charity Recipient { get; set; }
        public string DonorName { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }

        public override string ToString()
        {
            return $"Donation to {Recipient.Name}";
        }

    }
}