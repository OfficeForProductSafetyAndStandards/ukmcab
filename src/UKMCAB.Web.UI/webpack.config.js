const path = require("path");

module.exports = {
	devtool: "source-map",
	mode: "development",
	entry: "./Assets/js/main.js",
	output: {
		filename: "main.js",
		path: path.resolve(__dirname, "wwwroot/assets/js"),
	},
	module: {
		rules: [
            {
				test: /\.js$/,
				exclude: /node_modules/,
				use: {
					loader: "babel-loader",
					options: {
						presets: ["@babel/preset-env"],
                    },
                },
            },
        ],
    },
};