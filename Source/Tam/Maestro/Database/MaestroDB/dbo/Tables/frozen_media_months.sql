CREATE TABLE [dbo].[frozen_media_months] (
    [media_month_id]   SMALLINT NOT NULL,
    [effective_date]   DATETIME NOT NULL,
    [approved_user_id] INT      NOT NULL,
    [approved_time]    DATETIME NOT NULL,
    CONSTRAINT [PK_frozen_media_months] PRIMARY KEY CLUSTERED ([media_month_id] ASC) WITH (FILLFACTOR = 90)
);



