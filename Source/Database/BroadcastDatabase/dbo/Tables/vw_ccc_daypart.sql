CREATE TABLE [dbo].[vw_ccc_daypart] (
    [id]           INT          NOT NULL,
    [code]         VARCHAR (15) NOT NULL,
    [name]         VARCHAR (63) NOT NULL,
    [tier]         INT          NOT NULL,
    [start_time]   INT          NOT NULL,
    [end_time]     INT          NOT NULL,
    [mon]          INT          NULL,
    [tue]          INT          NULL,
    [wed]          INT          NULL,
    [thu]          INT          NULL,
    [fri]          INT          NULL,
    [sat]          INT          NULL,
    [sun]          INT          NULL,
    [daypart_text] VARCHAR (63) NOT NULL,
    [total_hours]  FLOAT (53)   NOT NULL,
    CONSTRAINT [PK_vw_ccc_daypart] PRIMARY KEY CLUSTERED ([id] ASC, [code] ASC, [name] ASC, [tier] ASC, [start_time] ASC, [end_time] ASC, [daypart_text] ASC, [total_hours] ASC)
);

