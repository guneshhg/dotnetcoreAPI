using Contractss;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployee.API.Controllers
{
   // https://localhost:5001/api/companies?api-version=2.0
    [ApiVersion("2.0")]
   // [Route("api/companies")]
    [Route("api/{v:apiversion}/companies")]
    //api/2.0/companies
    [ApiController]
    public class CompaniesV2Controller : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        public CompaniesV2Controller(IRepositoryManager repositoryManager)
        {
            _repository = repositoryManager;
        }

        [HttpGet]
        public IActionResult GetCompanies()
        {
            var companies = _repository.Company.GetAllCompanies(trackChanges: false);
            return Ok(companies);
        }



    }
}
