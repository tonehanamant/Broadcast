CREATE TABLE [dbo].[program_name_exceptions] (
    [id]                  INT            IDENTITY (1, 1) NOT NULL,
    [custom_program_name] NVARCHAR (500) NOT NULL,
    [genre_id]            INT            NOT NULL,
    [show_type_id]        INT            NOT NULL,
    [created_by]          VARCHAR (63)   NOT NULL,
    [created_at]          DATETIME       NOT NULL,
    CONSTRAINT [PK_program_name_exceptions] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_program_name_exceptions_genres] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_program_name_exceptions_show_types] FOREIGN KEY ([show_type_id]) REFERENCES [dbo].[show_types] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_program_name_exceptions_show_types]
    ON [dbo].[program_name_exceptions]([show_type_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_program_name_exceptions_genres]
    ON [dbo].[program_name_exceptions]([genre_id] ASC);

