using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using SPG_Fachtheorie.Aufgabe3.Dtos;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]  // [controller] --> Name vor "Controller" --> /api/employees
    [ApiController]              // Wird als controller berücksichtigt.
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeService _service;

        public EmployeesController(EmployeeService service)
        {
            _service = service;
        }

        // Verknüpfen von Adresse mit Methode.
        // HttpGet -> GET Request
        // Keine Adresse -> Adresse des Controllers
        // ==> Reagiert auf GET /api/employees
        //                  GET /api/employees?type=manager
        //                  GET /api/employees?type=MANAGER
        [HttpGet]
        public ActionResult<List<EmployeeDto>> GetAllEmployees([FromQuery] string? type)
        {
            return Ok(_service.Employees
                .Where(e => string.IsNullOrEmpty(type) ? true : e.Type.ToLower() == type.ToLower())
                .Select(e => new EmployeeDto(
                    e.RegistrationNumber, e.Type,
                    e.LastName,
                    e.FirstName))
                .ToList());
        }

        /// <summary>
        /// /api/employees/1001 --> 1001 wird in registrationNumber gelegt.
        /// </summary>
        /// <param name="registrationNumber"></param>
        /// <returns></returns>
        [HttpGet("{registrationNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<EmployeeDetailDto> GetEmployeeById(int registrationNumber)
        {
            var employee = _service.Employees
                .Where(e => e.RegistrationNumber == registrationNumber)
                .Select(e => new EmployeeDetailDto(
                    e.RegistrationNumber, e.LastName,
                    e.FirstName, e.Address,
                    e.Payments.Count()))
                .AsNoTracking()
                .FirstOrDefault();
            if (employee is null)
                return NotFound();  // Return 404 not found.
            return Ok(employee);
        }

        /// <summary>
        /// Reagiert auf POST /api/manager
        /// </summary>
        [HttpPost("/api/manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddManager([FromBody] NewManagerCmd cmd) =>
            CallServiceMethod(() =>
            {
                var manager = _service.AddManager(cmd);
                return CreatedAtAction(nameof(AddManager),
                    new { manager.RegistrationNumber });
            });

        /// <summary>
        /// DELETE /api/employee/{registrationNumber}
        /// Löscht den Employee. Da ON DELETE RESTRICT gesetzt wurde, müssen wir alle verbundenen
        /// Daten (Payments und PaymentItems) auch löschen.
        /// Ob das sinnvoll ist, muss immer geprüft werden. Eine Alternative wäre ein "soft delete",
        /// also ein Flag "visible" in Employee einfügen.
        /// </summary>
        [HttpDelete("{registrationNumber}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteEmployee(int registrationNumber) =>
            CallServiceMethod(() =>
            {
                _service.DeleteEmployee(registrationNumber);
                return NoContent();
            });

        /// <summary>
        /// PUT /api/manager/{registrationNumber}
        /// </summary>
        [HttpPut("/api/manager/{registrationNumber}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateManager(int registrationNumber, [FromBody] UpdateManagerCmd cmd)
        {
            if (registrationNumber != cmd.RegistrationNumber)
                return Problem("Invalid registration number", statusCode: 400);
            return CallServiceMethod(() =>
            {
                UpdateManager(registrationNumber, cmd);
                return NoContent();
            });
        }


        [HttpPatch("/api/manager/{registrationNumber}/address")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateAddress(int registrationNumber, [FromBody] AddressCmd cmd) =>
            CallServiceMethod(() =>
            {
                _service.UpdateAddress(registrationNumber, cmd);
                return NoContent();
            });

        private IActionResult CallServiceMethod(Func<IActionResult> serviceCall)
        {
            try
            {
                return serviceCall();
            }
            catch (EmployeeServiceException e) when (e.NotFoundException)
            {
                return Problem(e.Message, statusCode: 404);
            }
            catch (EmployeeServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }
    }
}
