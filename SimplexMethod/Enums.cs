namespace SimplexMethod
{
    public enum RelationType
    {
        Equality,
        MoreEqual,
        LessEqual
    }
    
    public enum ResultType
    {
        Done, //решение ЗЛП найдено
        NoVals, //множество допустимых решений ЗЛП пусто
        InfiniteMax //функция не ограничена
    }
}