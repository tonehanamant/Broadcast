CREATE TABLE [dbo].[plans] (
    [id]                INT IDENTITY (1, 1)     NOT NULL,
    [campaign_id]       INT                     NOT NULL,
    [name]              NVARCHAR (265)          NOT NULL,
    [product_id]        INT                     NULL,
    [product_master_id] UNIQUEIDENTIFIER        NULL,
    [latest_version_id] INT                     NOT NULL,
    CONSTRAINT [PK_plans] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plans_campaigns] FOREIGN KEY ([campaign_id]) REFERENCES [dbo].[campaigns] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plans_campaigns]
    ON [dbo].[plans]([campaign_id] ASC);

