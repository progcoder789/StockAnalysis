IF object_id('AnalysisResults') is null
Begin
	CREATE TABLE AnalysisResults (
		Id              INT           NOT NULL    IDENTITY    PRIMARY KEY,
		[MethodName]        Varchar(100)          NOT NUll,
		SymbolId   INT           NOT NUll,
		FOREIGN KEY (SymbolId) REFERENCES [AllStockSymbols](Id),
		[Date]  Date Not NULL,
		[Qualified] Bit Not NULL,
		[Valid] Bit Not NULL,
		CONSTRAINT AnalysisResults_Method_Symbol_Date UNIQUE ([MethodName], SymbolId,[Date])
	);

	CREATE INDEX Symbol_MethodNameIndex
	ON AnalysisResults (SymbolId, MethodName);
End

IF object_id('StagingAnalysisResults') is null
Begin
	CREATE TABLE StagingAnalysisResults (
		Id              INT           NOT NULL    IDENTITY    PRIMARY KEY,
		[MethodName]        Varchar(100)          NOT NUll,
		SymbolId   INT           NOT NUll,
		FOREIGN KEY (SymbolId) REFERENCES [AllStockSymbols](Id),
		[Date]  Date Not NULL,
		[Qualified] Bit Not NULL,
		[Valid] Bit Not NULL,
		CONSTRAINT StagingAnalysisResults_Method_Symbol_Date UNIQUE ([MethodName], SymbolId,[Date])
	);

	CREATE INDEX symbolIndex
	ON StagingAnalysisResults (SymbolId);
End
