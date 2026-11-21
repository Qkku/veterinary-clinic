// Adapter for the original backend; no booking decisions are made here.
const datePart = value => String(value ?? '').slice(0,10);
const clock = value => String(value ?? '').slice(11,16);
const occupancy = new Set(['booked','confirmed','completed','no_show']);
function intervalForDay(record,date,nextDate){
 const start=String(record.startDatetime??''),end=String(record.endDatetime??'');
 if(!start || !end || datePart(start)>date || datePart(end)<date || (datePart(end)===date && clock(end)==='00:00'))return null;
 return {startTime:datePart(start)<date?'00:00':clock(start),endTime:datePart(end)>=nextDate?'24:00':clock(end)};
}
function followingDay(date){const d=new Date(date+'T12:00:00Z');d.setUTCDate(d.getUTCDate()+1);return d.toISOString().slice(0,10);}
export function scheduleRows(days, masters, appointments, absences){
 return masters.flatMap(({id,name,schedules})=>days.flatMap((date,index)=>{
  const base={groomerId:id,groomerName:name,date};
  const next=followingDay(date);
  const shifts=schedules.filter(s=>s.isActive!==false && s.isActive!==0 && Number(s.dayOfWeek)===index+1 && datePart(s.validFrom)<=date && (!s.validTo||datePart(s.validTo)>=date)).map(s=>({...s,...base,kind:'shift'}));
  const intervals=(records,kind)=>records.filter(r=>String(r.groomerId)===String(id)).flatMap(r=>{const span=intervalForDay(r,date,next);return span?[{...r,...base,...span,kind}]:[];});
  return [...shifts,...intervals(appointments.filter(a=>occupancy.has(String(a.status).toLowerCase())),'appointment'),...intervals(absences,'leave')];
 }));
}
