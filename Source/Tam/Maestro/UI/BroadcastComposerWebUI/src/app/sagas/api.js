import axios from 'axios';
import { call } from 'redux-saga/effects';

// Methods
const GET = axios.get;
const POST = axios.post;
const PUT = axios.put;
const DELETE = axios.delete;

// Bases
/* eslint-disable no-undef */
const apiBase = __API__;

// Requests
// call(METHOD, ...args)
const app = {
  getEnvironment: () => (
    call(GET, `${apiBase}environment`, {})
  ),
  getEmployee: () => (
    call(GET, `${apiBase}employee`, {})
  ),
};

const post = {
  getPosts: () => call(() => (
    {
      status: 200,
      data: {
        Success: true,
        Data: [{
          Id: 1,
          FileName: 'Galago crassicaudataus',
          Source: 'Aimbo',
          UploadDate: '2017-03-06 12:37:50',
        }, {
          Id: 2,
          FileName: 'Rhabdomys pumilio',
          Source: 'Riffwire',
          UploadDate: '2017-11-20 17:30:44',
        }, {
          Id: 3,
          FileName: 'Suricata suricatta',
          Source: 'BlogXS',
          UploadDate: '2017-07-28 00:47:37',
        }, {
          Id: 4,
          FileName: 'Meleagris gallopavo',
          Source: 'Twinder',
          UploadDate: '2017-02-26 08:14:24',
        }, {
          Id: 5,
          FileName: 'Bugeranus caruncalatus',
          Source: 'Dabvine',
          UploadDate: '2017-09-22 20:06:55',
        }, {
          Id: 6,
          FileName: 'Nectarinia chalybea',
          Source: 'Dablist',
          UploadDate: '2017-12-12 01:24:27',
        }, {
          Id: 7,
          FileName: 'Macropus parryi',
          Source: 'Skajo',
          UploadDate: '2017-03-06 21:03:28',
        }, {
          Id: 8,
          FileName: 'Eubalaena australis',
          Source: 'Linkbuzz',
          UploadDate: '2017-06-24 20:54:05',
        }, {
          Id: 9,
          FileName: 'Thamnolaea cinnmomeiventris',
          Source: 'Yakidoo',
          UploadDate: '2017-03-11 11:16:24',
        }, {
          Id: 10,
          FileName: 'Otaria flavescens',
          Source: 'Skivee',
          UploadDate: '2017-11-07 22:20:37',
        }, {
          Id: 11,
          FileName: 'Loxodonta africana',
          Source: 'Vitz',
          UploadDate: '2017-02-24 23:10:02',
        }, {
          Id: 12,
          FileName: 'Dasyurus maculatus',
          Source: 'Yakijo',
          UploadDate: '2017-04-20 11:50:52',
        }, {
          Id: 13,
          FileName: 'Marmota flaviventris',
          Source: 'JumpXS',
          UploadDate: '2017-01-06 00:37:35',
        }, {
          Id: 14,
          FileName: 'Larus fuliginosus',
          Source: 'Browsezoom',
          UploadDate: '2017-01-28 12:40:08',
        }, {
          Id: 15,
          FileName: 'Sciurus niger',
          Source: 'Katz',
          UploadDate: '2017-10-01 11:52:27',
        }, {
          Id: 16,
          FileName: 'Damaliscus lunatus',
          Source: 'Skynoodle',
          UploadDate: '2017-04-08 00:10:50',
        }, {
          Id: 17,
          FileName: 'Trichosurus vulpecula',
          Source: 'Skaboo',
          UploadDate: '2017-06-25 19:44:59',
        }, {
          Id: 18,
          FileName: 'Smithopsis crassicaudata',
          Source: 'Bluezoom',
          UploadDate: '2017-04-28 00:56:48',
        }, {
          Id: 19,
          FileName: 'Laniaurius atrococcineus',
          Source: 'Zava',
          UploadDate: '2017-09-30 12:41:23',
        }, {
          Id: 20,
          FileName: 'Agkistrodon piscivorus',
          Source: 'Oodoo',
          UploadDate: '2017-03-03 14:52:59',
        }, {
          Id: 21,
          FileName: 'Loxodonta africana',
          Source: 'Mymm',
          UploadDate: '2017-09-03 21:32:13',
        }, {
          Id: 22,
          FileName: 'Zonotrichia capensis',
          Source: 'Kwilith',
          UploadDate: '2017-03-12 21:00:40',
        }, {
          Id: 23,
          FileName: 'Phalaropus lobatus',
          Source: 'Devpulse',
          UploadDate: '2017-09-08 18:15:42',
        }, {
          Id: 24,
          FileName: 'Macropus rufogriseus',
          Source: 'Kazio',
          UploadDate: '2017-01-23 04:03:16',
        }, {
          Id: 25,
          FileName: 'Morelia spilotes variegata',
          Source: 'Avavee',
          UploadDate: '2017-02-07 01:14:52',
        }, {
          Id: 26,
          FileName: 'Hippotragus equinus',
          Source: 'Thoughtworks',
          UploadDate: '2017-08-08 22:54:48',
        }, {
          Id: 27,
          FileName: 'Cynomys ludovicianus',
          Source: 'Feedbug',
          UploadDate: '2017-02-04 05:39:24',
        }, {
          Id: 28,
          FileName: 'Eutamias minimus',
          Source: 'Blogspan',
          UploadDate: '2017-12-18 10:23:46',
        }, {
          Id: 29,
          FileName: 'Thalasseus maximus',
          Source: 'Voolith',
          UploadDate: '2017-11-03 06:53:29',
        }, {
          Id: 30,
          FileName: 'Charadrius tricollaris',
          Source: 'Realbuzz',
          UploadDate: '2017-09-16 18:55:20',
        }, {
          Id: 31,
          FileName: 'unavailable',
          Source: 'Rhynoodle',
          UploadDate: '2017-10-22 06:44:02',
        }, {
          Id: 32,
          FileName: 'Tadorna tadorna',
          Source: 'InnoZ',
          UploadDate: '2017-08-08 07:05:37',
        }, {
          Id: 33,
          FileName: 'Phalacrocorax carbo',
          Source: 'Vimbo',
          UploadDate: '2017-01-08 01:40:23',
        }, {
          Id: 34,
          FileName: 'Dasyurus maculatus',
          Source: 'Twitterbridge',
          UploadDate: '2017-02-24 12:43:43',
        }, {
          Id: 35,
          FileName: 'Pandon haliaetus',
          Source: 'Linkbridge',
          UploadDate: '2017-10-18 23:07:55',
        }, {
          Id: 36,
          FileName: 'Semnopithecus entellus',
          Source: 'Zoonoodle',
          UploadDate: '2017-08-22 20:17:35',
        }, {
          Id: 37,
          FileName: 'Vanessa indica',
          Source: 'Tambee',
          UploadDate: '2017-03-13 16:42:46',
        }, {
          Id: 38,
          FileName: 'Tyto novaehollandiae',
          Source: 'Oyoyo',
          UploadDate: '2017-04-01 16:02:52',
        }, {
          Id: 39,
          FileName: 'Anhinga rufa',
          Source: 'Skyvu',
          UploadDate: '2017-07-17 06:23:14',
        }, {
          Id: 40,
          FileName: 'Ovibos moschatus',
          Source: 'Brainbox',
          UploadDate: '2017-07-11 14:07:06',
        }, {
          Id: 41,
          FileName: 'Sus scrofa',
          Source: 'Tagchat',
          UploadDate: '2017-05-18 23:20:33',
        }, {
          Id: 42,
          FileName: 'Phoca vitulina',
          Source: 'Youtags',
          UploadDate: '2017-01-19 20:45:50',
        }, {
          Id: 43,
          FileName: 'Madoqua kirkii',
          Source: 'Brightdog',
          UploadDate: '2017-03-03 22:47:03',
        }, {
          Id: 44,
          FileName: 'Nucifraga columbiana',
          Source: 'Vipe',
          UploadDate: '2017-12-14 19:34:42',
        }, {
          Id: 45,
          FileName: 'Bassariscus astutus',
          Source: 'Jetwire',
          UploadDate: '2017-05-27 14:13:13',
        }, {
          Id: 46,
          FileName: 'Kobus defassa',
          Source: 'Realmix',
          UploadDate: '2017-05-17 13:25:15',
        }, {
          Id: 47,
          FileName: 'Paradoxurus hermaphroditus',
          Source: 'Jaxspan',
          UploadDate: '2017-06-17 02:31:51',
        }, {
          Id: 48,
          FileName: 'Colobus guerza',
          Source: 'Leexo',
          UploadDate: '2017-09-03 04:47:14',
        }, {
          Id: 49,
          FileName: 'Anser caerulescens',
          Source: 'Browsetype',
          UploadDate: '2017-05-07 10:19:06',
        }, {
          Id: 50,
          FileName: 'Ramphastos tucanus',
          Source: 'Jaloo',
          UploadDate: '2017-07-07 17:05:22',
        }, {
          Id: 51,
          FileName: 'Anastomus oscitans',
          Source: 'Edgeclub',
          UploadDate: '2017-09-26 21:42:17',
        }, {
          Id: 52,
          FileName: 'Sagittarius serpentarius',
          Source: 'Skinix',
          UploadDate: '2017-06-02 00:40:19',
        }, {
          Id: 53,
          FileName: 'Dicrurus adsimilis',
          Source: 'Quamba',
          UploadDate: '2017-12-21 22:15:50',
        }, {
          Id: 54,
          FileName: 'Tragelaphus strepsiceros',
          Source: 'Latz',
          UploadDate: '2017-07-22 11:58:58',
        }, {
          Id: 55,
          FileName: 'Ceratotherium simum',
          Source: 'Livetube',
          UploadDate: '2017-02-26 15:45:30',
        }, {
          Id: 56,
          FileName: 'Haliaetus vocifer',
          Source: 'Zoovu',
          UploadDate: '2017-05-06 16:17:24',
        }, {
          Id: 57,
          FileName: 'Bassariscus astutus',
          Source: 'Realpoint',
          UploadDate: '2017-07-24 10:12:28',
        }, {
          Id: 58,
          FileName: 'Hippotragus niger',
          Source: 'Oyonder',
          UploadDate: '2017-10-16 15:49:38',
        }, {
          Id: 59,
          FileName: 'Antilocapra americana',
          Source: 'Zoonder',
          UploadDate: '2017-03-18 05:48:03',
        }, {
          Id: 60,
          FileName: 'Bucephala clangula',
          Source: 'Cogibox',
          UploadDate: '2017-06-23 06:58:38',
        }, {
          Id: 61,
          FileName: 'Varanus sp.',
          Source: 'Ainyx',
          UploadDate: '2017-11-21 19:44:15',
        }, {
          Id: 62,
          FileName: 'Spizaetus coronatus',
          Source: 'Skyvu',
          UploadDate: '2017-06-15 06:20:20',
        }, {
          Id: 63,
          FileName: 'Oxybelis sp.',
          Source: 'Shufflester',
          UploadDate: '2017-02-06 23:51:16',
        }, {
          Id: 64,
          FileName: 'Genetta genetta',
          Source: 'Bluezoom',
          UploadDate: '2017-07-27 19:32:49',
        }, {
          Id: 65,
          FileName: 'Cracticus nigroagularis',
          Source: 'Oodoo',
          UploadDate: '2017-01-23 22:43:41',
        }, {
          Id: 66,
          FileName: 'Ctenophorus ornatus',
          Source: 'Riffwire',
          UploadDate: '2017-02-14 07:58:07',
        }, {
          Id: 67,
          FileName: 'Sylvilagus floridanus',
          Source: 'Flashspan',
          UploadDate: '2017-06-16 14:05:44',
        }, {
          Id: 68,
          FileName: 'Ursus maritimus',
          Source: 'Fliptune',
          UploadDate: '2017-04-23 03:47:05',
        }, {
          Id: 69,
          FileName: 'Cochlearius cochlearius',
          Source: 'Yacero',
          UploadDate: '2017-12-30 23:59:48',
        }, {
          Id: 70,
          FileName: 'Castor fiber',
          Source: 'Aimbo',
          UploadDate: '2017-09-10 20:46:53',
        }, {
          Id: 71,
          FileName: 'Eremophila alpestris',
          Source: 'Jayo',
          UploadDate: '2017-06-29 15:55:21',
        }, {
          Id: 72,
          FileName: 'Pteronura brasiliensis',
          Source: 'Yozio',
          UploadDate: '2017-09-26 01:31:02',
        }, {
          Id: 73,
          FileName: 'Bassariscus astutus',
          Source: 'Thoughtstorm',
          UploadDate: '2017-12-19 01:51:33',
        }, {
          Id: 74,
          FileName: 'Ceryle rudis',
          Source: 'Zoomlounge',
          UploadDate: '2017-01-11 22:29:02',
        }, {
          Id: 75,
          FileName: 'Petaurus norfolcensis',
          Source: 'Edgetag',
          UploadDate: '2017-12-13 05:58:08',
        }, {
          Id: 76,
          FileName: 'Haematopus ater',
          Source: 'Browsecat',
          UploadDate: '2017-02-27 16:02:12',
        }, {
          Id: 77,
          FileName: 'Chloephaga melanoptera',
          Source: 'Yodo',
          UploadDate: '2017-08-26 05:33:09',
        }, {
          Id: 78,
          FileName: 'Iguana iguana',
          Source: 'Centidel',
          UploadDate: '2017-11-28 10:36:25',
        }, {
          Id: 79,
          FileName: 'Tragelaphus strepsiceros',
          Source: 'Fatz',
          UploadDate: '2017-02-27 18:04:28',
        }, {
          Id: 80,
          FileName: 'Eudromia elegans',
          Source: 'Leexo',
          UploadDate: '2017-07-22 05:53:32',
        }, {
          Id: 81,
          FileName: 'Canis lupus lycaon',
          Source: 'Zoonder',
          UploadDate: '2017-05-30 04:39:56',
        }, {
          Id: 82,
          FileName: 'Didelphis virginiana',
          Source: 'Fliptune',
          UploadDate: '2017-06-28 03:59:21',
        }, {
          Id: 83,
          FileName: 'Diceros bicornis',
          Source: 'Leexo',
          UploadDate: '2017-09-29 17:18:25',
        }, {
          Id: 84,
          FileName: 'Oryx gazella',
          Source: 'Flashset',
          UploadDate: '2017-05-20 16:09:34',
        }, {
          Id: 85,
          FileName: 'Bucephala clangula',
          Source: 'Eidel',
          UploadDate: '2017-05-09 18:50:30',
        }, {
          Id: 86,
          FileName: 'Eumetopias jubatus',
          Source: 'Riffwire',
          UploadDate: '2017-10-04 10:11:19',
        }, {
          Id: 87,
          FileName: 'Threskionis aethiopicus',
          Source: 'Skippad',
          UploadDate: '2017-10-23 05:45:59',
        }, {
          Id: 88,
          FileName: 'Tamiasciurus hudsonicus',
          Source: 'Voonte',
          UploadDate: '2017-02-12 09:42:21',
        }, {
          Id: 89,
          FileName: 'Genetta genetta',
          Source: 'Youtags',
          UploadDate: '2017-08-18 16:44:05',
        }, {
          Id: 90,
          FileName: 'Smithopsis crassicaudata',
          Source: 'Quinu',
          UploadDate: '2017-07-31 07:33:19',
        }, {
          Id: 91,
          FileName: 'Corvus brachyrhynchos',
          Source: 'Yamia',
          UploadDate: '2017-01-07 01:50:50',
        }, {
          Id: 92,
          FileName: 'Dipodomys deserti',
          Source: 'Kazio',
          UploadDate: '2017-01-17 06:32:28',
        }, {
          Id: 93,
          FileName: 'Hystrix cristata',
          Source: 'Layo',
          UploadDate: '2017-01-12 15:15:11',
        }, {
          Id: 94,
          FileName: 'Arctogalidia trivirgata',
          Source: 'Photolist',
          UploadDate: '2017-08-06 11:01:56',
        }, {
          Id: 95,
          FileName: 'Macaca mulatta',
          Source: 'Realcube',
          UploadDate: '2017-06-15 15:32:48',
        }, {
          Id: 96,
          FileName: 'Passer domesticus',
          Source: 'Dynabox',
          UploadDate: '2017-10-06 21:42:52',
        }, {
          Id: 97,
          FileName: 'Canis aureus',
          Source: 'Tagtune',
          UploadDate: '2017-04-07 23:51:32',
        }, {
          Id: 98,
          FileName: 'Pseudocheirus peregrinus',
          Source: 'Jaloo',
          UploadDate: '2017-01-08 03:59:45',
        }, {
          Id: 99,
          FileName: 'Anastomus oscitans',
          Source: 'Vinder',
          UploadDate: '2017-05-26 13:12:14',
        }, {
          Id: 100,
          FileName: 'Erethizon dorsatum',
          Source: 'Zoomlounge',
          UploadDate: '2017-01-23 21:32:11',
        }],
      },
    }
  )),
};

const postPrePosting = {
  getPrePostInitialData: () => (
    call(GET, `${apiBase}PostPrePosting/InitialData`, {})
  ),
  getPosts: () => (
    call(GET, `${apiBase}PostPrePosting`, {})
  ),
  getPost: id => (
    call(GET, `${apiBase}PostPrePosting/${id}`, {})
  ),
  uploadPost: params => (
    call(POST, `${apiBase}PostPrePosting`, params)
  ),
  savePost: params => (
    call(PUT, `${apiBase}PostPrePosting`, params)
  ),
  deletePost: id => (
    call(DELETE, `${apiBase}PostPrePosting/${id}`, {})
  ),
};

const planning = {
  getProposalInitialData: () => (
    call(GET, `${apiBase}Proposals/InitialData`, {})
  ),
  getProposals: () => (
    call(GET, `${apiBase}Proposals/GetProposals`, {})
  ),
  getProposalLock: id => (
    call(GET, `${apiBase}Proposals/Proposal/${id}/Lock`, {})
  ),
  getProposalUnlock: id => (
    call(GET, `${apiBase}Proposals/Proposal/${id}/Lock`, {})
  ),
  getProposal: id => (
    call(GET, `${apiBase}Proposals/Proposal/${id}`, {})
  ),
  getProposalVersions: id => (
    call(GET, `${apiBase}Proposals/Proposal/${id}/Versions`, {})
  ),
  getProposalVersion: params => (
    call(GET, `${apiBase}Proposals/Proposal/${params.id}/Versions/${params.version}`, {})
  ),
  saveProposal: params => (
    call(POST, `${apiBase}Proposals/SaveProposal`, params)
  ),
  deleteProposal: id => (
    call(DELETE, `${apiBase}Proposals/DeleteProposal/${id}`, {})
  ),
  unorderProposal: id => (
    call(POST, `${apiBase}Proposals/UnorderProposal?proposalId=${id}`, {})
  ),
  getProposalDetail: params => (
    call(POST, `${apiBase}Proposals/GetProposalDetail`, params)
  ),
  updateProposal: params => (
    call(POST, `${apiBase}Proposals/UpdateProposal`, params)
  ),
};

// Calls
const api = {
  app,
  post,
  postPrePosting,
  planning,
};

export default api;
