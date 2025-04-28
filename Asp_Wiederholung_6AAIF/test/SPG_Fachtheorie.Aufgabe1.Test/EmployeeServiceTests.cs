using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe1.Test
{
    [Collection("Sequential")]
    public class EmployeeServiceTests
    {
        private AppointmentContext GetEmptyDbContext()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite(@"Data Source=cash.db")
                .Options;

            var db = new AppointmentContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            return db;
        }

        [Fact]
        public void AddManagerSuccessTest()
        {
            // ARRANGE
            var db = GetEmptyDbContext();
            var service = new EmployeeService(db);
            var newManagerCmd = new NewManagerCmd(
                1001, "FN", "LN", new DateOnly(2000, 1, 1), 2000, null, "SUV");

            // ACT
            service.AddManager(newManagerCmd);

            // ASSERT
            db.ChangeTracker.Clear();
            var managerFromDb = db.Managers.First();
            Assert.True(managerFromDb.RegistrationNumber == 1001);
        }
        [Fact]
        public void DeleteEmployeeSuccessTest()
        {
            // ARRANGE
            var db = GetEmptyDbContext();
            var service = new EmployeeService(db);
            var employee = new Manager(
                1001, "FN", "LN", new DateOnly(2000, 1, 1), null, null, "SUV");
            var cashDesk = new CashDesk(1);
            var payment = new Payment(
                cashDesk, new DateTime(2025, 4, 7, 17, 0, 0), employee, PaymentType.Cash);
            var paymentItem = new PaymentItem("Monster", 2, 2.5M, payment);
            db.PaymentItems.Add(paymentItem);
            db.SaveChanges();

            // ACT
            service.DeleteEmployee(1001);

            // ASSERT
            db.ChangeTracker.Clear();
            Assert.False(db.Employees.Any());
        }

        [Theory]
        [InlineData(1, "2025-04-05T12:00:00", "Manager not found")]    // Fall ungültiger Manager (id nicht gefunden)
        [InlineData(1001, "2025-04-05T13:00:00", "Manager has changed")] // Fall LastUpdate entspricht nicht dem Wert in der DB
        [InlineData(1001, "2025-04-05T12:00:00", null)] // Fall OK
        public void UpdateManagerSuccessTest(int employeeId, string lastUpdateStr, string? expectedErrorMessage)
        {
            // ARRANGE
            var lastUpdate = DateTime.Parse(lastUpdateStr);
            var db = GetEmptyDbContext();
            var service = new EmployeeService(db);
            var employee = new Manager(
                1001, "FN", "LN", new DateOnly(2000, 1, 1), null, null, "SUV");
            employee.LastUpdate = new DateTime(2025, 4, 5, 12, 0, 0);
            db.Employees.Add(employee);
            db.SaveChanges();
            var updateManagerCmd = new UpdateManagerCmd(
                employeeId, "FN2", "LN2", new AddressCmd("Street", "1010", "Wien"),
                "SUV2", lastUpdate);

            // ASSERT
            // Wurde als Parameter eine expectedErrorMessage übergeben?
            // Dann prüfe mit Assert.Throws()
            if (!string.IsNullOrEmpty(expectedErrorMessage))
            {
                var e = Assert.Throws<EmployeeServiceException>(() => service.UpdateManager(updateManagerCmd));
                Assert.True(e.Message == expectedErrorMessage);
                return;
            }
            service.UpdateManager(updateManagerCmd);
            db.ChangeTracker.Clear();
            var managerFromDb = db.Managers.First();
            Assert.True(managerFromDb.FirstName == "FN2");
            Assert.True(managerFromDb.LastName == "LN2");
            Assert.True(managerFromDb.CarType == "SUV2");
            Assert.True(managerFromDb.Address?.Street == "Street");
            Assert.True(managerFromDb.Address?.Zip == "1010");
            Assert.True(managerFromDb.Address?.City == "Wien");
        }

        [Theory]
        [InlineData(1, "Manager not found")]    // Fall ungültiger Manager (id nicht gefunden)
        [InlineData(1001, null)] // Fall LastUpdate entspricht nicht dem Wert in der DB
        public void UpdateAddressSuccessTest(int registrationNumber,  string? expectedErrorMessage)
        {
            // ARRANGE
            var db = GetEmptyDbContext();
            var service = new EmployeeService(db);
            var employee = new Manager(
                1001, "FN", "LN", new DateOnly(2000, 1, 1), null, null, "SUV");
            db.Employees.Add(employee);
            db.SaveChanges();
            var addressCmd = new AddressCmd("Street", "Zip", "City");

            // ACT
            if (!string.IsNullOrEmpty(expectedErrorMessage))
            {
                var e = Assert.Throws<EmployeeServiceException>(
                    () => service.UpdateAddress(registrationNumber, addressCmd));
                Assert.True(e.Message == expectedErrorMessage);
                return;
            }
            service.UpdateAddress(registrationNumber, addressCmd);
            db.ChangeTracker.Clear();
            var managerFromDb = db.Managers.First();
            Assert.True(managerFromDb.Address?.Street == "Street");
            Assert.True(managerFromDb.Address?.Zip == "Zip");
            Assert.True(managerFromDb.Address?.City == "City");
        }
    }
}
