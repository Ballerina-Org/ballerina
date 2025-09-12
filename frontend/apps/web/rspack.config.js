const rspack = require("@rspack/core");
const refreshPlugin = require("@rspack/plugin-react-refresh");
const isDev = process.env.NODE_ENV === "development";
const dotenv = require("dotenv");
const dotenvExpand = require("dotenv-expand");
const fs = require("node:fs");
const envFile = fs.existsSync(".env.ide") ? ".env.ide" : ".env";
dotenv.config({ path: envFile });

console.log(`Loaded env file: ${envFile}`);
const isIDE = process.env.APP_FLAVOR === "ide";

const parsed = dotenv.config();       
dotenvExpand.expand(parsed);
const PUBLIC_KEYS = ["APP_FLAVOR", "API_ORIGIN", "API_PREFIX", "TENANT_ID"];
const injected = Object.fromEntries(
    PUBLIC_KEYS.map(k => [`process.env.${k}`, JSON.stringify(process.env[k] ?? "")])
);

/**
 * @type {import('@rspack/cli').Configuration}
 */
module.exports = {
  context: __dirname,
  entry: {
    main: "./main.tsx",
  },
  resolve: {
    extensions: ["...", ".ts", ".tsx", ".jsx"],
  },
  module: {
    rules: [
      {
        test: /\.svg$/,
        type: "asset",
      },
      {
        test: /\.(jsx?|tsx?)$/,
        use: [
          {
            loader: "builtin:swc-loader",
            options: {
              sourceMap: true,
              jsc: {
                parser: {
                  syntax: "typescript",
                  tsx: true,
                },
                transform: {
                  react: {
                    runtime: "automatic",
                    development: isDev,
                    refresh: isDev,
                  },
                },
              },
              env: {
                targets: [
                  "chrome >= 87",
                  "edge >= 88",
                  "firefox >= 78",
                  "safari >= 14",
                ],
              },
            },
          },
        ],
      },
    ],
  },
  devServer: {
    port: 5001,
      ...(isIDE && {
          proxy: {
              [process.env.API_PREFIX]: {
                  target: process.env.API_ORIGIN,
                  changeOrigin: true,
                  secure: false,
                  onProxyReq(proxyReq /*, req, res */) {
                      proxyReq.setHeader("Tenant-Id", process.env.TENANT_ID);
                  },
              },
          },
      }),
  },
  plugins: [
    new rspack.DefinePlugin({
      "process.env.NODE_ENV": JSON.stringify(process.env.NODE_ENV),
        //IDE
        __ENV__: JSON.stringify(injected), 
        ...injected,
    }),
    new rspack.ProgressPlugin({}),
    new rspack.HtmlRspackPlugin({
      template: "./index.html",
    }),
    isDev ? new refreshPlugin() : null,
  ].filter(Boolean),
};
