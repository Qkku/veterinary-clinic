import { API_URL } from './api';
export async function authRequest(path, options={}) {
 const response=await fetch(`${API_URL}/${path}`,options);
 const data=await response.json().catch(()=>null);
 if(!response.ok){const error=new Error(data?.message||`Ошибка ${response.status}: ${path}`);error.status=response.status;error.data=data;throw error;}
 if(!data||typeof data!=='object')throw new Error(`Некорректный ответ: ${path}`);
 return data;
}
export async function login(payload){
 const options={method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(payload)};
 let data;
 try{data=await authRequest('Auth/login',options);}catch(e){if(e.status!==401)throw e;data=await authRequest('Auth/groomer-login',options);}
 if(typeof data.token!=='string'||!data.token)throw new Error('Сервер не вернул токен.');
 if(!data.user&&!data.groomer)throw new Error('Сервер не вернул профиль.');
 return {token:data.token,kind:data.groomer?'groomer':'user'};
}
export async function profile(token,kind,signal){
 const path=kind==='groomer'?'GroomerCabinet/me':'Auth/me';
 const data=await authRequest(path,{headers:{Authorization:`Bearer ${token}`},signal});
 if(data.id==null||!data.email)throw new Error('Сервер вернул неполный профиль.');
 return kind==='groomer'?{...data,role:'Groomer'}:data;
}
