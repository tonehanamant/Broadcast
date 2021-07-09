CREATE TABLE [dbo].[reel_isci_products]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
	[isci] VARCHAR(50) NOT NULL,
	[product_name] VARCHAR(50) NOT NULL,
	[created_at] DATETIME2 NOT NULL,
	[created_by] VARCHAR(100) NOT NULL
);
