CREATE TABLE [dbo].[tam_post_reports] (
    [id]           INT      IDENTITY (1, 1) NOT NULL,
    [tam_post_id]  INT      NOT NULL,
    [document_id]  INT      NOT NULL,
    [employee_id]  INT      NOT NULL,
    [date_created] DATETIME NOT NULL,
    CONSTRAINT [PK_tam_post_reports] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_tam_post_reports_documents] FOREIGN KEY ([document_id]) REFERENCES [dbo].[documents] ([id]),
    CONSTRAINT [FK_tam_post_reports_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_tam_post_reports_tam_posts] FOREIGN KEY ([tam_post_id]) REFERENCES [dbo].[tam_posts] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains all generated PDF post reports.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'TAM Post Reports', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Used for auditing purposes.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the TAM Post that was printed.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the document (a pdf in this case).', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports', @level2type = N'COLUMN', @level2name = N'document_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports', @level2type = N'COLUMN', @level2name = N'document_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Employee who created the report.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'When the report was created.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_reports', @level2type = N'COLUMN', @level2name = N'date_created';

