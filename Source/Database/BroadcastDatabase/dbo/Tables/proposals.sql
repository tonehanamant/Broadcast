CREATE TABLE [dbo].[proposals] (
    [id]                 INT           IDENTITY (1, 1) NOT NULL,
    [name]               VARCHAR (127) NOT NULL,
    [advertiser_id]      INT           NOT NULL,
    [created_by]         VARCHAR (63)  NOT NULL,
    [created_date]       DATETIME      NOT NULL,
    [modified_by]        VARCHAR (63)  NOT NULL,
    [modified_date]      DATETIME      NOT NULL,
    [primary_version_id] INT           NOT NULL,
    [campaign_id]        INT           NULL,
    CONSTRAINT [PK_proposals] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposals_campaigns] FOREIGN KEY ([campaign_id]) REFERENCES [dbo].[campaigns] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposals_campaigns]
    ON [dbo].[proposals]([campaign_id] ASC);

