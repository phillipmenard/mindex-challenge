﻿using CodeChallenge.Models;
using System;
using System.Threading.Tasks;

namespace CodeChallenge.Repositories
{
    public interface IEmployeeRepository
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
        Employee Add(Employee employee);
        Employee Remove(Employee employee);

        /// <summary>
        /// Creates a new compensation record or updates an existing one.
        /// </summary>
        /// <param name="compensation">Record to create or update.</param>
        /// <returns>Final value.</returns>
        Compensation AddOrUpdate(Compensation compensation);

        Task SaveAsync();
    }
}