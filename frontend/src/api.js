export const API_URL = import.meta.env.VITE_API_URL || '/api';
export const routes = {pets:'Pets', appointments:'Appointments', slots:'Schedule/available', myAppointments:'Appointments', myPets:'Pets', myReviews:'Reviews', reviews:'Reviews', groomerSchedule:'GroomerCabinet/schedule', leaves:'LeaveRequests', myLeaves:'LeaveRequests/my', moderation:'Reviews/pending', adminAppointments:'Appointments/all', adminSchedule:'AdminGroomerSchedules'};
export async function request(path, token, method='GET', body, signal){
 const response=await fetch(`${API_URL}/${path}`, {method,signal,headers:{...(token?{Authorization:`Bearer ${token}`}:{ }),...(body!==undefined?{'Content-Type':'application/json'}:{})},...(body!==undefined?{body:JSON.stringify(body)}:{})});
 const raw=await response.text(); let data; try{data=JSON.parse(raw)}catch{data=raw}
 if(!response.ok) throw new Error(data?.message||data?.title||(typeof data==='string'&&data.length<500&&data)||`Ошибка API (${response.status})`);
 return data;
}
export function list(data,key){
 const a=Array.isArray(data)?data:Array.isArray(data?.[key])?data[key]:[];
 if(key==='slots')return a.flatMap(s=>s.groomers?.map(g=>({time:s.time,groomerId:g.id,groomerName:g.name,available:true}))||[]);
 if(key==='appointments' && Array.isArray(data?.days))return data.days.flatMap(d=>[...d.schedules.map(s=>({...s,id:`shift-${d.date}-${s.id}`,date:d.date,kind:'shift'})),...d.timeOffs.map(s=>({...s,id:`off-${d.date}-${s.id}`,date:d.date,kind:'leave'})),...d.appointments.map(s=>({...s,date:d.date}))]).map(normalize);
 return a.map(normalize);
}
function normalize(a){return {...a,date:a.date||a.startDatetime?.slice(0,10),time:a.time||a.startDatetime?.slice(11,16),clientName:a.clientName||a.customerName,clientPhone:a.clientPhone||a.customerPhone,comment:a.comment??a.notes};}
