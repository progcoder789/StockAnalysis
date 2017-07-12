/****** Object:  StoredProcedure [dbo].[TransferSymbols]    Script Date: 7/8/2017 3:17:59 AM ******/
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'TransferAnalysisResults')
BEGIN
 DROP PROCEDURE [dbo].[TransferAnalysisResults]
END


/****** Object:  StoredProcedure [dbo].[TransferSymbols]    Script Date: 7/8/2017 3:17:59 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[TransferAnalysisResults]     
AS   
	DECLARE @SQLQueryString nvarchar(500);  
	SET @SQLQueryString =  
		 N'Insert into [AnalysisResults]  ' +
		  'select [MethodName]
           ,[SymbolId]
           ,[Date]
           ,[Qualified]
           ,[Valid] from [dbo].[StagingAnalysisResults] b where NOT EXISTS (SELECT * FROM [AnalysisResults] a WHERE a.[MethodName] = b.[MethodName] and a.[SymbolId] = b.[SymbolId] and a.[Date] = b.[Date])';  
	EXECUTE sp_executesql @SQLQueryString

GO