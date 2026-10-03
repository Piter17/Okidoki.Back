using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Okidoki.Infrastructure.Exceptions;

public static class DbExceptions
{
    extension(DbUpdateException exception)
    {
        public bool IsUniqueConstraintViolation(string keyName) =>
            exception.InnerException is SqlException sqlException
                && (sqlException.Number == 2601 || sqlException.Number == 2627)
                && sqlException.Message.Contains(keyName);
    }

}
