module.exports = {
  development: {
    url: '/api',
    name: '',
    version: '',
  },
  qa: {
    // API and application will sit side-by-side on server, reference relatively
    // (i.e. API: http://cadapps-qa1/broadcastapi, Application: http://cadapps-qa1/broadcast)
    url: 'http://127.0.0.1/broadcastapi',
    name: '/api', // 'api'
    version: '', // 'v1'
  },
  production: {
    url: 'http://127.0.0.1/broadcastapi',
    name: '/api', // 'api'
    version: '', // 'v1'
  },
  createApi: function(type) {
    return JSON.stringify(this[type].url + this[type].name + this[type].version + '/');
  },
  proxy: {
    '/api': {
      target: 'http://localhost:63944',
      secure: false,
      changeOrigin: true,
      ws: true, // proxy websockets
      logLevel: 'error',
      // pathRewrite: {
      //   '^/api': ''
      // }
    }
  }
};
