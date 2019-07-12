const path = require('path');

module.exports = {
    entry: './SVDATA/compiled/js/main.js',
    output: {
        filename: 'main.js',
        path: path.resolve(__dirname, 'SVDATA/dist/js')
    },
    mode: 'production'
};