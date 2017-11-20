const context = require.context('./app', true, /.+\.spec\.(js|jsx)?$/);
context.keys().forEach(context);
