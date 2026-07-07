using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalSystem.Helpers
{
    public static class NameHelper
    {
        // Собирает ФИО в одну строку. Если отчество не указано —
        // просто не добавляет лишний пробел вместо него
        public static string FullName(string lastName, string firstName, string middleName = null)
        {
            return string.IsNullOrWhiteSpace(middleName)
                ? $"{lastName} {firstName}".Trim()
                : $"{lastName} {firstName} {middleName}".Trim();
        }
    }
}
