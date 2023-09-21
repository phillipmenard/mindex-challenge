﻿using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CodeChallenge.Services;
using CodeChallenge.Models;

namespace CodeChallenge.Controllers
{
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] Employee employee)
        {
            _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

            _employeeService.Create(employee);

            return CreatedAtRoute("getEmployeeById", new { id = employee.EmployeeId }, employee);
        }

        [HttpGet("{id}", Name = "getEmployeeById")]
        public IActionResult GetEmployeeById(String id)
        {
            _logger.LogDebug($"Received employee get request for '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        public IActionResult ReplaceEmployee(String id, [FromBody]Employee newEmployee)
        {
            _logger.LogDebug($"Received employee update request for '{id}'");

            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound();

            _employeeService.Replace(existingEmployee, newEmployee);

            return Ok(newEmployee);
        }

        /// <summary>
        /// Retrieves the structure of the team starting at employee.EmployeeId = id.
        /// </summary>
        /// <param name="id">EmployeeId to start with.</param>
        /// <param name="maxDepth">Maximum depth to walk to retrieve the information.</param>
        /// <returns><see cref="ReportingStructure"/></returns>
        [HttpGet("{id}/structure", Name = "getEmployeeReportingStructure")]
        public IActionResult GetReportingStructure(String id, int maxDepth = 5)
        {
            _logger.LogDebug($"Received request for employee reporting structure for '{id}'");
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("id is required");
            }

            // Limit the depth a user client request.
            // Here 10 is chosen arbitrarily, but one should reflect a real-world concern.
            // A value of 0 would always result in count 0, since self is not included.
            if (maxDepth > 10 || maxDepth <= 0)
            {
                return BadRequest($"{nameof(maxDepth)} must be between 1 and 10, inclusive.");
            }

            var employee = _employeeService.GetById(id, nameof(Employee.DirectReports));
            if (employee == null)
            {
                return NotFound();
            }

            var isTruncated = !this.TryCountReports(employee, maxDepth, 1, out var count);

            // Count includes "self", so remove the top employee from the count.
            count--;

            // For now we'll exclude the actual structure as a plausible size concern.
            // It was not requested explicitly in the work request.
            employee.DirectReports = null;

            return Ok(new ReportingStructure(employee, count, isTruncated));
        }

        /// <summary>
        /// Counts each employee starting at the provided employee (so result is >=1)
        ///  and all DirectReports recursively from there.
        /// </summary>
        /// <param name="employee"></param>
        /// <returns>False if truncated due to depth.</returns>
        private bool TryCountReports(Employee employee, int maxDepth, int depth, out int count)
        {
            // count self:
            count = 1;

            if (depth > maxDepth)
            {
                //Indicate depth issue.
                return false;
            }

            //Enrich employees, as needed
            if (employee.DirectReports == null)
            {
                employee = _employeeService.GetById(employee.EmployeeId, nameof(Employee.DirectReports));
            }

            var result = true;
            foreach(var subordinate in employee.DirectReports)
            {
                if (!this.TryCountReports(subordinate, maxDepth, depth+1, out var subCount))
                {
                    result = false;
                }
                count += subCount;
            }
            return result;
        }
    }
}
