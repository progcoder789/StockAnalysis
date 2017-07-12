
IF object_id('AStocks') is null
Begin
	CREATE TABLE AStocks (
		Id              INT           NOT NULL    IDENTITY    PRIMARY KEY,
		SymbolId        INT           NOT NUll,
		FOREIGN KEY (SymbolId) REFERENCES [AllStockSymbols](Id),
		[Date]  Date,
		[Open]  Decimal(18,2),
		[Close] Decimal(18,2),
		[High]  Decimal(18,2),
		[Low]   Decimal(18,2),
		[UnAdjustClose] Decimal(18,2),
		[TradeVolume] INT,
		[SMA5]  Decimal(18,2),
		[SMA10] Decimal(18,2),
		[SMA30] Decimal(18,2),
		[SMA60] Decimal(18,2),
		[Trend] Varchar(1),
		CONSTRAINT AStocks_Symbol_Date UNIQUE (SymbolId,[Date])
	);

	CREATE INDEX sybmolIndex
	ON AStocks (SymbolId);
End
