using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using System;
using System.Linq;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe1.Test
{
    [Collection("Sequential")]
    public class PaymentServiceTests
    {
        private AppointmentContext GetEmptyDbContext()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite("Data Source=cash.db")
                .Options;

            var db = new AppointmentContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            return db;
        }

        [Theory]
        [InlineData(999, "Cash", 1, "Invalid cash desk")]
        [InlineData(1, "Cash", 999, "Invalid employee")]
        [InlineData(1, "InvalidType", 1, "Invalid payment type")]
        [InlineData(1, "CreditCard", 1, "Insufficient rights to create a credit card payment.")]
        public void CreatePaymentExceptionsTest(int cashDeskNumber, string paymentType, int employeeRegistrationNumber, string expectedMessage)
        {
            // ARRANGE
            var db = GetEmptyDbContext();
            db.CashDesks.Add(new CashDesk(1));
            db.Employees.Add(new Cashier(1, "John", "Doe", new DateOnly(1990, 1, 1), null, null, "Food"));
            db.SaveChanges();

            var service = new PaymentService(db);
            var cmd = new NewPaymentCommand(cashDeskNumber, paymentType, employeeRegistrationNumber);

            // ACT & ASSERT
            var ex = Assert.Throws<PaymentServiceException>(() => service.CreatePayment(cmd));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void CreatePaymentSuccessTest()
        {
            // ARRANGE
            var db = GetEmptyDbContext();
            db.CashDesks.Add(new CashDesk(1));
            db.Employees.Add(new Manager(1, "Jane", "Doe", new DateOnly(1980, 1, 1), null, null, "SUV"));
            db.SaveChanges();

            var service = new PaymentService(db);
            var cmd = new NewPaymentCommand(1, "CreditCard", 1);

            // ACT
            var payment = service.CreatePayment(cmd);

            // ASSERT
            db.ChangeTracker.Clear();
            Assert.NotNull(db.Payments.FirstOrDefault(p => p.Id == payment.Id));
        }

        [Fact]
        public void ConfirmPaymentSuccessTest()
        {
            // ARRANGE
            var db = GetEmptyDbContext();
            var employee = new Manager(1, "Jane", "Doe", new DateOnly(1980, 1, 1), null, null, "SUV");
            var cashDesk = new CashDesk(1);
            var payment = new Payment(cashDesk, DateTime.UtcNow, employee, PaymentType.Cash);
            db.Payments.Add(payment);
            db.SaveChanges();

            var service = new PaymentService(db);

            // ACT
            service.ConfirmPayment(payment.Id);

            // ASSERT
            db.ChangeTracker.Clear();
            Assert.NotNull(db.Payments.First().Confirmed);
        }

        [Fact]
        public void ConfirmPaymentNotFoundTest()
        {
            // ARRANGE
            var db = GetEmptyDbContext();
            var service = new PaymentService(db);

            // ACT & ASSERT
            var ex = Assert.Throws<PaymentServiceException>(() => service.ConfirmPayment(999));
            Assert.Equal("Payment not found", ex.Message);
            Assert.True(ex.NotFoundException);
        }

        [Fact]
        public void AddPaymentItemSuccessTest()
        {
            // ARRANGE
            var db = GetEmptyDbContext();
            var employee = new Manager(1, "Jane", "Doe", new DateOnly(1980, 1, 1), null, null, "SUV");
            var cashDesk = new CashDesk(1);
            var payment = new Payment(cashDesk, DateTime.UtcNow, employee, PaymentType.Cash);
            db.Payments.Add(payment);
            db.SaveChanges();

            var service = new PaymentService(db);
            var cmd = new NewPaymentItemCommand("Water", 2, 1.5M, payment.Id);

            // ACT
            service.AddPaymentItem(cmd);

            // ASSERT
            db.ChangeTracker.Clear();
            Assert.Single(db.PaymentItems);
        }

        [Fact]
        public void AddPaymentItemPaymentNotFoundTest()
        {
            // ARRANGE
            var db = GetEmptyDbContext();
            var service = new PaymentService(db);
            var cmd = new NewPaymentItemCommand("Water", 2, 1.5M, 999);

            // ACT & ASSERT
            var ex = Assert.Throws<PaymentServiceException>(() => service.AddPaymentItem(cmd));
            Assert.Equal("Payment not found", ex.Message);
        }

        [Fact]
        public void AddPaymentItemAlreadyConfirmedTest()
        {
            // ARRANGE
            var db = GetEmptyDbContext();
            var employee = new Manager(1, "Jane", "Doe", new DateOnly(1980, 1, 1), null, null, "SUV");
            var cashDesk = new CashDesk(1);
            var payment = new Payment(cashDesk, DateTime.UtcNow, employee, PaymentType.Cash);
            payment.Confirmed = DateTime.UtcNow;
            db.Payments.Add(payment);
            db.SaveChanges();

            var service = new PaymentService(db);
            var cmd = new NewPaymentItemCommand("Water", 2, 1.5M, payment.Id);

            // ACT & ASSERT
            var ex = Assert.Throws<PaymentServiceException>(() => service.AddPaymentItem(cmd));
            Assert.Equal("Payment already confirmed.", ex.Message);
        }

        [Fact]
        public void DeletePaymentSuccessTest()
        {
            // ARRANGE
            var db = GetEmptyDbContext();
            var employee = new Manager(1, "Jane", "Doe", new DateOnly(1980, 1, 1), null, null, "SUV");
            var cashDesk = new CashDesk(1);
            var payment = new Payment(cashDesk, DateTime.UtcNow, employee, PaymentType.Cash);
            var paymentItem = new PaymentItem("Water", 2, 1.5M, payment);
            db.Payments.Add(payment);
            db.PaymentItems.Add(paymentItem);
            db.SaveChanges();

            var service = new PaymentService(db);

            // ACT
            service.DeletePayment(payment.Id, true);

            // ASSERT
            db.ChangeTracker.Clear();
            Assert.False(db.Payments.Any());
            Assert.False(db.PaymentItems.Any());
        }
    }
}
