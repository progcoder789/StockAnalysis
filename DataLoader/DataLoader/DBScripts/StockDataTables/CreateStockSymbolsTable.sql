
IF object_id('1StockSymbols') is null
Begin
	CREATE TABLE [AllStockSymbols] (
		Id              INT           NOT NULL    IDENTITY    PRIMARY KEY,
		Symbol          VARCHAR(100)  NOT NULL,
	    [Name]          VARCHAR(300),
		[Volume]        Decimal
	);

	CREATE INDEX sybmolIndex
	ON [AllStockSymbols] (Symbol);
End

IF object_id('StagingStockSymbols') is null
Begin
	CREATE TABLE [StagingStockSymbols] (
		Id              INT           NOT NULL    IDENTITY    PRIMARY KEY,
		Symbol          VARCHAR(100)  NOT NULL,
	    [Name]          VARCHAR(300),
		[Volume]        Decimal
	);

	CREATE INDEX StageSybmolIndex
	ON [StagingStockSymbols] (Symbol);
End