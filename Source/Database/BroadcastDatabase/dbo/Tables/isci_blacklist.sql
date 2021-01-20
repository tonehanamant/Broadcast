CREATE TABLE [dbo].[isci_blacklist] (
    [id]           INT           IDENTITY (1, 1) NOT NULL,
    [ISCI]         VARCHAR (63)  NOT NULL,
    [created_date] DATETIME      NULL,
    [created_by]   VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_isci_blacklist] PRIMARY KEY CLUSTERED ([id] ASC)
);

