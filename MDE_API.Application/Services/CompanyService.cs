using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDE_API.Application.Interfaces;
using MDE_API.Domain.Models;
using Microsoft.Extensions.Logging;

namespace MDE_API.Application.Services
{
    

    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(ICompanyRepository companyRepository, ILogger<CompanyService> logger)
        {
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public List<Company> GetAllCompanies() => _companyRepository.GetAllCompanies();

        public Company? GetCompanyById(int id) => _companyRepository.GetCompanyById(id);

        public void CreateCompany(CompanyModel companyModel)
        {
            _logger.LogInformation("🔥 Logger is working inside CompanyService.");

            string _apiToken = "6WYx0f-LkSgujUSxbn0EUbXSSd8DI0B75ptvsxwI";
            string _zoneId = "8eb734374b1ef138b41104b31620f7ea";
            var usedSubnets = _companyRepository.GetAllCompanies()
                    .Select(c => c.Subnet)
                    .ToHashSet();

            string? assignedSubnet = null;
            int chosenSubnetNumber = -1;

            for (int i = 10; i <= 200; i++)
            {
                var candidateSubnet = $"10.8.{i}.0";
                if (!usedSubnets.Contains(candidateSubnet))
                {
                    assignedSubnet = candidateSubnet;
                    chosenSubnetNumber = i;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(assignedSubnet))
                throw new InvalidOperationException("No available subnet could be assigned in range 10.8.10.0 - 10.8.200.0");

            var company = new Company
            {
                Name = companyModel.Name,
                Description = companyModel.Description,
                Subnet = assignedSubnet
            };

            _companyRepository.CreateCompany(company);

        }






        public bool UpdateCompany(CompanyModel company) => _companyRepository.UpdateCompany(company);

        public bool DeleteCompany(int id) => _companyRepository.DeleteCompany(id);
    }

}
