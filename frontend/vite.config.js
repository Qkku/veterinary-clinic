import { defineConfig, loadEnv } from 'vite';
export default defineConfig(({mode})=>{
 const env=loadEnv(mode,process.cwd(),'');
 const target=env.VITE_BACKEND_TARGET||'https://localhost:7084';
 const proxy={target,changeOrigin:true,secure:!/^https:\/\/(localhost|127\.0\.0\.1)(:\d+)?$/.test(target)};
 return {server:{port:5173,strictPort:true,proxy:{'/api':proxy,'/uploads':proxy}}};
});
