using CodeChallenge.Models;
using System;

namespace CodeChallenge.Services
{
    public interface IEmployeeService
    {
        Employee GetById(String id);
        /// <summary>
        /// Retrieves <see cref="Employee"/> by id.
        /// Optionally, returns additional fields not returned by default.
        /// </summary>
        /// <param name="id">EmployeeId to retrieve.</param>
        /// <param name="additionalFields">Additional fields to return.</param>
        /// <returns>Employee populated with additional fields.</returns>
        /// <remarks>
        /// This is added as an additional method as including all fields always
        /// may burden the service or clients unnecessarily.
        /// If the intent of the original was to provide these fields, then
        /// we should update the original, likely in a non-parameterized way.
        /// </remarks>
        Employee GetById(String id, params string[] additionalFields);
        Employee Create(Employee employee);
        Employee Replace(Employee originalEmployee, Employee newEmployee);

        /// <summary>
        /// Creates a new compensation row, or updates the salary of an
        /// existing row based on matching employee id + effective date.
        /// </summary>
        /// <param name="compensation">Record to add.</param>
        /// <returns>Updated <see cref="Compensation"/>.</returns>
        Compensation AddOrUpdateCompensation(Compensation compensation);
    }
}
