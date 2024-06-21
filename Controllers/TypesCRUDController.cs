using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NutriDbService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NutriDbService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TypesCRUDController : Controller
    {
        private readonly ILogger<TypesCRUDController> _logger;
        private RailwayContext _context;
        public TypesCRUDController(RailwayContext context, ILogger<TypesCRUDController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region AllCRUDS

        #region ProductType
        [HttpGet]
        public ActionResult<IEnumerable<Promo>> GetAllPromo()
        {
            try
            {
                var res = _context.Promos.ToList();
                return Ok(res);
            }
            catch (Exception ex)
            {
                return Problem(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
        }
        #endregion

        #endregion


    }
}
