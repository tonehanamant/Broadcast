CREATE TABLE [dbo].[frozen_topography_businesses] (
    [media_month_id] SMALLINT NOT NULL,
    [topography_id]  INT      NOT NULL,
    [business_id]    INT      NOT NULL,
    [include]        BIT      NOT NULL,
    CONSTRAINT [PK_frozen_topography_businesses] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [topography_id] ASC, [business_id] ASC)
);

