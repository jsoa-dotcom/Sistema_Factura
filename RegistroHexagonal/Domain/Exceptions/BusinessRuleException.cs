namespace RegistroHexagonal.Domain.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string mensaje) : base(mensaje) { }
    }
}
