
IF object_id('AnalysisStatistic') is null
Begin
	CREATE TABLE AnalysisStatistic (
		Id              INT           NOT NULL    IDENTITY    PRIMARY KEY,
		[MethodName]        Varchar(100)          NOT NUll,
		SymbolId   INT           NOT NUll,
		FOREIGN KEY (SymbolId) REFERENCES [AllStockSymbols](Id),
        TotalQualified Decimal(18,3) NOT NULL,
		TotalValid Decimal(18,3) NOT NULL,
		Last1YearValid Decimal(18,3) NOT NULL,
		Last1YearQualified Decimal(18,3) NOT NULL,
		Last3YearValid Decimal(18,3) NOT NULL,
		Last3YearQualified Decimal(18,3) NOT NULL,
		Last6YearValid Decimal(18,3) NOT NULL,
		Last6YearQualified Decimal(18,3) NOT NULL,
		Last10YearValid Decimal(18,3) NOT NULL,
		Last10YearQualified Decimal(18,3) NOT NULL,
		CONSTRAINT AnalysisStatistic_Method_Symbol UNIQUE ([MethodName], SymbolId)
	);

	CREATE INDEX Symbol_MethodNameIndex
	ON AnalysisStatistic (SymbolId, MethodName);
End

