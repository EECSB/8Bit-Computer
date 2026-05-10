const path = require("path");
const TerserPlugin = require("terser-webpack-plugin");
const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');

module.exports = {
    mode: "development",
    entry: ["./src/MonacoInterop.js"],
    output: {
        path: path.resolve(__dirname, "../wwwroot/js/webpack-bundle"),
        filename: "index.bundle.js"
    },
    module: {
        rules: [
            {
                test: /\.css$/,
                use: ['style-loader', 'css-loader']
            },
            {
                test: /\.ttf$/,
                use: ['file-loader']
            }
        ]
    },
    plugins: [new MonacoWebpackPlugin()],
    optimization: {
        //We don't want to minimize our code(while developing).
        minimize: true, //false
        minimizer: [
            new TerserPlugin({
                parallel: true,
                terserOptions: {
                    // https://github.com/webpack-contrib/terser-webpack-plugin#terseroptions
                }
            })
        ]
    }
}