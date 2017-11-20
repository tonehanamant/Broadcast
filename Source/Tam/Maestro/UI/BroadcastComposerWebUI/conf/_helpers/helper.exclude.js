var pkg = require('../../package.json');

var exclusions = [
  'font-awesome',
  'animatewithsass',
];

var dependencies = Object.keys(pkg.dependencies).filter(dep => !exclusions.includes(dep));

module.exports = dependencies;
