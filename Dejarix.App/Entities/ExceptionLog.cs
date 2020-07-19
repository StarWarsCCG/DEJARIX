using System;

namespace Dejarix.App.Entities
{
    public class ExceptionLog
    {
        public Guid ExceptionId { get; set; }
        public int Ordinal { get; set; }
        public DateTimeOffset ExceptionDate { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionStackTrace { get; set; }

        public static ExceptionLog FromException(Exception exception, Guid id, int ordinal, DateTimeOffset date)
        {
            return new ExceptionLog
            {
                ExceptionId = id,
                Ordinal = ordinal,
                ExceptionDate = date,
                ExceptionType = exception.GetType().ToString(),
                ExceptionMessage = exception.Message,
                ExceptionStackTrace = exception.StackTrace
            };
        }
    }
}