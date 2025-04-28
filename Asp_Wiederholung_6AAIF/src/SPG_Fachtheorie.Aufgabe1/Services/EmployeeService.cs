using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Linq;

namespace SPG_Fachtheorie.Aufgabe1.Services
{
    public class EmployeeService
    {
        private readonly AppointmentContext _db;

        public EmployeeService(AppointmentContext db)
        {
            _db = db;
        }

        public IQueryable<Employee> Employees => _db.Employees.AsQueryable();
        public Manager AddManager(NewManagerCmd cmd)
        {
            var manager = new Manager(
                cmd.RegistrationNumber, cmd.FirstName, cmd.LastName,
                cmd.Birthdate, cmd.Salary,
                cmd.Address is not null
                    ? new Address(cmd.Address.Street, cmd.Address.Zip, cmd.Address.City)
                    : null,
                cmd.CarType);
            _db.Managers.Add(manager);
            SaveOrThrow();
            return manager;
        }


        public void DeleteEmployee(int registrationNumber)
        {
            var employee = _db.Employees
                .FirstOrDefault(e => e.RegistrationNumber == registrationNumber);
            if (employee is null) return;

            var paymentItems = _db.PaymentItems
                .Where(p => p.Payment.Employee.RegistrationNumber == registrationNumber)
                .ToList();
            var payments = _db.Payments
                .Where(p => p.Employee.RegistrationNumber == registrationNumber)
                .ToList();
            try
            {
                _db.PaymentItems.RemoveRange(paymentItems);
                _db.SaveChanges();

                _db.Payments.RemoveRange(payments);
                _db.SaveChanges();

                _db.Employees.Remove(employee);
                _db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                throw new EmployeeServiceException(e.InnerException?.Message ?? e.Message);
            }
            catch (InvalidOperationException e)
            {
                throw new EmployeeServiceException(e.InnerException?.Message ?? e.Message);
            }
        }

        public void UpdateManager(UpdateManagerCmd cmd)
        {
            var manager = _db.Managers
                .FirstOrDefault(m => m.RegistrationNumber == cmd.RegistrationNumber);
            if (manager is null)
                throw new EmployeeServiceException("Manager not found") { NotFoundException = true };
            if (manager.LastUpdate != cmd.LastUpdate)
                throw new EmployeeServiceException("Manager has changed");

            manager.FirstName = cmd.FirstName;
            manager.LastName = cmd.LastName;
            manager.CarType = cmd.CarType;
            manager.Address = cmd.Address is null
                ? null
                : new Address(cmd.Address.Street, cmd.Address.Zip, cmd.Address.City);
            manager.LastUpdate = DateTime.UtcNow;
            SaveOrThrow();
        }

        public void UpdateAddress(int registrationNumber, AddressCmd cmd)
        {
            var manager = _db.Managers
                .FirstOrDefault(m => m.RegistrationNumber == registrationNumber);
            if (manager is null)
                throw new EmployeeServiceException("Manager not found")
                { NotFoundException = true };

            manager.Address = new Address(cmd.Street, cmd.Zip, cmd.City);
            SaveOrThrow();
        }


        private void SaveOrThrow()
        {
            try
            {
                _db.SaveChanges();  // INSERT INTO
            }
            catch (DbUpdateException e)
            {
                throw new EmployeeServiceException(e.InnerException?.Message ?? e.Message);
            }
        }
    }
}
