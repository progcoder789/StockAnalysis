
IF object_id('AnalysisAlerts') is null
Begin
	CREATE TABLE AnalysisAlerts (
		Id              INT           NOT NULL    IDENTITY    PRIMARY KEY,
	    [MethodName]        Varchar(100)          NOT NUll,
		SymbolId   INT           NOT NUll,
		FOREIGN KEY (SymbolId) REFERENCES [AllStockSymbols](Id),
		[Date]  Date Not NULL,
		CONSTRAINT AnalysisAlerts_Method_Symbol_Date UNIQUE ([MethodName], SymbolId,[Date])
	);

	CREATE INDEX symbolIndex
	ON AnalysisAlerts (SymbolId);
End
