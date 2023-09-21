using System;

namespace CodeChallenge.Models
{
    /// <summary>
    /// Entity for reporting and updating the compensation package for an employee.
    /// </summary>
    public sealed class Compensation
    {
        /// <summary>
        /// Employee associated with this compensation package.
        /// </summary>
        public Employee Employee { get; set; }

        /// <summary>
        /// The date on which this package (ie salary) is in place.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Salary for <see cref="Employee"/> on or after <see cref="EffectiveDate"/>.
        /// </summary>
        public Decimal Salary { get; set; }
    }
}
