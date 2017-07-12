IF NOT EXISTS (SELECT * FROM [dbo].[AllStockSymbols] where [Symbol] = @Symbol)   
Begin
	INSERT INTO [dbo].[AllStockSymbols]
			   ([Symbol]
			   ,[Name]
			   ,[Volume])
		 VALUES
			   (@Symbol,
				@Name,
				@Volume)
End
