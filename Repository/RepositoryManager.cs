using Contractss;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class RepositoryManager : IRepositoryManager
    {
        private RepositoryContext _context;
        private ICompanyRepository _companyRepository;
        private IEmployeeRepository _employeeRepository;


        public RepositoryManager(RepositoryContext context)
        {
            _context = context;
        }

        public ICompanyRepository Company
        {
            get
            {
                if (_companyRepository == null)
                    _companyRepository = new CompanyRepository(_context);
                return _companyRepository;

            }
        }

        public IEmployeeRepository Employee
        {
            get
            {
                if (_employeeRepository == null)
                    _employeeRepository = new EmployeeRepository(_context);
                return _employeeRepository;
            }
        }

        public void Save() => _context.SaveChanges();
    }
}
