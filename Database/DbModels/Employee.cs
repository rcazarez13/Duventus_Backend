
using Database.Common;
using System;
using System.ComponentModel.DataAnnotations;
namespace Database.DbModels
{ 
    public class Employee : BaseModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido paterno no puede tener más de 50 caracteres.")]
        public string LastName { get; set; }

        [StringLength(50, ErrorMessage = "El apellido materno no puede tener más de 50 caracteres.")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "El RFC es obligatorio.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "El RFC debe tener exactamente 13 caracteres.")]
        [RegularExpression(@"^[A-Z]{4}\d{6}[A-Z0-9]{3}$", ErrorMessage = "El RFC no tiene un formato válido.")]
        public string RFC { get; set; }

        [DataType(DataType.Date, ErrorMessage = "La fecha de nacimiento no tiene un formato válido.")]
        [CustomValidation(typeof(Employee), "ValidateFechaNacimiento")]
        public DateTime DateOfBirth { get; set; }

        [RegularExpression(@"^(local|externo)$", ErrorMessage = "El tipo de empleado debe ser 'local' o 'externo'.")]
        public string EmployeeType { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El salario por hora debe ser mayor que 0.")]
        public decimal HourlySalary { get; set; }

        [Range(0, 168, ErrorMessage = "Las horas trabajadas por semana deben estar entre 0 y 168.")]
        public int HoursWorkedPerWeek { get; set; }

        // Método de validación personalizada para FechaNacimiento
        public static ValidationResult ValidateFechaNacimiento(DateTime DateOfBirth)
        {
            if (DateOfBirth > DateTime.Now)
            {
                return new ValidationResult("La fecha de nacimiento no puede ser futura.");
            }
            return ValidationResult.Success;
        }
    }

}
