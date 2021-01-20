CREATE TABLE [dbo].[campaigns] (
    [id]            INT            IDENTITY (1, 1) NOT NULL,
    [name]          VARCHAR (255)  NOT NULL,
    [advertiser_id] INT            NOT NULL,
    [agency_id]     INT            NOT NULL,
    [created_date]  DATETIME       NOT NULL,
    [created_by]    VARCHAR (63)   NOT NULL,
    [modified_date] DATETIME       NOT NULL,
    [modified_by]   VARCHAR (63)   NOT NULL,
    [notes]         VARCHAR (1024) NULL,
    CONSTRAINT [PK_campaigns] PRIMARY KEY CLUSTERED ([id] ASC)
);

