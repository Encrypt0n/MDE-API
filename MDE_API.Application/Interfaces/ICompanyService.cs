using MDE_API.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDE_API.Application.Interfaces
{
    

    public interface ICompanyService
    {
        List<Company> GetAllCompanies();
        Company? GetCompanyById(int id);
        void CreateCompany(CompanyModel company);
        bool UpdateCompany(CompanyModel company);
        bool DeleteCompany(int id);
    }

    

}
