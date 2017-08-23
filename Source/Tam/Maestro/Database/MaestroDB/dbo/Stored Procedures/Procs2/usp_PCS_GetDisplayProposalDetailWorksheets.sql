/****** Object:  Table [dbo].[usp_PCS_GetDisplayProposalDetailWorksheets]    Script Date: 11/16/2012 14:51:25 ******/
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayProposalDetailWorksheets]
(
	@proposal_id int
)
AS
	SELECT 
		n.id, 
		n.code, 
		mw.start_date, 
		pdw.units,
		mw.end_date,
		mw.id,
		pd.id
	FROM 
		proposal_detail_worksheets pdw WITH (NOLOCK)
		JOIN proposal_details pd WITH (NOLOCK) on pd.id = pdw.proposal_detail_id
		JOIN networks n WITH (NOLOCK) on n.id = pd.network_id 
		JOIN media_weeks mw WITH (NOLOCK) on mw.id = pdw.media_week_id
	WHERE 
		pd.proposal_id = @proposal_id
	ORDER BY
		n.id, 
		pd.id, 
		mw.start_date
