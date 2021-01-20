CREATE TABLE [dbo].[isci_mapping] (
    [id]             INT           IDENTITY (1, 1) NOT NULL,
    [original_isci]  VARCHAR (63)  NOT NULL,
    [effective_isci] VARCHAR (63)  NOT NULL,
    [created_date]   DATETIME      NOT NULL,
    [created_by]     VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_isci_mapping] PRIMARY KEY CLUSTERED ([id] ASC)
);

