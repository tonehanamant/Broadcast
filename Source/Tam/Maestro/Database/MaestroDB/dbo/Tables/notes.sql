CREATE TABLE [dbo].[notes] (
    [id]                 INT          IDENTITY (1, 1) NOT NULL,
    [note_type]          VARCHAR (63) NOT NULL,
    [reference_id]       INT          NOT NULL,
    [employee_id]        INT          NULL,
    [comment]            TEXT         NOT NULL,
    [date_created]       DATETIME     NOT NULL,
    [date_last_modified] DATETIME     NOT NULL,
    CONSTRAINT [PK_notes] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_notes_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains notes for different types of entities.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Notes', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Entities are defined by the "note_type" field.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Defines a group of notes.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'note_type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'These are predefined (ex: system, zone, etc...)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'note_type';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The id into the source table.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'reference_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'This points to a record in another table described by "note_type".', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'reference_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Note', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'comment';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'comment';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Timestamp the note was entered.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'notes', @level2type = N'COLUMN', @level2name = N'date_last_modified';

