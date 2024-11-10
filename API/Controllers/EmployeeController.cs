using Dto;
using Microsoft.AspNetCore.Mvc;
using Repository.Roster.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // GET: api/Employee
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _employeeRepository.GetEmployeesAsync();
            return Ok(employees);
        }

        // GET: api/Employee/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _employeeRepository.GetEmployeeAsync(id);
            if (employee == null)
                return BadRequest("No se encontro ningun registro");

            return Ok(employee);
        }

        // POST: api/Employee
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeDto employee)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdEmployee = await _employeeRepository.CreateEmployeeAsync(employee);
                return CreatedAtAction(nameof(GetEmployee), new { id = createdEmployee.Id }, createdEmployee);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Employee/{id}
        [HttpPut("")]
        public async Task<IActionResult> UpdateEmployee([FromBody] EmployeeDto employee)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedEmployee = await _employeeRepository.UpdateEmployeeAsync(employee);
            if (updatedEmployee == null)
                return NotFound();

            return Ok(updatedEmployee);
        }

        // DELETE: api/Employee/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var result = await _employeeRepository.DeleteEmployeeAsync(id);
            if (!result)
                return BadRequest("No se encontro ningun registro");

            return NoContent();
        }

        // GET: api/Employee/{rfc}
        [HttpGet("exist-rfc/{rfc}")]
        public async Task<IActionResult> GetIsExistsRfc(string rfc)
        {
            if (string.IsNullOrEmpty(rfc))
                return BadRequest("No selecciono un rfc");

            var result = await _employeeRepository.GetCheckRfcAsync(rfc);
            return Ok(result);
        }
    }
}
