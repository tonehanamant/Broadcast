CREATE TABLE [dbo].[affidavit_outbound_file_problems] (
    [id]                         INT           IDENTITY (1, 1) NOT NULL,
    [affidavit_outbound_file_id] INT           NULL,
    [problem_description]        VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_affidavit_outbound_file_problems] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_affidavit_outbound_file_problems_affidavit_outbound_files] FOREIGN KEY ([affidavit_outbound_file_id]) REFERENCES [dbo].[affidavit_outbound_files] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_affidavit_outbound_file_problems_affidavit_outbound_files]
    ON [dbo].[affidavit_outbound_file_problems]([affidavit_outbound_file_id] ASC);

