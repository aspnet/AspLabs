const path = require('path');
const TsconfigPathsPlugin = require('tsconfig-paths-webpack-plugin');

module.exports = (env, args) => ({
    target: 'electron-renderer',
    resolve: {
        extensions: ['.ts', '.js'],
        plugins: [new TsconfigPathsPlugin()]
    },
    devtool: 'inline-source-map',
    module: {
        rules: [{ test: /\.ts?$/, loader: 'ts-loader' }]
    },
    entry: {
        'components.electron': './src/Boot.Electron.ts'
    },
    output: { path: path.join(__dirname, '/dist'), filename: '[name].js' }
});
