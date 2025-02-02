import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import { env } from 'process';

const baseFolder =
    env.APPDATA && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

const certificateName = "openweatherproxyapp.client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

const httpsConfig: { key: Buffer; cert: Buffer } | undefined = fs.existsSync(certFilePath) && fs.existsSync(keyFilePath)
    ? {
        key: fs.readFileSync(keyFilePath),
        cert: fs.readFileSync(certFilePath),
    }
    : undefined; 

const backendUrl = 'https://localhost:7250'; // Matches backend launch settings
console.log("Proxying API calls to:", backendUrl);

export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        proxy: {
            '/api': {
                target: backendUrl,
                changeOrigin: true,
                secure: false,
            }
        },
        port: 5173, // Frontend port
        https: httpsConfig, // Uses HTTPS if certs exist, otherwise defaults to HTTP
    }
});
