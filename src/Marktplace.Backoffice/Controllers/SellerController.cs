using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marktplace.Backoffice.Controllers
{
    [ApiController]
    public class SellerController : ControllerBase
    {
        private List<object> _sellersDB;

        public SellerController()
        {
            _sellersDB = new List<object>
            {
                new {Id = "rchlo", Name = "Riachuelo"},
                new {Id = "opengate", Name = "Open gate"},
                new {Id = "odisseia", Name = "Odisseia"},
                new {Id = "gears", Name = "Gears"}
            };
        }

        [HttpGet("api/public/sellers")]
        public IList<object> GetSellers()
        {
            return _sellersDB;
        }

        [HttpGet("api/private/sellers")]
        [Authorize]
        public IList<object> GetSellersWithAuthentication()
        {
            return _sellersDB;
        }

        [HttpGet("api/private-role/sellers")]
        [Authorize(Roles = "view-seller")]
        public IList<object> GetSellersWithAuthenticationAndRole()
        {
            return _sellersDB;
        }

        [HttpGet("api/tenat/sellers/{id}")]
        [Authorize()]
        public ActionResult<IList<object>> GetSellersWithAuthenticationAndTenat(string id)
        {
            var tenantId = User?.FindFirst("tenantId")?.Value;

            if (tenantId != id)
                return new ForbidResult();

            return _sellersDB;
        }
    }
}
