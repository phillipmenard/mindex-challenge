
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using CodeChallenge.Models;

using CodeCodeChallenge.Tests.Integration.Extensions;
using CodeCodeChallenge.Tests.Integration.Helpers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeCodeChallenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        [ClassInitialize]
        // Attribute ClassInitialize requires this signature
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void InitializeClass(TestContext context)
        {
            _testServer = new TestServer();
            _httpClient = _testServer.NewClient();
        }

        [ClassCleanup]
        public static void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newEmployee = response.DeserializeContent<Employee>();
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(expectedLastName, employee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(11)]
        public void GetReportingStructure_Returns_BadRequest(int maxDepth)
        {
            // Arrange
            // Unknown employeeId:
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/structure?maxDepth={maxDepth}");
            var response = getRequestTask.Result;

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "because we provided an invalid argument value.");
            response.Content.ReadAsStringAsync().Result.Should().Be("maxDepth must be between 1 and 10, inclusive.");
        }

        [TestMethod]
        public void GetReportingStructure_Returns_NotFound()
        {
            // Arrange
            // Unknown employeeId:
            var employeeId = "39AD864F-BBB4-4D65-80E7-0AC9244A29B5";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/structure");
            var response = getRequestTask.Result;

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound, "because we did not find the employeeId.");
        }

        [TestMethod]
        [DataRow("16a596ae-edd3-4847-99fe-c4518e82c86f", 4, DisplayName = "Lennon, John")]
        [DataRow("b7839309-3348-463b-a7e3-5de1c168beb3", 0, DisplayName = "McCartney, Paul")]
        [DataRow("03aa1462-ffa9-4978-901b-7c001562cf6f", 2, DisplayName = "Ringo, Starr")]
        public void GetReportingStructure_Returns_OK(string employeeId, int expectedCount)
        {
            // Arrange
            // Nothing to arrange here.

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/structure");
            var response = getRequestTask.Result;

            // Assert
            using var scope = new AssertionScope();
            response.StatusCode.Should().Be(HttpStatusCode.OK, "because we found the employeeId.");
            var structure = response.DeserializeContent<ReportingStructure>();
            structure.Employee.EmployeeId.Should().Be(employeeId, "because this was the requested employee.");
            structure.Employee.DirectReports.Should().BeNull("because we did not request the structure.");
            structure.NumberOfReports.Should().Be(expectedCount, "because that is total number of reports in the test data.");
            structure.IsTruncated.Should().BeFalse("because the data has less depth than the default limit.");
        }

        [TestMethod]
        [DataRow("16a596ae-edd3-4847-99fe-c4518e82c86f", 3, 4, false, DisplayName = "Lennon, John - depth:3")]
        // This data point may seem counter-intuitive: the maxDepth is sufficient, but kept us from actually
        // knowing that it is.
        [DataRow("16a596ae-edd3-4847-99fe-c4518e82c86f", 2, 4, true, DisplayName = "Lennon, John - depth:2")]
        [DataRow("16a596ae-edd3-4847-99fe-c4518e82c86f", 1, 2, true, DisplayName = "Lennon, John - depth:1")]
        [DataRow("03aa1462-ffa9-4978-901b-7c001562cf6f", 2, 2, false, DisplayName = "Ringo, Starr - depth:2")]
        [DataRow("03aa1462-ffa9-4978-901b-7c001562cf6f", 1, 2, true, DisplayName = "Ringo, Starr - depth:1")]
        [DataRow("b7839309-3348-463b-a7e3-5de1c168beb3", 2, 0, false, DisplayName = "McCartney, Paul - depth:2")]
        [DataRow("b7839309-3348-463b-a7e3-5de1c168beb3", 1, 0, false, DisplayName = "McCartney, Paul - depth:1")]

        public void GetReportingStructure_Returns_Truncated(string employeeId, int maxDepth, int expectedCount, bool expectedIsTruncated)
        {
            // Arrange
            // Nothing to arrange here.

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/structure?maxDepth={maxDepth}");
            var response = getRequestTask.Result;

            // Assert
            using var scope = new AssertionScope();
            response.StatusCode.Should().Be(HttpStatusCode.OK, "because we found the employeeId.");
            var structure = response.DeserializeContent<ReportingStructure>();
            structure.Employee.EmployeeId.Should().Be(employeeId, "because this was the requested employee.");
            structure.Employee.DirectReports.Should().BeNull("because we did not request the structure.");
            structure.NumberOfReports.Should().Be(expectedCount, "because that is total number of reports in the test data.");
            structure.IsTruncated.Should().Be(expectedIsTruncated, "because the data has more ore less depth than the provided limit.");
        }

        [TestMethod]
        public void GetReportingStructure_Returns_Tree()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/structure?includeStructure=true");
            var response = getRequestTask.Result;

            // Assert
            using var scope = new AssertionScope();
            response.StatusCode.Should().Be(HttpStatusCode.OK, "because we found the employeeId.");
            var structure = response.DeserializeContent<ReportingStructure>();
            structure.Employee.EmployeeId.Should().Be(employeeId, "because this was the requested employee.");

            var lennonsReports = structure.Employee.DirectReports.Select(d => d.EmployeeId).ToList();
            lennonsReports.Should().BeEquivalentTo(
                new[] { "03aa1462-ffa9-4978-901b-7c001562cf6f", "b7839309-3348-463b-a7e3-5de1c168beb3" },
                options => options.WithoutStrictOrdering());

            var ringosReports = structure.Employee.DirectReports?
                .First(d => d.EmployeeId == "03aa1462-ffa9-4978-901b-7c001562cf6f")
                .DirectReports?
                .Select(d => d.EmployeeId)
                .ToList();

            ringosReports.Should().BeEquivalentTo(
                new[] { "c0c2293d-16bd-4603-8e08-638a9d18b22c", "62c1084e-6e34-4630-93fd-9153afb65309" },
                options => options.WithoutStrictOrdering());
        }

        [TestMethod]
        public void GetReportingStructure_Returns_TruncatedTree()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/structure?includeStructure=true&maxDepth=1");
            var response = getRequestTask.Result;

            // Assert
            using var scope = new AssertionScope();
            response.StatusCode.Should().Be(HttpStatusCode.OK, "because we found the employeeId.");
            var structure = response.DeserializeContent<ReportingStructure>();
            structure.Employee.EmployeeId.Should().Be(employeeId, "because this was the requested employee.");

            var lennonsReports = structure.Employee.DirectReports.Select(d => d.EmployeeId).ToList();
            lennonsReports.Should().BeEquivalentTo(
                new[] { "03aa1462-ffa9-4978-901b-7c001562cf6f", "b7839309-3348-463b-a7e3-5de1c168beb3" },
                options => options.WithoutStrictOrdering());

            structure.Employee.DirectReports
                .All(e => e.DirectReports == null).Should().BeTrue("because we limited depth.");
        }
    }
}
