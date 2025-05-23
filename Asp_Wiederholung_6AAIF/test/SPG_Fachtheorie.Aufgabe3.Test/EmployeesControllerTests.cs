using Spg.Fachtheorie.Aufgabe3.API.Test;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe3.Dtos;
using System.Net;

namespace SPG_Fachtheorie.Aufgabe3.Test
{
    public class EmployeesControllerTests
    {
        [Theory]
        [InlineData("cashier", 1)]
        [InlineData("manager", 2)]
        public async Task GetAllEmployeesSuccessTest(string type, int expectedId)
        {
            // ARRANGE
            var factory = new TestWebApplicationFactory();
            factory.InitializeDatabase((db) =>
            {
                var employee1 = new Cashier(
                    registrationNumber: 1, firstName: "FN", lastName: "LN", birthday: new DateOnly(2004, 3, 2), salary: 3000M, address: null, jobSpezialisation: "Feinkost");
                var employee2 = new Manager(
                    registrationNumber: 2, firstName: "FN", lastName: "LN", birthday: new DateOnly(2004, 3, 2), salary: 3000M, address: null,
                    carType: "Audi A8");
                db.Employees.AddRange(employee1, employee2);
                db.SaveChanges();
            });

            // ACT
            // Send GET /api/employees?type=(casher|manager)
            var (statusCode, content) =
                await factory.GetHttpContent<List<EmployeeDto>>($"/api/employees?type={type}");

            // ASSERT
            Assert.True(statusCode == System.Net.HttpStatusCode.OK);
            Assert.NotNull(content);
            Assert.True(content.First().RegistrationNumber == expectedId);
        }

        [Theory]
        [InlineData("", HttpStatusCode.BadRequest)]
        [InlineData("LN", HttpStatusCode.Created)]
        public async Task AddManagerTest(string lastname, HttpStatusCode expectedStatusCode)
        {
            // ARRANGE
            var factory = new TestWebApplicationFactory();
            // Wichtig: Man muss InitializeDatabase aufrufen, denn hier wird mit
            // EnsureDeleted() die Db vom vorigem Testdurchlauf gelöscht.
            factory.InitializeDatabase(db => { });
            var cmd = new NewManagerCmd(
                1, "FN", lastname, new DateOnly(2000, 1, 1), 3000M,
                null, "SUV");

            // ACT
            var (statusCode, content) = await factory.PostHttpContent("/api/manager", cmd);

            // ASSERT
            Assert.True(statusCode == expectedStatusCode);
            if (expectedStatusCode == HttpStatusCode.Created)
            {
                // Im Erfolgsfall liefert der Controller
                // { "registrationNumber": 1 }
                Assert.True(content.GetProperty("registrationNumber").GetInt32() == 1);
                var managerFromDb = factory.QueryDatabase(db => db.Managers.First());
                Assert.True(managerFromDb.RegistrationNumber == 1);
            }
        }
    }
}
