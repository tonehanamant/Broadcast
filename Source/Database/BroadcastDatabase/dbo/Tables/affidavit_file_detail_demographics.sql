CREATE TABLE [dbo].[affidavit_file_detail_demographics] (
    [id]                       INT        IDENTITY (1, 1) NOT NULL,
    [audience_id]              INT        NULL,
    [affidavit_file_detail_id] BIGINT     NULL,
    [overnight_rating]         FLOAT (53) NULL,
    [overnight_impressions]    FLOAT (53) NULL,
    CONSTRAINT [PK_affidavit_file_detail_demographics] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_affidavit_file_detail_demographics_affidavit_file_details] FOREIGN KEY ([affidavit_file_detail_id]) REFERENCES [dbo].[affidavit_file_details] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_affidavit_file_detail_demographics_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_affidavit_file_detail_demographics_affidavit_file_details]
    ON [dbo].[affidavit_file_detail_demographics]([affidavit_file_detail_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_affidavit_file_detail_demographics_audiences]
    ON [dbo].[affidavit_file_detail_demographics]([audience_id] ASC);

