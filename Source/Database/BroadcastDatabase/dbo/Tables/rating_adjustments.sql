CREATE TABLE [dbo].[rating_adjustments] (
    [media_month_id]    INT            NOT NULL,
    [annual_adjustment] DECIMAL (5, 2) NOT NULL,
    [nti_adjustment]    DECIMAL (5, 2) NOT NULL,
    CONSTRAINT [PK_rating_adjustments] PRIMARY KEY CLUSTERED ([media_month_id] ASC),
    CONSTRAINT [FK_rating_adjustments_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id])
);

