CREATE TABLE [dbo].[comments] (
    [id]              INT           IDENTITY (1, 1) NOT NULL,
    [comment_type_id] INT           NOT NULL,
    [rtf_text]        VARCHAR (MAX) NOT NULL,
    [plain_text]      VARCHAR (MAX) NULL,
    [app_name]        VARCHAR (100) NOT NULL,
    [form_name]       VARCHAR (100) NOT NULL,
    [entity_name]     VARCHAR (100) NULL,
    [reference_id]    INT           NULL,
    [modified_date]   DATETIME      NOT NULL,
    [employee_id]     INT           NOT NULL,
    CONSTRAINT [PK_comments] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_comments_comment_types] FOREIGN KEY ([comment_type_id]) REFERENCES [dbo].[comment_types] ([id]),
    CONSTRAINT [FK_comments_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_comments_form_name]
    ON [dbo].[comments]([app_name] ASC) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'rtf_text';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'plain_text';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'plain_text';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'app_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'app_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'form_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'form_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'entity_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'entity_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'reference_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'reference_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'modified_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'modified_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'comment_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'comment_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'comments', @level2type = N'COLUMN', @level2name = N'rtf_text';

