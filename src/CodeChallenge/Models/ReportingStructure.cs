using System;

namespace CodeChallenge.Models
{
    /// <summary>
    /// Entity for returning information about the structure of a team starting at employee.
    /// </summary>
    public sealed class ReportingStructure
    {
        /// <summary>
        /// Constructor for creating an instance of <see cref="ReportingStructure"/>.
        /// </summary>
        /// <param name="employee">Employee which we started with.</param>
        /// <param name="numberOfReports">Total number of employees reporting to employee.</param>
        /// <param name="isTruncated">Flag indicating that the information is incomplete due to system limits.</param>
        public ReportingStructure(Employee employee, int numberOfReports, bool isTruncated)
        {
            Employee = employee ?? throw new ArgumentNullException(nameof(employee));
            this.NumberOfReports = numberOfReports;
            this.IsTruncated = isTruncated;
        }

        /// <summary>
        /// The <see cref="Models.Employee"/> which has been requested.
        /// </summary>
        public Employee Employee { get; }

        /// <summary>
        /// The total number of employees reporting to <see cref="this.Employee"/> directly or indirectly.
        /// </summary>
        public int NumberOfReports { get; }

        /// <summary>
        /// If the limits of traversing the tree have been reached, IsTruncated will be true.
        /// In this case, the count will stop at the depth requested.
        /// </summary>
        public bool IsTruncated { get; }
    }
}
