import React,{useEffect,useState} from 'react';
import {request,routes} from './api';
import {weekDays,buildSlots} from './calendar-utils';
import {scheduleRows} from './admin-schedule-data';
export default function AdminCalendar({token,groomers,version,onlyId}){
 const [filters,setFilters]=useState({free:false,booked:true,leave:true});
 const toggle=kind=>setFilters(prev=>({...prev,[kind]:!prev[kind]}));
 const [anchor,setAnchor]=useState(()=>new Date());
 const days=weekDays(anchor), key=days[0];
 const [state,setState]=useState({key:'',rows:[],error:'',loading:true});
 const masterKey=JSON.stringify(groomers.filter(g=>onlyId==null||String(g.id)===String(onlyId)).map(g=>({id:g.id,name:g.name})));
 useEffect(()=>{
  const abort=new AbortController(); const selected=JSON.parse(masterKey);
  setState({key,rows:[],error:'',loading:true});
  const get=async path=>{const data=await request(path,token,'GET',undefined,abort.signal);if(!Array.isArray(data))throw new Error(`Неожиданный формат ответа: ${path}`);return data;};
  const load=async()=>{
   if(!selected.length)return [];
   const [appointments,absences,masters]=await Promise.all([
    get(routes.adminAppointments),get('GroomerTimeOff'),
    Promise.all(selected.map(async g=>({...g,schedules:await get(`${routes.adminSchedule}/${encodeURIComponent(g.id)}`)})))
   ]);
   return scheduleRows(days,masters,appointments,absences);
  };
  load().then(rows=>{if(!abort.signal.aborted)setState({key,rows,error:'',loading:false});}).catch(e=>{if(!abort.signal.aborted)setState({key,rows:[],error:e.message,loading:false});});
  return()=>abort.abort();
 },[key,token,version,masterKey]);
 const move=n=>setAnchor(prev=>{const d=new Date(prev);d.setDate(d.getDate()+n*7);return d;});
 const fmt=date=>new Date(date+'T12:00:00').toLocaleDateString('ru-RU',{weekday:'short',day:'2-digit',month:'2-digit',year:'2-digit'});
 const time=value=>String(value??'').slice(11,16);
 const isPastAppointment=a=>String(a.status).toLowerCase()==='completed'||new Date(a.endDatetime)<new Date();
 const statusText=status=>({booked:'Запланирована',confirmed:'Подтверждена',completed:'Завершена',no_show:'Неявка'})[String(status).toLowerCase()]||status;
 const masters=new Map(JSON.parse(masterKey).map(g=>[String(g.id),g.name]));state.rows.filter(r=>onlyId==null||String(r.groomerId)===String(onlyId)).forEach(r=>masters.set(String(r.groomerId),r.groomerName||masters.get(String(r.groomerId))));
 return <div className="admin-calendar"><div className="week-toolbar"><button type="button" className="btn btn-outline" aria-label="Предыдущая неделя" onClick={()=>move(-1)}>‹</button><strong>{fmt(days[0])} — {fmt(days[6])}</strong><button type="button" className="btn btn-outline" aria-label="Следующая неделя" onClick={()=>move(1)}>›</button><button type="button" className="btn btn-outline" onClick={()=>setAnchor(new Date())}>Сегодня</button></div><div className="calendar-legend admin-calendar-filters" aria-label="Фильтры расписания">{[['free','Свободно','slot-free'],['booked','Занято','slot-booked'],['leave','Отпуск / отсутствие','slot-off']].map(([kind,label,style])=><button key={kind} type="button" className={'calendar-filter'+(filters[kind]?' selected':'')} aria-pressed={filters[kind]} onClick={()=>toggle(kind)}><i className={style}/>{label}</button>)}<span className="calendar-past-legend"><i className="slot-past"/>Прошедшая запись</span><button type="button" className="calendar-filter" onClick={()=>setFilters({free:true,booked:true,leave:true})}>Все</button></div>{state.loading||state.key!==key?<p>Загрузка расписания…</p>:state.error?<p role="alert" className="field-error">{state.error}</p>:!masters.size?<p>Нет мастеров.</p>:<div className="week-scroll">{[...masters].map(([id,name])=><section className="master-week" key={id}><h3>{name}</h3><div className="week-grid">{days.map(date=>{const rows=state.rows.filter(r=>String(r.groomerId)===id&&r.date===date);const slots=buildSlots(rows);const freeSlots=filters.free?slots.filter(s=>!s.booked):[];const appointments=filters.booked?rows.filter(r=>r.kind==='appointment'):[];const events=[...freeSlots.map(s=>({type:'free',start:s.start,label:s.label})),...appointments.map(a=>({type:'appointment',start:Number(time(a.startDatetime).slice(0,2))*60+Number(time(a.startDatetime).slice(3,5)),row:a}))].sort((a,b)=>a.start-b.start);const leaves=rows.filter(r=>r.kind==='leave');const shifts=rows.filter(r=>r.kind==='shift');return <div className="week-day" key={date}><strong>{fmt(date)}</strong>{events.map((event,i)=>{if(event.type==='free')return <div key={'free-'+event.start} className="calendar-slot slot-free" title="Свободно">{event.label}</div>;const a=event.row;const past=isPastAppointment(a);const start=time(a.startDatetime)||a.startTime;const end=time(a.endDatetime)||a.endTime;const client=a.customerName||a.clientName||a.client?.name;const pet=a.petName||a.pet?.name;const service=a.serviceName||a.service?.name;return <div key={'appointment-'+a.id+'-'+i} className={'calendar-slot '+(past?'slot-past':'slot-booked')+' appointment-slot'} title={[statusText(a.status),client,pet,service].filter(Boolean).join(' · ')}><b>{start}–{end}</b>{(client||pet||service)&&<small>{[client,pet,service].filter(Boolean).join(' · ')}</small>}</div>})}{filters.leave&&leaves.map((r,i)=><small key={i} className="leave-note admin-leave-block">{r.type==='vacation'?'Отпуск':r.type==='sick_leave'?'Больничный':'Отсутствие'}<br/>{r.startTime}–{r.endTime}</small>)}{!shifts.length&&!leaves.length&&!events.length&&<small className="leave-note">Выходной</small>}{shifts.length>0&&!events.length&&!(filters.leave&&leaves.length>0)&&<small className="leave-note">Нет записей</small>}</div>})}</div></section>)}</div>}</div>
}
