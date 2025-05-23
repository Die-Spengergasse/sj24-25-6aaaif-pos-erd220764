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
    public class PaymentServiceTests
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

        [Theory]
        [InlineData(10, "Cash", 1, "Invalid cash desk")]
        [InlineData(1, "Cash", 10, "Invalid employee")]
        [InlineData(1, "XXX", 1, "Invalid payment type")]
        [InlineData(2, "Cash", 1, "Open payment for cashdesk")]
        [InlineData(1, "CreditCard", 1, "Insufficient rights to create a credit card payment.")]
        public void CreatePaymentThrowsServiceExceptionTest(
            int cashDeskNumber, string paymentType,
            int employeeRegistrationNumber, string message)
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var service = new PaymentService(db);
            var cashDesk = new CashDesk(number: 1);
            var cashDesk2 = new CashDesk(number: 2);
            var employee = new Cashier(
                registrationNumber: 1, firstName: "FN", lastName: "LN",
                birthday: new DateOnly(2004, 1, 1),
                salary: 3000M, address: null, jobSpezialisation: "Feinkost");
            // Confirmed ist null, da es im Konstruktor nicht gesetzt wird.
            // D. h. das payment ist nicht confirmed.
            var payment = new Payment(
                cashDesk: cashDesk2, paymentDateTime: new DateTime(2025, 5, 3),
                employee: employee, PaymentType.Cash);
            db.AddRange(cashDesk, cashDesk2, employee, payment);
            db.SaveChanges();

            // ACT & ASSERT

            // "Invalid cash desk"
            var cmd = new NewPaymentCommand(
                CashDeskNumber: cashDeskNumber, PaymentType: paymentType, EmployeeRegistrationNumber: employeeRegistrationNumber);
            var e = Assert.Throws<PaymentServiceException>(() => service.CreatePayment(cmd));
            Assert.True(e.Message == message);
            // ASSERT
        }

        [Fact]
        public void CreatePaymentSuccessTest()
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var service = new PaymentService(db);
            var cashDesk = new CashDesk(number: 1);
            var employee = new Cashier(
                registrationNumber: 1, firstName: "FN", lastName: "LN",
                birthday: new DateOnly(2004, 1, 1),
                salary: 3000M, address: null, jobSpezialisation: "Feinkost");
            db.AddRange(cashDesk, employee);
            db.SaveChanges();

            // ACT
            var cmd = new NewPaymentCommand(
                CashDeskNumber: 1, PaymentType: "Cash", EmployeeRegistrationNumber: 1);
            service.CreatePayment(cmd);

            // ASSERT
            db.ChangeTracker.Clear();
            var paymentFromDb = db.Payments.First();
            Assert.True(paymentFromDb.Id != 0);

        }
    }
}
