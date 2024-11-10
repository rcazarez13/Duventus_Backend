using Repository.Roster.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.DbContexts;
using Database.DbModels;
using Dto;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Repository.Shared;
using System.Reflection.Metadata.Ecma335;

namespace Repository.Roster.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;
        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        // Crear un nuevo empleado
        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employee)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (await GetCheckRfcAsync(employee.RFC))
                    throw new InvalidOperationException("El RFC ya existe");

                var employeeEntity = new Employee
                {
                    FirstName               = employee.FirstName,
                    LastName                = employee.LastName,
                    MiddleName              = employee.MiddleName,
                    RFC                     = employee.RFC,
                    DateOfBirth             = employee.DateOfBirth,
                    HourlySalary            = employee.HourlySalary,
                    HoursWorkedPerWeek      = employee.HoursWorkedPerWeek,
                    EmployeeType            = employee.EmployeeType,
                    CreatedAt               = DateTime.Now,
                    CreatedByApplicationUserId = Constants.USER_ID
                };

                _context.Add(employeeEntity);
                await _context.SaveChangesAsync();

                employee.Id = employeeEntity.Id;

                await transaction.CommitAsync();
                return employee;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        // Obtener una lista de empleados
        public async Task<List<EmployeeDto>> GetEmployeesAsync()
        {
            var employees = await Task.Run(() =>
            {
                return _context.Employees.AsEnumerable()
                .Select(x => new EmployeeDto
                {
                    Id                      = x.Id,
                    FirstName               = x.FirstName,
                    LastName                = x.LastName,
                    MiddleName              = x.MiddleName,
                    RFC                     = x.RFC,
                    DateOfBirth             = x.DateOfBirth,
                    HourlySalary            = x.HourlySalary,
                    HoursWorkedPerWeek      = x.HoursWorkedPerWeek,
                    EmployeeType            = x.EmployeeType,
                    CreatedAt               = x.CreatedAt,
                    SalaryReceived          = CalculateSalary(x.EmployeeType, x.HourlySalary, x.HoursWorkedPerWeek),
                    CreatedByApplicationUserId = x.CreatedByApplicationUserId,
                }).ToList();
            });

            return employees;
        }

        // Obtener un solo empleado
        public async Task<EmployeeDto> GetEmployeeAsync(int id)
        {
            var employee = await _context.Employees
                .Where(x => x.Id == id)
                .Select(x => new EmployeeDto
                {
                    Id = x.Id,
                    FirstName                  = x.FirstName,
                    LastName                   = x.LastName,
                    MiddleName                 = x.MiddleName,
                    RFC                        = x.RFC,
                    DateOfBirth                = x.DateOfBirth,
                    HourlySalary               = x.HourlySalary,
                    HoursWorkedPerWeek         = x.HoursWorkedPerWeek,
                    EmployeeType               = x.EmployeeType,
                    CreatedAt                  = x.CreatedAt,
                    CreatedByApplicationUserId = x.CreatedByApplicationUserId
                }).FirstOrDefaultAsync();

            return employee;
        }

        // Actualizar la información de un empleado
        public async Task<EmployeeDto> UpdateEmployeeAsync(EmployeeDto employee)
        {
            var existingEmployee = await _context.Employees.FindAsync(employee.Id);

            if (existingEmployee == null)
            {
                return null;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                existingEmployee.FirstName                  = employee.FirstName;
                existingEmployee.LastName                   = employee.LastName;
                existingEmployee.MiddleName                 = employee.MiddleName;
                existingEmployee.RFC                        = employee.RFC;
                existingEmployee.DateOfBirth                = employee.DateOfBirth;
                existingEmployee.HourlySalary               = employee.HourlySalary;
                existingEmployee.HoursWorkedPerWeek         = employee.HoursWorkedPerWeek;
                existingEmployee.EmployeeType               = employee.EmployeeType;
                existingEmployee.UpdatedAt                  = DateTime.Now;
                existingEmployee.UpdatedByApplicationUserId = Constants.USER_ID;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return employee;

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        // Eliminar un empleado
        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id);

            if (employee == null)
            {
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                employee.DeletedAt = DateTime.Now;
                employee.DeletedByApplicationUserId = Constants.USER_ID;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<bool> GetCheckRfcAsync(string rfc)
        {
            return await _context.Employees.AnyAsync(x => x.RFC == rfc);
        }

        // Salario basado en el tipo de empleado y las horas trabajadas
        private decimal CalculateSalary(string employeeType, decimal hourlySalary, decimal hoursWorkedPerWeek)
        {
            var salaryReceived = 0m;
            var isLocal = employeeType == Constants.EMPLOYEE_TYPE_LOCAL;

            var regularHours = isLocal
                ? Constants.EMPLOYEE_TYPE_LOCAL_HOURS 
                : Constants.EMPLOYEE_TYPE_EXTERNO_HOURS;

            var extraHours = hoursWorkedPerWeek - regularHours;
            if (extraHours <= 0)
            {
                // Salario base
                salaryReceived = hourlySalary * hoursWorkedPerWeek;
            }
            else if(isLocal)
            {
                var firstTierExtraHours = Math.Min(extraHours, 12);
                var secondTierExtraHours = Math.Max(0, extraHours - 12);

                salaryReceived = ((hourlySalary * regularHours)
                                 + (firstTierExtraHours * hourlySalary * 1.30m) // 30% extra por las primeras 12 horas
                                 + (secondTierExtraHours * hourlySalary * 1.60m)); // 60% extra por las demás horas
            }
            else 
            {
                salaryReceived = (hourlySalary * regularHours)
                                 + (extraHours * hourlySalary * 1.50m); // 50% extra por horas extras
            }

            return salaryReceived;
        }


    }
}

