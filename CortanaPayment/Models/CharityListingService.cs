
namespace CortanaPayment.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    public class CharityListingService
    {
        private static readonly IEnumerable<Charity> FakeCharityListing = new List<Charity>
        {
            new Charity
            {
                EventCode = 1111,
                Name = "American Red Cross",
                Causes = new string[] { "refugee", "health", "war", "humanitarian" },
                Id = new Guid("bc861179-46a5-4645-a249-7eba2a4d9846"),
                ImageUrl = "http://www.redcross.org/images/MEDIA_CustomProductCatalog/m48040100_ButtonLogo200.jpg",
                Website = "http://www.redcross.org/"
            },
            new Charity
            {
                EventCode = 1234,
                Name = "United Way",
                Causes = new string[] { "communities", "education", "health", "low income" },
                Id = new Guid("c9d96bb4-c580-42fb-a81c-b2870cc78492"),
                ImageUrl = "http://www.unitedway.org/assets/img/logo.svg",
                Website = "http://www.unitedway.org"
            }
        };

        public Task<Charity> GetListingByEventCodeAsync(int eventCode)
        {
            return Task.FromResult(FakeCharityListing.FirstOrDefault(o => o.EventCode.Equals(eventCode)));
        }

        public Task<Charity> GetRandomListingAsync()
        {
            // getting a random item - currently we have only one choice :p
            return Task.FromResult(FakeCharityListing.First());
        }
    }
}