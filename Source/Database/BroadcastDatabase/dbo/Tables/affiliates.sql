CREATE TABLE [dbo].[affiliates] (
    [id]            INT           IDENTITY (1, 1) NOT NULL,
    [name]          VARCHAR (127) NOT NULL,
    [created_by]    VARCHAR (63)  NOT NULL,
    [created_date]  DATETIME      NOT NULL,
    [modified_by]   VARCHAR (63)  NOT NULL,
    [modified_date] DATETIME      NOT NULL,
    CONSTRAINT [PK_affiliates] PRIMARY KEY CLUSTERED ([id] ASC)
);

