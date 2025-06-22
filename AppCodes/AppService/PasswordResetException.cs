

namespace mvcDapper3.AppCodes.AppService
{
    public class PasswordResetException : Exception
    {
        public string UserFriendlyMessage { get; }

        public PasswordResetException(string message) : base(message)
        {
            UserFriendlyMessage = message;
        }

        public PasswordResetException(string message, Exception innerException) : base(message, innerException)
        {
            UserFriendlyMessage = message;
        }

        public PasswordResetException(string message, string userFriendlyMessage) : base(message)
        {
            UserFriendlyMessage = userFriendlyMessage;
        }

        public PasswordResetException(string message, string userFriendlyMessage, Exception innerException) : base(message, innerException)
        {
            UserFriendlyMessage = userFriendlyMessage;
        }
    }
}
