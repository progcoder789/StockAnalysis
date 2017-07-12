IF NOT EXISTS (SELECT * FROM [dbo].[AnalysisResults] where [MethodName] = @MethodName and SymbolId = @SymbolId and [Date] = @Date)   
Begin
    INSERT INTO [dbo].[AnalysisResults]
           ([MethodName]
           ,[SymbolId]
           ,[Date]
           ,[Qualified]
		   ,[Valid])
     VALUES
           (@MethodName
           ,@SymbolId
           ,@Date
           ,@Qualified
		   ,@Valid);
End