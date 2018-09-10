module.exports = {
  local: {
    host: 'http://localhost:8080/',
    apiName: 'api',
    // version: 'v1',
    proxyToDev: {
      '/api/**': {
        target: 'http://localhost:61799',
        secure: false,
        changeOrigin: true,
        // ws: true, // proxy websockets
        logLevel: 'error',
      },
    },
  },
  development: {
    host: '/broadcast/',
    apiName: 'api',
    // version: 'v1',
  },
  qa: {
    host: '/broadcast/',
    apiName: 'api',
    // version: 'v1',
  },
  production: {
    host: '/broadcast/',
    apiName: 'api',
    // version: 'v1',
  },
  createApi: function(type) {
    return JSON.stringify(this[type].host + this[type].apiName + '/'); // + this[type].version + '/'
  },
};
