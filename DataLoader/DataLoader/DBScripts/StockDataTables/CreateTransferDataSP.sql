IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'TransferData')
BEGIN
 DROP PROCEDURE [dbo].[TransferData]
END

/****** Object:  StoredProcedure [dbo].[TransferData]    Script Date: 7/7/2017 10:40:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[TransferData]     
AS   
DECLARE @Prefix varchar(1);  
DECLARE @SQLQueryString nvarchar(500);  
DECLARE @cnt INT = 0;

WHILE @cnt < 26
BEGIN
    SET @SQLQueryString =  
     N'Insert into ' +  @Prefix + 'Stocks ' +
      'select [SymbolId]
      ,[Date]
      ,[Open]
      ,[Close]
      ,[High]
      ,[Low]
      ,[AdjustClose]
	  ,[TradeVolume]
      ,[SMA5]
      ,[SMA10]
      ,[SMA30]
      ,[SMA60]
      ,[Trend] from [dbo].[AllStocks] a join [dbo].[AllStockSymbols] b on b.Id = a.SymbolId where b.Symbol Like ''' + @Prefix + '%'' and 
	  NOT EXISTS (SELECT * FROM '+ @Prefix + 'Stocks ' +' c WHERE a.SymbolId = c.SymbolId and a.[Date] = c.[Date])';  

	SET @Prefix = char(65+@cnt);  
	EXECUTE sp_executesql @SQLQueryString
    SET @cnt = @cnt + 1;
END;
GO

/****** Object:  StoredProcedure [dbo].[TransferSymbols]    Script Date: 7/8/2017 3:17:59 AM ******/
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'TransferSymbols')
BEGIN
 DROP PROCEDURE [dbo].[TransferSymbols]
END


/****** Object:  StoredProcedure [dbo].[TransferSymbols]    Script Date: 7/8/2017 3:17:59 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[TransferSymbols]     
AS   
	DECLARE @SQLQueryString nvarchar(500);  
	SET @SQLQueryString =  
		 N'Insert into [AllStockSymbols]  ' +
		  'select [Symbol]
			   ,[Name]
			   ,[Volume] from [dbo].[StagingStockSymbols] b where NOT EXISTS (SELECT * FROM [AllStockSymbols] a WHERE a.Symbol = b.Symbol)';  
	EXECUTE sp_executesql @SQLQueryString

GO
