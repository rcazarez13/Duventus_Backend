using Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Roster.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employee);
        Task<List<EmployeeDto>> GetEmployeesAsync();
        Task<EmployeeDto> GetEmployeeAsync(int id);
        Task<EmployeeDto> UpdateEmployeeAsync(EmployeeDto employee);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<bool> GetCheckRfcAsync(string rfc);

    }
}
