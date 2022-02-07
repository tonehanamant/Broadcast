CREATE TABLE [dbo].[plans] (
    [id]                         INT              IDENTITY (1, 1) NOT NULL,
    [campaign_id]                INT              NOT NULL,
    [name]                       NVARCHAR (265)   NOT NULL,
    [product_id]                 INT              NULL,
    [latest_version_id]          INT              NOT NULL,
    [product_master_id]          UNIQUEIDENTIFIER NULL,
    [spot_allocation_model_mode] INT              NULL,
    [plan_mode]                  INT              NOT NULL,
    [deleted_by]                 VARCHAR (100)    NULL,
    [deleted_at]                 DATETIME2 (7)    NULL,
    CONSTRAINT [PK_plans] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_plans_campaigns] FOREIGN KEY ([campaign_id]) REFERENCES [dbo].[campaigns] ([id])
);




GO
CREATE NONCLUSTERED INDEX [IX_plans_campaign_id]
    ON [dbo].[plans]([campaign_id] ASC)
    INCLUDE([id]);

