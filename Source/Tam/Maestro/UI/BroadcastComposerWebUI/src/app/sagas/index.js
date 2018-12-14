import * as appSaga from "./app";
import * as postSaga from "./post";
import * as postPrePostingSaga from "./postPrePosting";
import * as planningSaga from "./planning";
import * as trackerSaga from "./tracker";

export default function* rootSaga() {
  yield [
    appSaga.watchRequestEnvironment(),
    appSaga.watchRequestEmployee(),
    appSaga.watchReadFileB64(),

    postSaga.watchRequestPost(),
    postSaga.watchRequestPostFiltered(),
    postSaga.watchRequestUnlinkedFiltered(),
    postSaga.watchRequestArchivedFiltered(),
    // postSaga.watchRequestAssignPostDisplay(),
    postSaga.watchRequestPostClientScrubbing(),
    postSaga.watchRequestUniqueIscis(),
    postSaga.watchRequestScrubbingDataFiltered(),
    postSaga.watchRequestClearScrubbingFiltersList(),
    postSaga.watchRequestOverrideStatus(),
    postSaga.watchSwapProposalDetail(),
    postSaga.watchArchiveUnlinkedIsci(),
    postSaga.watchRequestUniqueIscisSuccess(),
    postSaga.watchRequestArchivedIscisSuccess(),
    postSaga.watchLoadArchivedIscis(),
    postSaga.watchLoadValidIscis(),
    postSaga.watchMapUnlinkedIsci(),
    postSaga.watchMapUnlinkedIsciSuccess(),
    postSaga.watchRescrubUnlinkedIsci(),
    postSaga.watchCloseUnlinkedIsciModal(),
    postSaga.watchUndoArchivedIscis(),
    postSaga.watchUndoScrubStatus(),
    postSaga.watchUndoScrubStatusSuccess(),
    postSaga.watchRequestClearFilteredScrubbingData(),
    postSaga.watchRequestProcessNtiFile(),
    postSaga.watchProcessNtiFileSuccess(),

    postPrePostingSaga.watchRequestPostPrePostingInitialData(),
    postPrePostingSaga.watchRequestPostPrePosting(),
    postPrePostingSaga.watchRequestPostPrePostingFiltered(),
    postPrePostingSaga.watchDeletePostPrePostingById(),
    postPrePostingSaga.watchDeletePostPrePostingByIdSuccess(),
    postPrePostingSaga.watchRequestPostPrePostingFileEdit(),
    postPrePostingSaga.watchPostPrePostingFileEditSuccess(),
    postPrePostingSaga.watchPostPrePostingFileSave(),
    postPrePostingSaga.watchPostPrePostingFileSaveSuccess(),
    postPrePostingSaga.watchUploadPostPrePostingFile(),
    postPrePostingSaga.watchUploadPostPrePostingFileSuccess(),

    planningSaga.watchRequestProposalInitialData(),
    planningSaga.watchRequestProposals(),
    planningSaga.watchRequestProposalLock(),
    planningSaga.watchRequestProposalUnlock(),
    planningSaga.watchRequestProposal(),
    planningSaga.watchRequestProposalVersions(),
    planningSaga.watchRequestProposalVersion(),
    planningSaga.watchSaveProposal(),
    planningSaga.watchSaveProposalAsVersion(),
    planningSaga.watchDeleteProposalById(),
    planningSaga.watchUpdateProposal(),
    planningSaga.watchModelNewProposalDetail(),
    planningSaga.watchModelUnorderProposal(),
    planningSaga.watchRequestGenres(),
    planningSaga.watchRequestPrograms(),
    planningSaga.watchRequestShowTypes(),
    planningSaga.watchDeleteProposalDetail(),
    planningSaga.watchRequestPlanningFiltered(),
    planningSaga.watchRerunPostScrubing(),
    planningSaga.watchLoadOpenMarketData(),
    planningSaga.watchUpdateEditMarketsData(),
    planningSaga.watchUpdateEditMarketsDataSuccess(),
    planningSaga.watchUpdateProprietaryCpms(),
    planningSaga.watchUploadSCXFile(),
    planningSaga.watchAllocateSpots(),
    // planningSaga.watchUploadSCXFileSuccess(),
    planningSaga.watchFilterOpenMarketData(),
    planningSaga.watchLoadPricingDataSuccess(),
    planningSaga.watchLoadPricingData(),
    planningSaga.watchSavePricingData(),
    planningSaga.watchCopyToBuyFlow(),
    planningSaga.watchCopyToBuySaga(),
    planningSaga.watchGenerateScx(),
    planningSaga.watchGenerateScxSuccess(),

    trackerSaga.watchUploadTrackerFile(),
    trackerSaga.watchUploadTrackerFileSuccess(),
    trackerSaga.watchRequestTracker(),
    trackerSaga.watchRequestTrackerFiltered(),
    trackerSaga.watchRequestUnlinkedFiltered(),
    trackerSaga.watchRequestArchivedFiltered(),
    trackerSaga.watchRequestTrackerClientScrubbing(),
    trackerSaga.watchRequestUniqueIscis(),
    trackerSaga.watchRequestScrubbingDataFiltered(),
    trackerSaga.watchRequestClearScrubbingFiltersList(),
    trackerSaga.watchRequestOverrideStatus(),
    trackerSaga.watchSwapProposalDetail(),
    trackerSaga.watchArchiveUnlinkedIsci(),
    trackerSaga.watchRequestUniqueIscisSuccess(),
    trackerSaga.watchRequestArchivedIscisSuccess(),
    trackerSaga.watchLoadArchivedIscis(),
    trackerSaga.watchLoadValidIscis(),
    trackerSaga.watchMapUnlinkedIsci(),
    trackerSaga.watchMapUnlinkedIsciSuccess(),
    trackerSaga.watchRescrubUnlinkedIsci(),
    trackerSaga.watchCloseUnlinkedIsciModal(),
    trackerSaga.watchUndoArchivedIscis(),
    trackerSaga.watchUndoScrubStatus(),
    trackerSaga.watchUndoScrubStatusSuccess(),
    trackerSaga.watchRequestClearFilteredScrubbingData()
  ];
}
