const { merge } = require('webpack-merge');
const common = require('./webpack.config.js');

module.exports = merge(common, {
    mode: 'development',
    devtool: "nosources-source-map",
    devServer: {
        host: 'local-ip',
        port: '',
        static: false,
        compress: true,
        port: 9000,
    }
});
