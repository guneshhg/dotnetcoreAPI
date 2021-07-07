using AutoMapper;
using Contractss;
using Contractss.DTOs;
using Entities;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CompanyEmployee.API.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDto> _dataShaper;


        public EmployeesController(IRepositoryManager repository, ILoggerManager logger,IMapper mapper,IDataShaper<EmployeeDto> dataShaper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _dataShaper = dataShaper;
        }

        //[HttpGet]
        //public IActionResult GetEmployeesForCompany(Guid companyId)
        //{
        //    var company = _repository.Company.GetCompanyAsync(companyId, trackChanges: false);
        //    if (company == null)
        //    {
        //        _logger.LogInfo($"Company with id:{companyId} doesnt exist in the database.");
        //        return NotFound();
        //    }
        //    var employeeFromDb = _repository.Employee.GetEmployees(companyId, false);
        //    var employeeDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeeFromDb);

        //    return Ok(employeeDto);
        //}


        ////https://localhost:5001/api/companies/C9D4C053-49B6-410C-BC78-2D54A9991870/employees?pageNumber=2&pageSize=2
        //[HttpGet(Name = "GetEmployeeForCompany")]
        //public IActionResult GetEmployeeForCompany(Guid companyId,[FromQuery]EmployeeParameters employeeParameters)
        //{
        //    var company = _repository.Company.GetCompany(companyId, false);
        //    if (company == null)
        //    {
        //        _logger.LogInfo($"Company with id:{companyId} doesnt exist in the database.");
        //        return NotFound();
        //    }

        //    var employeeDb = _repository.Employee.GetEmployees(companyId, employeeParameters, false);


        //    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employeeDb.MetaData));

        //    var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeeDb);
        //    return Ok(employeesDto);
        //}


        //https://localhost:5001/api/companies/C9D4C053-49B6-410C-BC78-2D54A9991870/employees?pageNumber=2&pageSize=2
        //https://localhost:5001/api/companies/c9d4c053-49b6-410c-bc78-2d54a9991870/employees?searchTerm=Mihael
        //https://localhost:44306/api/companies/c9d4c053-49b6-410c-bc78-2d54a9991870/employees?MinAge=30&MaxAge=55&SearchTerm=Martin&PageNumber=1&PageSize=6&OrderBy=name%2Cage%20desc
        //https://localhost:44306/api/companies/fa0c3680-9f96-4563-2be6-08d93e4a91c3/employees?MinAge=40&MaxAge=50&PageNumber=1&PageSize=6&Fields=name
        [HttpGet(Name = "GetEmployeeForCompany")]
        public IActionResult GetEmployeeForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.ValidAgeRange)
            {
                return BadRequest("Max age cant be less than min age.");
            }
            var company = _repository.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id:{companyId} doesnt exist in the database.");
                return NotFound();
            }

            var employeeDb = _repository.Employee.GetEmployees(companyId, employeeParameters, false);


            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employeeDb.MetaData));

            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeeDb);
            return Ok(_dataShaper.ShapeData(employeesDto, employeeParameters.Fields));

        }


        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId,[FromBody]EmployeeForCreationDto employee)
        {
            if (employee == null)
            {
                _logger.LogError("EmployeeCreationDto object sent from client is null.");
                return BadRequest("EmployeeForCreationDto object is null");
            }
            if(!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForCreationDto object");
                return UnprocessableEntity(ModelState);
            }
            var company = _repository.Company.GetCompany(companyId, trackChanges: false);
            if(company==null)
            {
                _logger.LogInfo($"Company with id:{companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employeeEntity = _mapper.Map<Employee>(employee);
            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            _repository.Save();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);


        }


        [HttpDelete("{id}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId,Guid id)
        {
            var company = _repository.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id:{companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeForCompany = _repository.Employee.GetEmployee(companyId, id, trackChanges: false);

            if (employeeForCompany == null)
            {
                _logger.LogInfo($"Employee with id: { id} doesn't exist in the database.");
                return NotFound();
            }
            _repository.Employee.DeleteEmployee(employeeForCompany);
            _repository.Save();
            return NoContent();
        }


        [HttpPut("{id}")]
        public IActionResult UpdateEmployeeForCompany(Guid companyId,Guid id,[FromBody]EmployeeForUpdateDto employee)
        {
            if (employee == null)
            {
                _logger.LogError("EmployeeForUpdateDto object sent from client is null");
                return BadRequest("EmployeeForUpdateDto object is null");
            }

            var company = _repository.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id:{companyId} doesnt exist in the database.");
                return NotFound();
            }
            var employeeEntity = _repository.Employee.GetEmployee(companyId, id, trackChanges: true);
            if (employeeEntity == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
                return NotFound();
            }
            _mapper.Map(employee, employeeEntity);
            _repository.Save();
            return NoContent();

        }




        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateEmployeeForCompany(Guid companyId,Guid id,[FromBody]JsonPatchDocument<EmployeeForUpdateDto>patchDoc)
        {
            if (patchDoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }

            var company = _repository.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity = _repository.Employee.GetEmployee(companyId, id, trackChanges: true);
            if (employeeEntity == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
                return NotFound();
            }

            /*
             
                 [
                  {
                    "operationType": 0,
                    "path": "/Age",
                    "op": "replace",
                    "value": "28"
                  }
                ]
            */

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
            patchDoc.ApplyTo(employeeToPatch);
            _mapper.Map(employeeToPatch, employeeEntity);
            _repository.Save();
            return NoContent();
        }


        


    }
}
