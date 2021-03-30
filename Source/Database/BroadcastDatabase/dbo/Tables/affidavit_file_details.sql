﻿CREATE TABLE [dbo].[affidavit_file_details] (
    [id]                    BIGINT        IDENTITY (1, 1) NOT NULL,
    [affidavit_file_id]     INT           NOT NULL,
    [station]               VARCHAR (15)  NOT NULL,
    [original_air_date]     DATETIME      NOT NULL,
    [adjusted_air_date]     DATETIME      NOT NULL,
    [air_time]              INT           NOT NULL,
    [spot_length_id]        INT           NOT NULL,
    [isci]                  VARCHAR (63)  NOT NULL,
    [program_name]          VARCHAR (255) NULL,
    [genre]                 VARCHAR (255) NULL,
    [leadin_genre]          VARCHAR (255) NULL,
    [leadin_program_name]   VARCHAR (255) NULL,
    [leadout_genre]         VARCHAR (255) NULL,
    [leadout_program_name]  VARCHAR (255) NULL,
    [market]                VARCHAR (63)  NULL,
    [estimate_id]           INT           NULL,
    [inventory_source]      INT           NULL,
    [spot_cost]             FLOAT (53)    NULL,
    [affiliate]             VARCHAR (15)  NULL,
    [leadin_end_time]       INT           NULL,
    [leadout_start_time]    INT           NULL,
    [program_show_type]     VARCHAR (255) NULL,
    [leadin_show_type]      VARCHAR (255) NULL,
    [leadout_show_type]     VARCHAR (255) NULL,
    [archived]              BIT           NOT NULL,
    [supplied_program_name] VARCHAR (255) NULL,
    CONSTRAINT [PK_affidavit_file_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_affidavit_files_affidavit_file_details] FOREIGN KEY ([affidavit_file_id]) REFERENCES [dbo].[affidavit_files] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_affidavit_files_affidavit_file_details]
    ON [dbo].[affidavit_file_details]([affidavit_file_id] ASC);
