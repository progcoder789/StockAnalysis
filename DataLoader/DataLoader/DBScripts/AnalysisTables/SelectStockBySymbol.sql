select * from AStocks
where SymbolId = @SymbolId and [Date] > @Date
order by [Date]