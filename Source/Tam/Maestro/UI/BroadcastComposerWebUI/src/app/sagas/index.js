import * as appSaga from './app';
import * as postSaga from './post';
import * as postPrePostingSaga from './postPrePosting';
import * as planningSaga from './planning';
import * as trackerSaga from './tracker';

export default function* rootSaga() {
  yield [

    appSaga.watchRequestEnvironment(),
    appSaga.watchRequestEmployee(),
    appSaga.watchReadFileB64(),

    postSaga.watchRequestPost(),
    postSaga.watchRequestPostFiltered(),
    postSaga.watchRequestUnlinkedFiltered(),
    postSaga.watchRequestArchivedFiltered(),
    postSaga.watchRequestAssignPostDisplay(),
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
    planningSaga.watchUploadSCXFile(),
    // planningSaga.watchUploadSCXFileSuccess(),
    planningSaga.watchFilterOpenMarketData(),

    trackerSaga.watchUploadTrackerFile(),
    trackerSaga.watchUploadTrackerFileSuccess(),
  ];
}
