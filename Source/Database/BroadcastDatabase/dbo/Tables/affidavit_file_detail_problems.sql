CREATE TABLE [dbo].[affidavit_file_detail_problems] (
    [id]                       INT           IDENTITY (1, 1) NOT NULL,
    [affidavit_file_detail_id] BIGINT        NOT NULL,
    [problem_type]             INT           NOT NULL,
    [problem_description]      VARCHAR (255) NULL,
    CONSTRAINT [PK_affidavit_file_detail_problems] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_affidavit_file_detail_problems_affidavit_file_details] FOREIGN KEY ([affidavit_file_detail_id]) REFERENCES [dbo].[affidavit_file_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_affidavit_file_detail_problems_affidavit_file_details]
    ON [dbo].[affidavit_file_detail_problems]([affidavit_file_detail_id] ASC);

