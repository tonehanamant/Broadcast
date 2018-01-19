var pkg = require('../../package.json');

var exclusions = [
  'font-awesome',
  'animatewithsass',

  // No need to reload out of order
  'babel-polyfill',
  'react',
  'react-dom',
];

var dependencies = Object.keys(pkg.dependencies).filter(dep => !exclusions.includes(dep));

console.log('Compiling Vendor Dependencies', dependencies);

module.exports = dependencies;
