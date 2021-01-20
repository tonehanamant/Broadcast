﻿CREATE TABLE [dbo].[spot_tracker_files] (
    [id]           INT           IDENTITY (1, 1) NOT NULL,
    [file_name]    VARCHAR (127) NOT NULL,
    [start_date]   DATETIME      NOT NULL,
    [end_date]     DATETIME      NOT NULL,
    [file_hash]    VARCHAR (63)  NOT NULL,
    [created_by]   VARCHAR (63)  NOT NULL,
    [created_date] DATETIME      NOT NULL,
    CONSTRAINT [PK_spot_tracker_files] PRIMARY KEY CLUSTERED ([id] ASC)
);

