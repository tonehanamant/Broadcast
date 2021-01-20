CREATE TABLE [dbo].[nti_transmittals_file_report_ratings] (
    [id]                              INT           IDENTITY (1, 1) NOT NULL,
    [nti_transmittals_file_report_id] INT           NOT NULL,
    [category_name]                   VARCHAR (255) NOT NULL,
    [subcategory_name]                VARCHAR (255) NULL,
    [percent]                         FLOAT (53)    NOT NULL,
    [impressions]                     FLOAT (53)    NOT NULL,
    [VPVH]                            INT           NULL,
    CONSTRAINT [PK_nti_transmittals_file_report_ratings] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_nti_transmittals_file_report_ratings_nti_transmittals_file_reports] FOREIGN KEY ([nti_transmittals_file_report_id]) REFERENCES [dbo].[nti_transmittals_file_reports] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_transmittals_file_report_ratings_nti_transmittals_file_reports]
    ON [dbo].[nti_transmittals_file_report_ratings]([nti_transmittals_file_report_id] ASC);

