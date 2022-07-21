namespace UKMCAB.Common.Exceptions;

[Serializable]
public class PermissionDeniedException : DomainException
{
    public PermissionDeniedException() { }
    public PermissionDeniedException(string message) : base(message) { }
    public PermissionDeniedException(string message, Exception inner) : base(message, inner) { }
    protected PermissionDeniedException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
