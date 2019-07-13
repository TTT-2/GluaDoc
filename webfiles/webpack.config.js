const path = require('path');

module.exports = {
    entry: './__workspace__/compiled/js/main.js',
    output: {
        filename: 'main.js',
        path: path.resolve(__dirname, 'SVDATA/dist/js')
    },
    mode: 'production'
};