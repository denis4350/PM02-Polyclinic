using System;

namespace HospitalSystem.Helpers
{
    public static class AuditHelper
    {
        public static void Log(string action, string tableName)
        {
            var currentUser = SessionManager.CurrentUser;
            if (currentUser == null) return; // до входа в систему логировать некого

            Log(action, tableName, currentUser.UserID);
        }

        // Перегрузка для случаев, когда действие относится не к текущему
        // вошедшему пользователю (например — неудачная попытка входа другого)
        public static void Log(string action, string tableName, int userId)
        {
            try
            {
                using (var db = new HospitalDBEntities())
                {
                    db.AuditLogs.Add(new AuditLogs
                    {
                        UserID = userId,
                        Action = action,
                        TableName = tableName,
                        ActionDate = DateTime.Now
                    });

                    db.SaveChanges();
                }
            }
            catch
            {
                // Аудит — вспомогательная вещь. Если запись лога не удалась,
                // основное действие уже прошло успешно и не должно из-за этого
                // выглядеть как ошибка
            }
        }
    }
}