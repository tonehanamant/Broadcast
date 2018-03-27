import * as appSaga from './app';
import * as postSaga from './post';
import * as postPrePostingSaga from './postPrePosting';
import * as planningSaga from './planning';

export default function* rootSaga() {
  yield [

    appSaga.watchRequestEnvironment(),
    appSaga.watchRequestEmployee(),
    appSaga.watchReadFileB64(),

    postSaga.watchRequestPost(),
    postSaga.watchRequestPostFiltered(),
    postSaga.watchRequestAssignPostDisplay(),
    postSaga.watchRequestPostScrubbingHeader(),

    postPrePostingSaga.watchRequestPostPrePostingInitialData(),
    postPrePostingSaga.watchRequestPostPrePosting(),
    postPrePostingSaga.watchRequestAssignPostPrePostingDisplay(),
    postPrePostingSaga.watchRequestPostPrePostingFiltered(),
    postPrePostingSaga.watchDeletePostPrePostingById(),
    postPrePostingSaga.watchRequestPostPrePostingFileEdit(),
    postPrePostingSaga.watchSavePostPrePostingFileEdit(),
    postPrePostingSaga.watchUploadPostPrePostingFile(),

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
    planningSaga.watchDeleteProposalDetail(),
  ];
}
