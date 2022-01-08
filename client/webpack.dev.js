const { merge } = require('webpack-merge');
const common = require('./webpack.config.js');

module.exports = merge(common, {
    mode: 'development',
    devtool: "source-map",
    devServer: {
        host: 'local-ip',
        port: '',
        static: false,
        compress: true,
        host: '127.0.0.1',
        port: 5001,
    }
});
