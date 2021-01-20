CREATE TABLE [dbo].[affidavit_file_problems] (
    [id]                  BIGINT         IDENTITY (1, 1) NOT NULL,
    [affidavit_file_id]   INT            NOT NULL,
    [problem_description] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_affidavit_file_problems] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_affidavit_file_problems_affidiavit_file_id_affidavit_id] FOREIGN KEY ([affidavit_file_id]) REFERENCES [dbo].[affidavit_files] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_affidavit_file_problems_affidiavit_file_id_affidavit_id]
    ON [dbo].[affidavit_file_problems]([affidavit_file_id] ASC);

