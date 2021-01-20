CREATE TABLE [dbo].[nti_transmittals_file_reports] (
    [id]                       INT           IDENTITY (1, 1) NOT NULL,
    [nti_transmittals_file_id] INT           NOT NULL,
    [date]                     DATETIME      NOT NULL,
    [advertiser]               VARCHAR (63)  NOT NULL,
    [report_name]              VARCHAR (255) NOT NULL,
    [program_id]               INT           NOT NULL,
    [stream]                   VARCHAR (16)  NOT NULL,
    [program_type]             VARCHAR (4)   NOT NULL,
    [program_duration]         INT           NOT NULL,
    [stations]                 INT           NOT NULL,
    [CVG]                      INT           NOT NULL,
    [TbyC]                     INT           NOT NULL,
    [TA]                       FLOAT (53)    NOT NULL,
    [week_ending]              DATETIME      NOT NULL,
    CONSTRAINT [PK_nti_transmittals_file_reports] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_nti_transmittals_file_reports_nti_transmittals_files] FOREIGN KEY ([nti_transmittals_file_id]) REFERENCES [dbo].[nti_transmittals_files] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_transmittals_file_reports_nti_transmittals_files]
    ON [dbo].[nti_transmittals_file_reports]([nti_transmittals_file_id] ASC);

