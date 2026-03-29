/** @type {import('next').NextConfig} */
const nextConfig = {
  output: "standalone",
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${process.env.GATEWAY_URL || "http://localhost:5000"}/api/:path*`,
      },
      {
        source: "/hubs/:path*",
        destination: `${process.env.GATEWAY_URL || "http://localhost:5000"}/hubs/:path*`,
      },
    ];
  },
};

module.exports = nextConfig;
