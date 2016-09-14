using System;
using ALinq;
using System.Reflection;

namespace ALinq.SqlClient
{
    /// <summary>
    /// Provides methods that correspond to SQL Server functions. Methods in the ALinq.SqlClient.SqlMethods class are only supported in LINQ to SQL queries.
    /// </summary>
    public static class SqlMethods
    {
        /// <summary>
        /// Counts the number of day boundaries between two non-nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>The number of day boundaries between the two specified dates.</returns>
        public static int DateDiffDay(DateTime startDate, DateTime endDate)
        {
            var span = endDate.Date - startDate.Date;
            return span.Days;
        }

        /// <summary>
        /// Counts the number of day boundaries between two nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>When both parameters are not null, returns the number of day boundaries between the two specified dates. When one or both parameters are null, returns a null value.</returns>
        public static int? DateDiffDay(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                return DateDiffDay(startDate.Value, endDate.Value);
            }
            return null;
        }

        /// <summary>
        /// Counts the number of hour boundaries between two nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>When both parameters are not null, returns the number of hour boundaries between the two specified dates. When one or both parameters are null, returns a null value.</returns>
        public static int? DateDiffHour(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                return DateDiffHour(startDate.Value, endDate.Value);
            }
            return null;
        }

        /// <summary>
        /// Counts the number of hour boundaries between two non-nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>The number of hour boundaries between the two specified dates.</returns>
        public static int DateDiffHour(DateTime startDate, DateTime endDate)
        {
            return (((DateDiffDay(startDate, endDate) * 0x18) + endDate.Hour) - startDate.Hour);
        }

        /// <summary>
        /// Counts the number of microsecond boundaries between two non-nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>The number of microsecond boundaries between the two specified dates.</returns>
        public static int DateDiffMillisecond(DateTime startDate, DateTime endDate)
        {
            return (((DateDiffSecond(startDate, endDate) * 0x3e8) + endDate.Millisecond) - startDate.Millisecond);
        }

        /// <summary>
        /// Counts the number of microsecond boundaries between two nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>When both parameters are not null, returns the number of microsecond boundaries between the two specified dates. When one or both parameters are null, returns a null value.</returns>
        public static int? DateDiffMillisecond(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                return DateDiffMillisecond(startDate.Value, endDate.Value);
            }
            return null;
        }

        /// <summary>
        /// Counts the number of minute boundaries between two non-nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>The number of minute boundaries between the two specified dates.</returns>
        public static int DateDiffMinute(DateTime startDate, DateTime endDate)
        {
            return (((DateDiffHour(startDate, endDate) * 60) + endDate.Minute) - startDate.Minute);
        }

        /// <summary>
        /// Counts the number of minute boundaries between two nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>When both parameters are not null, returns the number of minute boundaries between the two specified dates. When one or both parameters are null, returns a null value.</returns>
        public static int? DateDiffMinute(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                return DateDiffMinute(startDate.Value, endDate.Value);
            }
            return null;
        }

        /// <summary>
        /// Counts the number of month boundaries between two nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>When both parameters are not null, returns the number of month boundaries between the two specified dates. When one or both parameters are null, returns a null value.</returns>
        public static int? DateDiffMonth(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                return DateDiffMonth(startDate.Value, endDate.Value);
            }
            return null;
        }

        /// <summary>
        /// Counts the number of month boundaries between two non-nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>The number of month boundaries between the two specified dates.</returns>
        public static int DateDiffMonth(DateTime startDate, DateTime endDate)
        {
            return (((12 * (endDate.Year - startDate.Year)) + endDate.Month) - startDate.Month);
        }

        /// <summary>
        /// Counts the number of second boundaries between two non-nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>The number of second boundaries between the two specified dates.</returns>
        public static int DateDiffSecond(DateTime startDate, DateTime endDate)
        {
            return (((DateDiffMinute(startDate, endDate) * 60) + endDate.Second) - startDate.Second);
        }

        /// <summary>
        /// Counts the number of second boundaries between two nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>When both parameters are not null, returns the number of second boundaries between the two specified dates is returned. When one or both parameters are null, returns a null value.</returns>
        public static int? DateDiffSecond(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                return DateDiffSecond(startDate.Value, endDate.Value);
            }
            return null;
        }

        /// <summary>
        /// Counts the number of year boundaries between two nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>When both parameters are not null, returns the number of year boundaries between the two specified dates. When one or both parameters are null, returns a null value.</returns>
        public static int? DateDiffYear(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                return DateDiffYear(startDate.Value, endDate.Value);
            }
            return null;
        }

        /// <summary>
        /// Counts the number of year boundaries between two non-nullable dates.
        /// </summary>
        /// <param name="startDate">The start date for the time period.</param>
        /// <param name="endDate">The end date for the time period.</param>
        /// <returns>The number of year boundaries between the two specified dates.</returns>
        public static int DateDiffYear(DateTime startDate, DateTime endDate)
        {
            return (endDate.Year - startDate.Year);
        }

        /// <summary>
        /// Determines whether a specific character string matches a specified pattern. This method is currently only supported in LINQ to SQL queries.
        /// </summary>
        /// <param name="matchExpression">The string to be searched for a match.</param>
        /// <param name="pattern">The pattern, which may include wildcard characters, to match in matchExpression.</param>
        /// <returns>true if matchExpression matches the pattern; otherwise, false.</returns>
        public static bool Like(string matchExpression, string pattern)
        {
            throw Error.SqlMethodOnlyForSql(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// Determines whether a specific character string matches a specified pattern. This method is currently only supported in LINQ to SQL queries.
        /// </summary>
        /// <param name="matchExpression">The string to be searched for a match.</param>
        /// <param name="pattern">The pattern, which may include wildcard characters, to match in matchExpression.</param>
        /// <param name="escapeCharacter">The character to put in front of a wildcard character to indicate that it should be interpreted as a regular character and not as a wildcard character.</param>
        /// <returns>true if matchExpression matches the pattern; otherwise, false.</returns>
        public static bool Like(string matchExpression, string pattern, char escapeCharacter)
        {
            throw Error.SqlMethodOnlyForSql(MethodBase.GetCurrentMethod());
        }

        internal static int RawLength(Binary value)
        {
            throw Error.SqlMethodOnlyForSql(MethodBase.GetCurrentMethod());
        }

        internal static int RawLength(byte[] value)
        {
            throw Error.SqlMethodOnlyForSql(MethodBase.GetCurrentMethod());
        }

        internal static int RawLength(string value)
        {
            throw Error.SqlMethodOnlyForSql(MethodBase.GetCurrentMethod());
        }

        //internal static int Identity(this DataContext dataContext)
        //{
        //    throw Error.SqlMethodOnlyForSql(MethodBase.GetCurrentMethod());
        //}
    }
}