using System.Net;

namespace Domain.Exceptions
{
    public class BusinessException : Exception
    {
        public HttpStatusCode Status { get; private set; }
        public string MessageStatus { get; private set; }
        public string DescriptionStatus { get; private set; }

        public BusinessException()
        {
            Status = HttpStatusCode.BadRequest;
            DescriptionStatus = string.Empty;
        }

        public BusinessException(HttpStatusCode status, string descriptionStatus, string message) : base(message)
        {
            Status = status;
            DescriptionStatus = descriptionStatus;
        }
        public BusinessException(HttpStatusCode status, string messageStatus, string descriptionStatus, string message) : base(message)
        {
            Status = status;
            MessageStatus = messageStatus;
            DescriptionStatus = descriptionStatus;
        }
    }
}
