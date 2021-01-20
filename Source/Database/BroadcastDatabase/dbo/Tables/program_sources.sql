CREATE TABLE [dbo].[program_sources] (
    [id]   INT          IDENTITY (1, 1) NOT NULL,
    [name] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_program_sources] PRIMARY KEY CLUSTERED ([id] ASC)
);

