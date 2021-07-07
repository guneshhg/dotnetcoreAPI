using Contractss;
using Entities;
using Entities.RequestFeatures;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class EmployeeRepository:RepositoryBase<Employee>,IEmployeeRepository
    {
        public EmployeeRepository(RepositoryContext repositoryContext)
            :base(repositoryContext)
        {

        }

        public void CreateEmployeeForCompany(Guid companyId, Employee employee)
        {
            employee.CompanyId = companyId;
            Create(employee);
        }

        public Employee GetEmployee(Guid companyId, Guid id, bool trackChanges) =>
      FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(id),trackChanges).SingleOrDefault();

        public PagedList<Employee> GetEmployees(Guid companyId,
        EmployeeParameters employeeParameters, bool trackChanges)
        {

            var employees =  FindByCondition(e => e.CompanyId.Equals(companyId),trackChanges)
           .FilterEmployees(employeeParameters.MinAge,employeeParameters.MaxAge)
           .Search(employeeParameters.SearchTerm)
           .Sort(employeeParameters.OrderBy)
           .ToList();

            return PagedList<Employee>
                .ToPagedList(employees, employeeParameters.PageNumber, employeeParameters.PageSize);

        }



        public void DeleteEmployee(Employee employee)
        {
            Delete(employee);
        }
       

    }
}
