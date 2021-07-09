CREATE TABLE [dbo].[reel_isci_advertiser_name_references]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
	[reel_isci_id] INT NOT NULL,
	[advertiser_name_reference] NVARCHAR(100) NOT NULL,
	CONSTRAINT [FK_reel_isci_advertiser_name_references_reel_iscis] FOREIGN KEY ([reel_isci_id]) REFERENCES [dbo].[reel_iscis]([ID]) ON DELETE CASCADE
);
