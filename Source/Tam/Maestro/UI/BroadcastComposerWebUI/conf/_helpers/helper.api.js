module.exports = {
  local: {
    host: 'http://localhost:8080/',
    apiName: 'api',
    // version: 'v1',
    proxyToDev: {
      '/api/**': {
        target: 'http://localhost:63944',
        secure: false,
        changeOrigin: true,
        // ws: true, // proxy websockets
        logLevel: 'error',
      },
    },
  },
  development: {
    host: 'http://cadapps-qa1.crossmw.com/broadcast/',
    apiName: 'api',
    // version: 'v1',
  },
  qa: {
    host: 'http://cadapps-qa1.crossmw.com/broadcast/',
    apiName: 'api',
    // version: 'v1',
  },
  production: {
    host: 'http://cadapps-qa1.crossmw.com/broadcast/',
    apiName: 'api',
    // version: 'v1',
  },
  createApi: function(type) {
    return JSON.stringify(this[type].host + this[type].apiName + '/'); // + this[type].version + '/'
  },
};