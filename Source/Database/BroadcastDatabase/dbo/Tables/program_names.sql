CREATE TABLE [dbo].[program_names] (
    [id]           INT            IDENTITY (1, 1) NOT NULL,
    [program_name] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_program_names] PRIMARY KEY CLUSTERED ([id] ASC)
);

