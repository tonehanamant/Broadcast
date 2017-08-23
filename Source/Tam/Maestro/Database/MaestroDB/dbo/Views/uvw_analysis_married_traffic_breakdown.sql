--NOTE
--THIS QUERY BELOW IS TO MODIFY THE VIEW 
CREATE VIEW  uvw_analysis_married_traffic_breakdown as
SELECT    dbo.rate_card_types.name AS rate_card, dbo.traffic.id AS traffic_id, dbo.products.name AS product_name, dbo.materials.code AS isci, 
		  dbo.release_cpmlink.proposal_id, dbo.networks.code AS network, dbo.GetTrafficDetailCoverageUniverse(dbo.traffic_details.id, pa.audience_id, 1) 
		  AS Actual_Universe_Used_for_calc_spot_rate_imps, 
		  CASE WHEN tr_pr.traffic_rating < dbo.proposal_detail_audiences.rating THEN dbo.GetTrafficDetailCoverageUniverse(dbo.traffic_details.id, 
		  pa.audience_id, 1) * tr_pr.traffic_rating / 1000.0 ELSE dbo.GetTrafficDetailCoverageUniverse(dbo.traffic_details.id, pa.audience_id, 1) 
		  * dbo.proposal_detail_audiences.rating / 1000.0 END AS [trafficked primary aud impressions], 
		  dbo.GetProposalCoverageUniverseRatio(dbo.proposal_details.id, dbo.traffic.id) AS Proposal_Universe_Ratio, 
		  dbo.GetTrafficDetailCoverageUniverse(dbo.traffic_details.id, 31, 1) AS UNIVERSE_USED_FOR_SYSTEM_DOLLAR_ALLOCATION_Traf_HH_Universe, 
		  dbo.GetProposalDetailCoverageUniverse(dbo.proposal_details.id, dbo.proposal_detail_audiences.audience_id) AS MP_Primary_Universe, 
		  dbo.GetProposalDetailCoverageUniverse(dbo.proposal_details.id, 31) AS MP_HH_Universe, 
		  dbo.GetProposalDetailCoverageUniverse(dbo.proposal_details.id, 31) * mp_hh.rating / 1000.0 AS [media plan household impressions], 
		  ROUND(mp_hh.rating * 100.0, 6) AS media_plan_household_rating, ROUND(dbo.proposal_detail_audiences.rating * 100.0, 6) 
		  AS media_plan_demo_rating, ROUND(tr_pr.traffic_rating * 100.0, 6) AS traffic_primary_demo_rating, 
		  dbo.proposal_details.proposal_rate * dbo.release_cpmlink.weighting_factor AS traffic_rate, dbo.proposal_details.proposal_rate AS mp_rate, 
		  SUM(dbo.traffic_detail_topographies.spots) AS traffic_spots, dbo.proposal_details.num_spots AS mp_spots, 
		  SUM(dbo.traffic_detail_topographies.spots) * dbo.proposal_details.proposal_rate * dbo.release_cpmlink.weighting_factor AS traffic_total_dollars, 
		  (dbo.proposal_details.proposal_rate * dbo.release_cpmlink.weighting_factor) 
		  / (dbo.proposal_detail_audiences.rating * dbo.proposal_detail_audiences.us_universe * dbo.proposal_details.universal_scaling_factor / 1000.0) 
		  / 2.0 AS traffic_cpm, 
		  dbo.proposal_details.proposal_rate / (dbo.proposal_detail_audiences.rating * dbo.proposal_detail_audiences.us_universe * dbo.proposal_details.universal_scaling_factor
		   / 1000.0) AS mp_cpm
FROM         dbo.release_cpmlink WITH (NOLOCK) INNER JOIN
			  dbo.traffic WITH (NOLOCK) ON dbo.traffic.id = dbo.release_cpmlink.traffic_id INNER JOIN
			  dbo.traffic_details WITH (NOLOCK) ON dbo.traffic_details.traffic_id = dbo.traffic.id INNER JOIN
			  dbo.traffic_audiences AS pa WITH (NOLOCK) ON dbo.traffic.audience_id = pa.audience_id AND pa.traffic_id = dbo.traffic.id INNER JOIN
			  dbo.networks WITH (NOLOCK) ON dbo.networks.id = dbo.traffic_details.network_id INNER JOIN
			  dbo.proposals WITH (NOLOCK) ON dbo.proposals.id = dbo.release_cpmlink.proposal_id INNER JOIN
			  dbo.proposal_details WITH (NOLOCK) ON dbo.proposal_details.proposal_id = dbo.proposals.id AND 
			  dbo.proposal_details.network_id = dbo.traffic_details.network_id INNER JOIN
			  dbo.proposal_detail_audiences AS mp_hh WITH (NOLOCK) ON mp_hh.audience_id = 31 AND 
			  mp_hh.proposal_detail_id = dbo.proposal_details.id INNER JOIN
			  dbo.proposal_detail_audiences WITH (NOLOCK) ON dbo.proposal_detail_audiences.proposal_detail_id = dbo.proposal_details.id INNER JOIN
			  dbo.proposal_audiences WITH (NOLOCK) ON dbo.proposal_audiences.proposal_id = dbo.proposals.id AND 
			  dbo.proposal_audiences.audience_id = dbo.proposal_detail_audiences.audience_id AND 
			  dbo.proposal_audiences.ordinal = dbo.proposals.guarantee_type INNER JOIN
			  dbo.traffic_detail_audiences AS tr_pr WITH (NOLOCK) ON tr_pr.audience_id = dbo.proposal_audiences.audience_id AND 
			  tr_pr.traffic_detail_id = dbo.traffic_details.id INNER JOIN
			  dbo.traffic_detail_weeks tdw (NOLOCK) on tdw.traffic_detail_id = dbo.traffic_details.id INNER JOIN
			  dbo.traffic_detail_topographies WITH (NOLOCK) ON dbo.traffic_detail_topographies.traffic_detail_week_id = tdw.id INNER JOIN
			  dbo.topographies WITH (NOLOCK) ON dbo.topographies.id = dbo.traffic_detail_topographies.topography_id INNER JOIN
			  dbo.rate_card_types WITH (NOLOCK) ON dbo.rate_card_types.id = dbo.traffic.rate_card_type_id LEFT OUTER JOIN
			  dbo.traffic_materials WITH (NOLOCK) ON dbo.traffic_materials.traffic_id = dbo.traffic.id LEFT OUTER JOIN
			  dbo.products ON dbo.proposals.product_id = dbo.products.id LEFT OUTER JOIN
			  dbo.materials ON dbo.materials.id = dbo.traffic_materials.material_id
WHERE     (dbo.topographies.topography_type = 0)
GROUP BY dbo.rate_card_types.name, dbo.traffic.id, dbo.products.name, dbo.materials.code, dbo.release_cpmlink.proposal_id, dbo.networks.code, 
		  dbo.proposal_detail_audiences.rating * dbo.proposal_detail_audiences.us_universe * dbo.proposal_details.universal_scaling_factor / 1000.0, 
		  dbo.proposal_details.proposal_rate * dbo.release_cpmlink.weighting_factor, dbo.proposal_details.proposal_rate, dbo.proposal_details.num_spots, 
		  dbo.release_cpmlink.weighting_factor, dbo.proposal_details.id, dbo.proposal_detail_audiences.audience_id, dbo.traffic_details.id, pa.audience_id, 
		  tr_pr.traffic_rating, mp_hh.rating, dbo.proposal_audiences.audience_id, dbo.proposal_detail_audiences.rating

GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane2', @value = N'DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "mp_hh"
            Begin Extent = 
               Top = 6
               Left = 887
               Bottom = 121
               Right = 1073
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "proposal_detail_audiences"
            Begin Extent = 
               Top = 486
               Left = 38
               Bottom = 601
               Right = 224
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "proposal_audiences"
            Begin Extent = 
               Top = 486
               Left = 262
               Bottom = 601
               Right = 430
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tr_pr"
            Begin Extent = 
               Top = 6
               Left = 677
               Bottom = 121
               Right = 849
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "traffic_detail_topographies"
            Begin Extent = 
               Top = 606
               Left = 38
               Bottom = 721
               Right = 212
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "topographies"
            Begin Extent = 
               Top = 606
               Left = 250
               Bottom = 721
               Right = 431
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "rate_card_types"
            Begin Extent = 
               Top = 726
               Left = 38
               Bottom = 826
               Right = 206
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "traffic_materials"
            Begin Extent = 
               Top = 726
               Left = 244
               Bottom = 841
               Right = 423
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "products"
            Begin Extent = 
               Top = 846
               Left = 38
               Bottom = 961
               Right = 264
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "materials"
            Begin Extent = 
               Top = 966
               Left = 38
               Bottom = 1081
               Right = 233
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
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'uvw_analysis_married_traffic_breakdown';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 2, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'uvw_analysis_married_traffic_breakdown';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[14] 4[9] 2[58] 3) )"
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
         Begin Table = "release_cpmlink"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 121
               Right = 216
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "traffic"
            Begin Extent = 
               Top = 126
               Left = 38
               Bottom = 241
               Right = 289
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "traffic_details"
            Begin Extent = 
               Top = 6
               Left = 254
               Bottom = 121
               Right = 433
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "pa"
            Begin Extent = 
               Top = 6
               Left = 471
               Bottom = 121
               Right = 639
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "networks"
            Begin Extent = 
               Top = 246
               Left = 38
               Bottom = 361
               Right = 206
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "proposals"
            Begin Extent = 
               Top = 366
               Left = 38
               Bottom = 481
               Right = 393
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "proposal_details"
            Begin Extent = 
               Top = 246
               Left = 244
               Bottom = 361
               Right = 457
            End
            ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'uvw_analysis_married_traffic_breakdown';

