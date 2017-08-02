INSERT INTO [dbo].[AStocks]
           ([SymbolId]
           ,[Date]
           ,[Open]
           ,[Close]
           ,[High]
           ,[Low]
		   ,[Volume]
           ,[AdjustClose])
     VALUES
           (@SymbolId,
		    @Date,
			@Open,
			@Close,
			@High,
			@Low,
			@TradeVolume,
			@AdjustClose)