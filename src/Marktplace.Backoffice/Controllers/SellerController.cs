using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marktplace.Backoffice.Controllers
{
    [ApiController]
    public class SellerController : ControllerBase
    {
        private List<Seller> _sellersDB;

        public SellerController()
        {
            _sellersDB = new List<Seller>
            {
                new Seller {Id = "rchlo", Name = "Riachuelo"},
                new Seller {Id = "opengate", Name = "Open gate"},
                new Seller {Id = "odisseia", Name = "Odisseia"},
                new Seller {Id = "gears", Name = "Gears"}
            };
        }

        [HttpGet("api/public/sellers")]
        public IList<Seller> GetSellers()
        {
            return _sellersDB;
        }

        [HttpGet("api/private/sellers")]
        [Authorize]
        public IList<Seller> GetSellersWithAuthentication()
        {
            return _sellersDB;
        }

        [HttpGet("api/private-role/sellers")]
        [Authorize(Roles = "view-seller")]
        public IList<Seller> GetSellersWithAuthenticationAndRole()
        {
            return _sellersDB;
        }
    }

    public class Seller
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
