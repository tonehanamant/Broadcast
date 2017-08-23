CREATE TABLE [dbo].[genres] (
    [id]   INT          IDENTITY (1, 1) NOT NULL,
    [name] VARCHAR (63) NOT NULL,
    CONSTRAINT [PK_genres] PRIMARY KEY CLUSTERED ([id] ASC)
);

