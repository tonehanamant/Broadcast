CREATE VIEW [dbo].[vw_ccc_daypart]
AS
	SELECT
		dp.id,
		dp.code,
		dp.name,
		dp.tier,
		ts.start_time,
		ts.end_time,
		CASE (sum(power(2,d.ordinal - 1)) & 0x01) 
			WHEN 0x01 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END mon,
		CASE (sum(power(2,d.ordinal - 1)) & 0x02) 
			WHEN 0x02 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END  tue,
		CASE (sum(power(2,d.ordinal - 1)) & 0x04) 
			WHEN 0x04 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END  wed,
		CASE (sum(power(2,d.ordinal - 1)) & 0x08) 
			WHEN 0x08 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END  thu,
		CASE (sum(power(2,d.ordinal - 1)) & 0x10) 
			WHEN 0x10 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END  fri,
		CASE (sum(power(2,d.ordinal - 1)) & 0x20) 
			WHEN 0x20 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END  sat,
		CASE (sum(power(2,d.ordinal - 1)) & 0x40) 
			WHEN 0x40 THEN 1
			WHEN 0x00 THEN 0
			ELSE NULL
		END sun,
		dp.daypart_text,
		dp.total_hours
	FROM
		dayparts dp (NOLOCK)
		join timespans ts (NOLOCK) ON 
			ts.id = dp.timespan_id
		join daypart_days dp_d (NOLOCK) ON
			dp.id = dp_d.daypart_id
		join days d (NOLOCK) ON 
			d.id = dp_d.day_id
	GROUP BY
		dp.id,
		dp.code,
		dp.name,
		dp.tier,
		ts.start_time,
		ts.end_time,
		dp.daypart_text,
		dp.total_hours
GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "b"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 91
               Right = 190
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a"
            Begin Extent = 
               Top = 6
               Left = 228
               Bottom = 121
               Right = 380
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'vw_ccc_daypart';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 1, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'vw_ccc_daypart';

