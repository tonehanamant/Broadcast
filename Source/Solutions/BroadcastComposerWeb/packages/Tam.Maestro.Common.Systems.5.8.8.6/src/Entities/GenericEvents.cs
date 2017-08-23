namespace Tam.Maestro.Data.Entities
{
    public delegate void GenericEvent();
    public delegate void GenericEvent<T>(T t);
    public delegate void GenericEvent<T, U>(T t, U u);
    public delegate void GenericEvent<T, U, V>(T t, U u, V v);
    public delegate void GenericEvent<T, U, V, W>(T t, U u, V v, W w);
    public delegate void GenericEvent<T, U, V, W, X>(T t, U u, V v, W w, X x);
    public delegate void GenericEvent<T, U, V, W, X, Y>(T t, U u, V v, W w, X x, Y y);
}
