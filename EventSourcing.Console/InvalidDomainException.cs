    using System.Runtime.Serialization;

namespace EventSourcing.Console
{
    [Serializable]
    internal class InvalidDomainException : Exception
    {
        public InvalidDomainException()
        {
        }

        public InvalidDomainException(string? message) : base(message)
        {
        }

        public InvalidDomainException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidDomainException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}