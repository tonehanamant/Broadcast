﻿CREATE TABLE [dbo].[advertiser_industries] (
    [id]   INT            IDENTITY (1, 1) NOT NULL,
    [name] NVARCHAR (256) NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90)
);

