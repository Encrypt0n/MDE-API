using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDE_API.Domain;
using MDE_API.Domain.Models;

namespace MDE_API.Application.Interfaces
{
   

    public interface ICompanyRepository
    {
        List<Company> GetAllCompanies();
        Company? GetCompanyById(int id);
        void CreateCompany(Company company);
        bool UpdateCompany(CompanyModel company);
        bool DeleteCompany(int id);
    }

}
