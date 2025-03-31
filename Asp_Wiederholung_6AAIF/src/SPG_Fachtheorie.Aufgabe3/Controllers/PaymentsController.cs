using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe3.Commands;
using SPG_Fachtheorie.Aufgabe3.Dtos;
using System;
using System.Threading.Tasks;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]  // --> /api/payments
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppointmentContext _db;

        public PaymentsController(AppointmentContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET /api/payments
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<PaymentDto>> GetAllPayments(
            [FromQuery] int? cashDesk, [FromQuery] DateTime? dateFrom)
        {
            var payments = _db.Payments
                .Where(p =>
                    cashDesk.HasValue
                        ? p.CashDesk.Number == cashDesk.Value : true)
                .Where(p =>
                    dateFrom.HasValue
                        ? p.PaymentDateTime >= dateFrom.Value : true)
                .Select(p => new PaymentDto(
                    p.Id, p.Employee.FirstName, p.Employee.LastName,
                    p.PaymentDateTime,
                    p.CashDesk.Number, p.PaymentType.ToString(),
                    p.PaymentItems.Sum(p => p.Amount)))
                .ToList();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<PaymentDetailDto> GetPaymentDetail(int id)
        {
            var payment = _db.Payments
                .Where(p => p.Id == id)
                .Select(p => new PaymentDetailDto(
                    p.Id, p.Employee.FirstName, p.Employee.LastName,
                    p.CashDesk.Number, p.PaymentType.ToString(),
                    p.PaymentItems
                        .Select(pi => new PaymentItemDto(
                            pi.ArticleName, pi.Amount, pi.Price))
                        .ToList()
                    ))
                .FirstOrDefault();
            if (payment is null)
                return NotFound();
            return Ok(payment);
        }

        /// <summary>
        /// POST /api/payments
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddPayment(NewPaymentCommand cmd)
        {
            var cashDesk = _db.CashDesks.FirstOrDefault(c => c.Number == cmd.CashDeskNumber);
            if (cashDesk is null) return Problem("Invalid cash desk", statusCode: 400);
            var employee = _db.Employees.FirstOrDefault(e => e.RegistrationNumber == cmd.EmployeeRegistrationNumber);
            if (employee is null) return Problem("Invalid employee", statusCode: 400);

            if (!Enum.TryParse<PaymentType>(cmd.PaymentType, out var paymentType))
                return Problem("Invalid payment type", statusCode: 400);

            var payment = new Payment(
                cashDesk, cmd.PaymentDateTime, employee, paymentType);
            _db.Payments.Add(payment);
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                return Problem(e.InnerException?.Message ?? e.Message);
            }
            return CreatedAtAction(nameof(AddPayment), new { Id = payment.Id });
        }

        /// <summary>
        /// DELETE /api/payments/{id}?deleteItems=true|false
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeletePayment(int id, [FromQuery] bool deleteItems)
        {
            var payment = _db.Payments.FirstOrDefault(p => p.Id == id);
            if (payment is null) return NoContent();
            var paymentItems = _db.PaymentItems.Where(p => p.Payment.Id == id).ToList();
            if (paymentItems.Any() && deleteItems)
            {
                try
                {
                    _db.PaymentItems.RemoveRange(paymentItems);
                    _db.SaveChanges();
                }
                catch (DbUpdateException e)
                {
                    return Problem(e.InnerException?.Message ?? e.Message, statusCode: 400);
                }
                catch (InvalidOperationException e)
                {
                    return Problem(
                        e.InnerException?.Message ?? e.Message,
                        statusCode: StatusCodes.Status400BadRequest);
                }
            }
            try
            {
                _db.Payments.Remove(payment);
                _db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                return Problem(e.InnerException?.Message ?? e.Message, statusCode: 400);
            }
            catch (InvalidOperationException e)
            {
                return Problem(
                    e.InnerException?.Message ?? e.Message,
                    statusCode: StatusCodes.Status400BadRequest);
            }
            return NoContent();
        }

        [HttpPut("paymentItems/{id}")]
        public async Task<IActionResult> UpdatePaymentItem(int id, [FromBody] UpdatePaymentItemCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Invalid payment item ID"
                });
            }

            var paymentItem = await _db.PaymentItems.FindAsync(id);
            if (paymentItem == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = 404,
                    Title = "Payment Item not found"
                });
            }

            if (paymentItem.LastUpdated != command.LastUpdated)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Payment item has changed"
                });
            }

            var payment = await _db.Payments.FindAsync(command.PaymentId);
            if (payment == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Invalid payment ID"
                });
            }

            paymentItem.ArticleName = command.ArticleName;
            paymentItem.Amount = command.Amount;
            paymentItem.Price = command.Price;
            paymentItem.PaymentId = command.PaymentId;
            paymentItem.LastUpdated = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateConfirmed(int id, [FromBody] UpdateConfirmedCommand command)
        {
            var payment = await _db.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = 404,
                    Title = "Payment not found"
                });
            }

            if (payment.Confirmed.HasValue)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Payment already confirmed"
                });
            }

            payment.Confirmed = command.Confirmed;
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
