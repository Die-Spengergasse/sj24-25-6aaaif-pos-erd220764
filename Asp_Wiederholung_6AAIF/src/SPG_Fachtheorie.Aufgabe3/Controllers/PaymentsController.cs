using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using SPG_Fachtheorie.Aufgabe3.Dtos;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]  // --> /api/payments
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _service;

        public PaymentsController(PaymentService service)
        {
            _service = service;
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
            var payments = _service.Payments
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
            var payment = _service.Payments
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
            try
            {
                var payment = _service.CreatePayment(cmd);
                return CreatedAtAction(nameof(AddPayment), new { Id = payment.Id });
            }
            catch (PaymentServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
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
            try
            {
                _service.DeletePayment(id, deleteItems);
                return NoContent();
            }
            catch (PaymentServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }

        //[HttpPut("/api/paymentItems/{id}")]
        //public IActionResult UpdatePaymentItem(int id, UpdatePaymentItemCommand cmd)
        //{
        //    if (id != cmd.Id)
        //        return Problem("Invalid payment item ID", statusCode: 400);

        //    var paymentItem = _db.PaymentItems.FirstOrDefault(p => p.Id == id);
        //    if (paymentItem is null)
        //        return Problem("Payment Item not found", statusCode: 404);
        //    var payment = _db.Payments.FirstOrDefault(p => p.Id == cmd.PaymentId);
        //    if (payment is null)
        //        return Problem("Payment not found", statusCode: 400);
        //    if (paymentItem.LastUpdated != cmd.LastUpdated)
        //        return Problem("Payment item has changed", statusCode: 400);

        //    paymentItem.ArticleName = cmd.ArticleName;
        //    paymentItem.Amount = cmd.Amount;
        //    paymentItem.Price = cmd.Price;
        //    paymentItem.LastUpdated = DateTime.UtcNow;
        //    return TrySave(new NoContentResult());
        //}

        [HttpPatch("{id}")]
        public IActionResult UpdateConfirmed(int id)
        {
            try
            {
                _service.ConfirmPayment(id);
                return NoContent();
            }
            catch (PaymentServiceException e) when (e.NotFoundException)
            {
                return Problem(e.Message, statusCode: 404);
            }
            catch (PaymentServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }
    }
}
