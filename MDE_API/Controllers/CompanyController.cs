﻿using Microsoft.AspNetCore.Mvc;
using MDE_API.Application.Interfaces;
using MDE_API.Domain;
using MDE_API.Domain.Models;
using Microsoft.AspNetCore.Authorization;

namespace MDE_API.Controllers
{
    
    [ApiController]
    [Route("api/companies")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [Authorize(Policy = "Role1Only")]
        [HttpGet]
        public ActionResult<List<Company>> GetAll()
        {
            var companies = _companyService.GetAllCompanies();
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public ActionResult<Company?> GetById(int id)
        {
            var company = _companyService.GetCompanyById(id);
            return company is null ? NotFound() : Ok(company);
        }

        [Authorize(Policy = "Role1Only")]
        [HttpPost]
        public IActionResult Create([FromBody] Domain.Models.CompanyModel company)
        {
            _companyService.CreateCompany(company);
            return CreatedAtAction(nameof(GetById), new { id = company.CompanyID }, company);
        }

        [Authorize(Policy = "Role1Only")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Domain.Models.CompanyModel company)
        {
            if (id != company.CompanyID)
                return BadRequest("ID mismatch.");

            var success = _companyService.UpdateCompany(company);
            return success ? NoContent() : NotFound();
        }

        [Authorize(Policy = "Role1Only")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var success = _companyService.DeleteCompany(id);
            return success ? NoContent() : NotFound();
        }

       
    }
   
}
