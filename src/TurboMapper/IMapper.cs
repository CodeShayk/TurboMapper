namespace TurboMapper
{
    public interface IMapper
    {
        TTarget Map<TSource, TTarget>(TSource source);
    }
}