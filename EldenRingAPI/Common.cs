using System.Globalization;
using System.Net;

namespace EldenRingAPI
{
    public class Response
    {
        public Response(HttpStatusCode statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
            Data = null;
        }
        public Response(HttpStatusCode statusCode, string message, object data)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
        }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }
    }

    public interface IResponse<T>
    {
        HttpStatusCode StatusCode { get; set; }
        string Message { get; set; }
        T Data { get; set; }
    }

    public class Results<T>
    {
        public long count { get; set; }
        public decimal pages { get; set; }
        public int page { get; set; }
        public List<T> items { get; set; }
    }


    public static class Common
    {
        public static DateTime ParseISO(string date)
        {
            return DateTime.Parse(date, CultureInfo.InvariantCulture);
        }

        public static string ToISO()
        {
            return ToISO(DateTime.UtcNow);
        }
        public static string ToISO(DateTime date)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc).ToString("o", CultureInfo.InvariantCulture);
        }
    }
}
