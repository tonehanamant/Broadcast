using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.spotcableXML;

namespace Services.Broadcast.Converters.Scx
{
    /// <summary>
    /// Feature not available
    /// </summary>
    public interface IProposalScxConverter : IApplicationService
    {
        //List<ProposalScxFile> ConvertProposal(ProposalDto proposal);
        //void ConvertProposalDetail(ProposalDto proposal, ProposalDetailDto propDetail, ref ProposalScxFile scxFile);
        //adx BuildFromProposalDetail(ProposalDto proposal, ProposalDetailDto propDetail);
    }
    
    public class ProposalScxConverter : ScxBaseConverter, IProposalScxConverter
    {        
        private readonly IProposalScxDataPrep _proposalScxDataPrep;

        public ProposalScxConverter(IProposalScxDataPrep proposalScxDataPrep
                                    , IDaypartCache daypartCache) : base(daypartCache)
        {
            _proposalScxDataPrep = proposalScxDataPrep;
        }

        //public List<ProposalScxFile> ConvertProposal(ProposalDto proposal)
        //{
        //    List<ProposalScxFile> scxFiles = new List<ProposalScxFile>();
        //    foreach (var propDetail in proposal.Details)
        //    {
        //        ProposalScxFile scxFile = new ProposalScxFile();
        //        ConvertProposalDetail(proposal, propDetail, ref scxFile);
        //        scxFiles.Add(scxFile);
        //    }

        //    return scxFiles;
        //}

        //public void ConvertProposalDetail(ProposalDto proposal, ProposalDetailDto propDetail, ref ProposalScxFile scxFile)
        //{
        //    adx a = BuildFromProposalDetail(proposal, propDetail);

        //    if (a == null)
        //        return;

        //    string xml = a.Serialize();
        //    var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        //    scxFile = new ProposalScxFile()
        //    {
        //        ProposalDetailDto = propDetail,
        //        ScxStream = stream
        //    };
        //}

        //public adx BuildFromProposalDetail(ProposalDto proposal, ProposalDetailDto propDetail)
        //{
        //    var data = _proposalScxDataPrep.GetDataFromProposalDetail(proposal, propDetail);
        //    if (data.MarketIds.IsNullOrEmpty())
        //        return null;

        //    return CreateAdxObject(data);
        //}
    }
}

