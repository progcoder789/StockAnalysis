INSERT INTO [dbo].[AStocks]
           ([SymbolId]
           ,[Date]
           ,[Open]
           ,[Close]
           ,[High]
           ,[Low]
		   ,[Volume]
           ,[UnAdjustClose])
     VALUES
           (@SymbolId,
		    @Date,
			@Open,
			@Close,
			@High,
			@Low,
			@TradeVolume,
			@UnAdjustClose)