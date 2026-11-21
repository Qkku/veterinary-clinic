import AdminReviewPaws from './review-paws';
import {PhotoField,uploadPhoto,photoUrl} from './photo-field';
import React,{useState,useEffect,useId} from 'react';
import {request,routes,list} from './api';
import AdminCalendar from './admin-calendar';
import StatusLabel from './status-label';
export const today=()=>localDate(new Date());
function localDate(d){return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`;}
export function limitDate(){const d=new Date(),day=d.getDate();d.setDate(1);d.setMonth(d.getMonth()+3);const last=new Date(d.getFullYear(),d.getMonth()+1,0).getDate();d.setDate(Math.min(day,last));return localDate(d);}
export function useList(path,token,key,version=0){const [state,set]=useState({items:[],loading:true,error:''});useEffect(()=>{const c=new AbortController();set({path,items:[],loading:!!path,error:''});if(path)request(path,token,'GET',undefined,c.signal).then(d=>{if(!c.signal.aborted)set({path,items:list(d,key),loading:false,error:''});}).catch(e=>{if(!c.signal.aborted)set({path,items:[],loading:false,error:e.message});});return()=>c.abort();},[path,token,key,version]);return state;}
function Feedback({state}){return <>{state.loading&&<p className="hint">Загрузка…</p>}{state.error&&<p className="field-error" role="alert">{state.error}</p>}{!state.loading&&!state.error&&!state.items.length&&<p className="hint">Нет данных.</p>}</>}
export function roleOf(user){const r=String(user?.role||'').toLowerCase();return user?.isAdmin||r==='admin'||r==='administrator'?'admin':user?.isGroomer||r==='groomer'?'groomer':'client';}
const breeds={dog:['Лабрадор-ретривер','Золотистый ретривер','Шпиц','Йоркширский терьер','Пудель','Мальтипу','Ши-тцу','Корги','Чихуахуа','Такса','Хаски','Самоед','Бигль','Французский бульдог','Немецкая овчарка','Бишон фризе','Метис','Дворняга'],cat:['Британская короткошёрстная','Шотландская вислоухая','Мейн-кун','Сфинкс','Сибирская','Персидская','Бенгальская','Рэгдолл','Сиамская','Абиссинская','Метис','Беспородная']};
function BreedField({type,value,onChange}){
  const id=useId();
  const [open,setOpen]=useState(false),[active,setActive]=useState(-1);
  const options=(breeds[type]||[...new Set([...breeds.dog,...breeds.cat])]).filter(b=>b.toLocaleLowerCase('ru').startsWith(value.trim().toLocaleLowerCase('ru')));
  const shown=open&&options.length>0;
  function choose(b){onChange(b);setOpen(false);setActive(-1);}
  function keys(e){
    if(e.key==='Escape'){if(open)e.preventDefault();setOpen(false);setActive(-1);}
    if(e.key==='ArrowDown'||e.key==='ArrowUp'){
      if(!options.length)return;
      e.preventDefault();setOpen(true);
      setActive(i=>e.key==='ArrowDown'?(i+1)%options.length:(i<=0?options.length-1:i-1));
    }
    if(e.key==='Enter'&&shown&&active>=0&&active<options.length){e.preventDefault();choose(options[active]);}
  }
  return <div className="breed-field" onBlur={e=>{if(!e.currentTarget.contains(e.relatedTarget)){setOpen(false);setActive(-1);}}}>
    <label htmlFor={id}>Порода</label>
    <div className="breed-control">
      <input id={id} required maxLength={100} autoComplete="off" role="combobox" aria-autocomplete="list" aria-expanded={shown} aria-controls={id+'-options'} aria-activedescendant={shown&&active>=0?`${id}-${active}`:undefined} placeholder="Начните вводить или укажите свою" value={value} onFocus={()=>setOpen(true)} onClick={()=>setOpen(true)} onChange={e=>{onChange(e.target.value);setOpen(true);setActive(-1);}} onKeyDown={keys}/>
      <button className="breed-toggle" type="button" aria-label="Показать породы" aria-expanded={shown} onClick={()=>{setOpen(v=>!v);setActive(-1);}}><svg width="14" height="14" viewBox="0 0 16 16" fill="none" aria-hidden="true"><path d="m4 6 4 4 4-4" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"/></svg></button>
      {shown&&<div className="breed-options" id={id+'-options'} role="listbox">{options.map((b,i)=><div key={b} id={`${id}-${i}`} role="option" aria-selected={i===active} className={i===active?'breed-option active':'breed-option'} onMouseDown={e=>e.preventDefault()} onClick={()=>choose(b)}>{b}</div>)}</div>}
    </div>
  </div>
}
const emptyPet={name:'',type:'dog',breed:'',age:'',comment:''};
function PetForm({save,pet,onClose}){
  const [photo,setPhoto]=useState(null);
  const [p,set]=useState(()=>pet?{name:pet.name||'',type:pet.type||'unknown',breed:pet.breed||'',age:pet.age??'',comment:pet.notes??pet.comment??''}:emptyPet);
  const f=(k,v)=>set(x=>({...x,[k]:v}));
  return <form className="pet-editor" onSubmit={async e=>{
    e.preventDefault();
    const body={name:p.name.trim(),type:p.type,notes:p.comment.trim(),breed:p.breed.trim(),age:p.age===''?null:Number(p.age),...(pet?{size:pet.size??null}:{})};
    if(await save(pet?`${routes.pets}/${pet.id}`:routes.pets,pet?'PUT':'POST',body,photo,pet?.id))onClose();
  }}>
    <h3>{pet?'Редактирование питомца':'Новый питомец'}</h3>
    <label>Кличка<input required maxLength={100} value={p.name} onChange={e=>f('name',e.target.value)}/></label>
    <label>Вид<select value={p.type} onChange={e=>set({...p,type:e.target.value,breed:''})}>{p.type==='unknown'&&<option value="unknown">Не указан</option>}<option value="dog">Собака</option><option value="cat">Кошка</option></select></label>
    <BreedField type={p.type} value={p.breed} onChange={v=>f('breed',v)}/>
    <label>Возраст (лет)<input type="number" min="0" max="40" step="1" value={p.age} onChange={e=>f('age',e.target.value)}/></label>
    <label>Особенности питомца<textarea maxLength={1000} rows={4} placeholder="Повадки, аллергии, точный возраст, особенности породы" value={p.comment} onChange={e=>f('comment',e.target.value)}/></label>
    <PhotoField current={pet?.imageUrl} file={photo} onChange={setPhoto} hint="Добавьте фотографию своего шерстяного друга"/>
    <div className="pet-actions"><button type="button" className="pet-link" onClick={onClose}>Отмена</button><button className="btn btn-dark" disabled={!p.name.trim()||!p.breed.trim()}>{pet?'Сохранить':'Добавить питомца'}</button></div>
  </form>
}
function ClientPets({state,save}){
  const [editor,E]=useState(null);
  return <div className="client-pets">
    {state.loading?<p className="hint">Загрузка…</p>:state.error?<p className="field-error" role="alert">{state.error}</p>:!state.items.length?<p className="hint">У вас пока нет питомцев.</p>:null}
    {state.items.map(p=><article className="pet-card" key={p.id}>
      <b>{p.name}</b><p>{[p.type==='dog'?'Собака':p.type==='cat'?'Кошка':'',p.breed,p.age!=null?`Возраст: ${p.age} лет`:'Возраст не указан'].filter(Boolean).join(' · ')}</p>
      {(p.notes||p.comment)&&<p>{p.notes||p.comment}</p>}
      {p.imageUrl&&<img className="photo-preview" src={photoUrl(p.imageUrl)} alt={`Фото ${p.name}`}/>}
      <div className="pet-actions"><button type="button" className="pet-link" onClick={()=>E({mode:'edit',pet:p})}>Редактировать</button><button type="button" className="pet-link" onClick={async()=>{if(window.confirm(`Удалить питомца «${p.name}»? Связанные записи и история посещений также могут быть удалены.`)){if(await save(`${routes.pets}/${p.id}`,'DELETE')&&editor?.pet?.id===p.id)E(null);}}}>Удалить</button></div>
      {editor?.mode==='edit'&&editor.pet.id===p.id&&<PetForm key={`edit-${p.id}`} pet={p} save={save} onClose={()=>E(null)}/>}
    </article>)}
    {!state.loading&&!state.error&&<button type="button" className="pet-add" aria-label="Добавить питомца" aria-expanded={editor?.mode==='add'} onClick={()=>E(editor?.mode==='add'?null:{mode:'add'})}>+</button>}
    {editor?.mode==='add'&&<PetForm key="add" save={save} onClose={()=>E(null)}/>}
  </div>
}
export function Booking({token,services,groomers,initialService,initialMaster,onSuccess,onCabinet}){
 const [mode,setMode]=useState(initialMaster?'master':'time');
 const [service,setService]=useState(String(initialService||'')),[master,setMaster]=useState(String(initialMaster||''));
 const [date,setDate]=useState(initialMaster?'':today()),[week,setWeek]=useState(today),[time,setTime]=useState('');
 const [pet,setPet]=useState(''),[comment,setComment]=useState(''),[version,refresh]=useState(0),[busy,setBusy]=useState(false),[error,setError]=useState('');
 const [days,setDays]=useState({key:'',items:[],errors:0,loading:false});
 const queryFor=(d,g)=>`${routes.slots}?${new URLSearchParams({serviceId:service,date:d,...(g?{groomerId:g}:{})})}`;
 const query=service&&date&&date>=today()&&date<=limitDate()&&(mode==='time'||master)?queryFor(date,mode==='master'?master:''):null;
 const slots=useList(query,token,'slots',version),pets=useList(routes.myPets,token,'pets');
 const available=(items,d)=>items.filter(s=>s.available!==false&&typeof s.time==='string'&&s.groomerId!=null&&new Date(`${d}T${s.time.slice(0,5)}:00`)>new Date());
 const dayKey=mode==='master'&&service&&master?`${service}/${master}/${week}/${version}`:'';
 useEffect(()=>{
  if(!dayKey)return;
  const c=new AbortController();setDays({key:dayKey,items:[],errors:0,loading:true});
  const dates=Array.from({length:7},(_,i)=>shiftBookingDate(week,i)).filter(d=>d>=today()&&d<=limitDate());
  Promise.all(dates.map(async d=>{try{const data=await request(queryFor(d,master),token,'GET',undefined,c.signal);return {date:d,count:available(list(data,'slots'),d).filter(s=>String(s.groomerId)===master).length};}catch{return {date:d,error:true};}})).then(items=>{if(!c.signal.aborted)setDays({key:dayKey,items,errors:items.filter(x=>x.error).length,loading:false});});
  return()=>c.abort();
 },[dayKey,token]);
 const valid=slots.path===query&&!slots.loading?available(slots.items,date).filter(s=>mode==='time'||String(s.groomerId)===master):[];
 const times=[...new Set(valid.map(s=>s.time))].sort(),masters=valid.filter(s=>s.time===time);
 const canBook=!!(query&&pet&&time&&master&&masters.some(s=>String(s.groomerId)===master));
 function changeMode(m){if(m===mode)return;setMode(m);setMaster('');setTime('');setDate(m==='time'?today():'');setWeek(today());setError('');}
 function changeService(s){setService(s);setTime('');setError('');if(mode==='time')setMaster('');else setDate('');}
 function changeWeek(n){setWeek(shiftBookingDate(week,n));setDate('');setTime('');setError('');}
 async function book(e){e.preventDefault();if(busy||!canBook)return;setBusy(true);setError('');try{const fresh=available(list(await request(query,token),'slots'),date);if(!fresh.some(s=>s.time===time&&String(s.groomerId)===master))throw new Error('Выбранное время больше недоступно.');await request(routes.appointments,token,'POST',{petId:Number(pet),serviceId:Number(service),groomerId:Number(master),startDatetime:`${date}T${time.slice(0,5)}:00`,comment:comment.trim()});onSuccess();}catch(e){setError(e.message);setTime('');if(mode==='time')setMaster('');refresh(v=>v+1);}finally{setBusy(false);}}
 const masterSelect=(availableMasters,disabled,step)=> <label>{step}. Мастер<select required value={master} disabled={disabled} onChange={e=>{setMaster(e.target.value);setError('');if(mode==='master'){setDate('');setTime('');setWeek(today());}}}><option value="">Выберите мастера</option>{availableMasters.map(g=><option key={g.id} value={g.id}>{g.name}</option>)}</select></label>;
 return <section className="section wrap page-section"><h1 className="page-title">Онлайн-<em>запись</em></h1><div className="booking-grid"><form className="panel" onSubmit={book}><fieldset disabled={busy} className="booking-fields">
 <h3>Как вам удобнее записаться?</h3><div className="booking-modes">{[['time','По времени','Сначала дата и время'],['master','По мастеру','Сначала любимый мастер']].map(([m,title,desc])=><button key={m} type="button" aria-pressed={mode===m} className={'booking-mode '+(mode===m?'selected':'')} onClick={()=>changeMode(m)}><strong>{title}</strong><small>{desc}</small></button>)}</div>
 <label>1. Услуга<select required value={service} onChange={e=>changeService(e.target.value)}><option value="">Выберите услугу</option>{services.map(s=><option key={s.id} value={s.id}>{s.name} · {s.duration_minutes} мин</option>)}</select></label>
 {mode==='master'?<>{masterSelect(groomers,!service,2)}<h3>3. Свободные даты</h3>{!dayKey?<p className="hint">Выберите услугу и мастера.</p>:<><div className="booking-week"><button type="button" className="btn btn-outline" disabled={week<=today()} onClick={()=>changeWeek(-7)}>Назад</button><span>{week.slice(8)}.{week.slice(5,7)} — {shiftBookingDate(week,6).slice(8)}.{shiftBookingDate(week,6).slice(5,7)}</span><button type="button" className="btn btn-outline" disabled={shiftBookingDate(week,7)>limitDate()} onClick={()=>changeWeek(7)}>Далее</button></div>{days.key!==dayKey||days.loading?<p className="hint">Проверяем свободные дни…</p>:<><div className="booking-days">{days.items.map(d=><button type="button" key={d.date} disabled={!d.count||d.error} aria-pressed={date===d.date} className={'time '+(date===d.date?'selected':'')} onClick={()=>{setDate(d.date);setTime('');setError('');}}>{new Date(d.date+'T12:00:00').toLocaleDateString('ru-RU',{weekday:'short',day:'numeric',month:'short'})}<small>{d.error?'Ошибка':d.count?'Есть время':'Нет мест'}</small></button>)}</div>{days.errors>0&&<p className="field-error">Не удалось проверить некоторые дни. <button type="button" className="subtle-link" onClick={()=>refresh(v=>v+1)}>Повторить</button></p>}{!days.errors&&!days.items.some(d=>d.count)&&<p className="hint">На этой неделе нет свободного времени. Посмотрите следующую неделю.</p>}</>}</>}</>:<label>2. Дата<input required type="date" min={today()} max={limitDate()} value={date} onChange={e=>{setDate(e.target.value);setTime('');setMaster('');setError('');}}/></label>}
 <p className="hint">Запись доступна на три месяца вперёд.</p><h3>{mode==='master'?4:3}. Свободное время</h3>{!query?<p className="hint">Выберите услугу и дату.</p>:slots.path!==query||slots.loading?<p className="hint">Загрузка…</p>:slots.error?<p className="field-error">{slots.error}</p>:times.length?<div className="time-slots">{times.map(t=><button key={t} type="button" className={'time '+(t===time?'selected':'')} onClick={()=>{setTime(t);if(mode==='time')setMaster('');setError('');}}>{t.slice(0,5)}</button>)}</div>:<p className="hint">На эту дату нет свободного времени.</p>}
 {mode==='time'&&masterSelect(masters.map(s=>({id:s.groomerId,name:s.groomerName||groomers.find(g=>String(g.id)===String(s.groomerId))?.name||`Мастер ${s.groomerId}`})),!time,4)}
 <label>5. Питомец<select required value={pet} onChange={e=>setPet(e.target.value)}><option value="">Выберите питомца</option>{pets.items.map(p=><option key={p.id} value={p.id}>{p.name} — {p.breed}</option>)}</select></label><Feedback state={pets}/><button type="button" className="subtle-link" onClick={onCabinet}>Добавить питомца в кабинете</button><label>Комментарий к записи<textarea rows={4} maxLength={1000} value={comment} onChange={e=>setComment(e.target.value)} placeholder="Что мастеру нужно знать о питомце?"/></label>{error&&<p role="alert" className="field-error">{error}</p>}<button className="btn btn-dark full" disabled={busy||!canBook}>{busy?'Проверяем время…':'Подтвердить запись'}</button></fieldset></form><aside className="panel order-panel"><h2>Ваша запись</h2><p>{services.find(s=>String(s.id)===service)?.name||'Услуга не выбрана'}</p><p>{groomers.find(g=>String(g.id)===master)?.name||'Мастер не выбран'}</p><p>{date||'Дата не выбрана'} {time.slice(0,5)}</p><p className="hint">Свободные интервалы получаем от сервера с учётом длительности услуги, графика, отпусков и записей.</p></aside></div></section>;
}
function shiftBookingDate(date,n){const d=new Date(date+'T12:00:00');d.setDate(d.getDate()+n);return localDate(d);}
function Appointment({a,children}){return <article className="appointment"><b>{a.serviceName||a.service?.name||'Услуга'}</b><p>{a.date} {a.time||a.startTime} <StatusLabel value={a.status}/></p>{a.cancellationReason&&<p>Причина отмены: {a.cancellationReason}</p>}<p>{a.petName||a.pet?.name} {a.breed||a.pet?.breed} {(a.petType||a.pet?.type)==='dog'?'Собака':(a.petType||a.pet?.type)==='cat'?'Кошка':''} {a.petAge??a.pet?.age??''}</p><p>{a.clientName||a.client?.name} {a.clientPhone||a.client?.phone} {a.groomerName}</p>{(a.comment||a.pet?.notes)&&<p>Особенности: {a.pet?.notes} {a.comment}</p>}{children}</article>}
function LeaveForm({save}){const minDate=limitDate();const [from,F]=useState(minDate),[to,T]=useState(minDate),[reason,R]=useState('');return <form onSubmit={e=>{e.preventDefault();if(from<minDate||to<from||!reason.trim())return;save(routes.leaves,'POST',{startDate:from,endDate:to,reason:reason.trim()});}}><label>С<input type="date" required min={minDate} value={from} onChange={e=>{F(e.target.value);if(to<e.target.value)T(e.target.value);}}/></label><label>По<input type="date" required min={from} value={to} onChange={e=>T(e.target.value)}/></label><label>Причина<textarea required maxLength={500} value={reason} onChange={e=>R(e.target.value)}/></label><button className="btn btn-dark" disabled={from<minDate||to<from||!reason.trim()}>Отправить заявку</button></form>}
function ReviewPaws({rating}){return <div className="client-review-paws client-review-readonly" role="img" aria-label={`Оценка ${rating} из 5`}>{[1,2,3,4,5].map(n=><span key={n} className={n<=Number(rating)?"selected":""}><svg viewBox="0 0 32 32" width="30" height="30" aria-hidden="true" fill="currentColor"><ellipse cx="6" cy="13" rx="3.5" ry="5" transform="rotate(-24 6 13)"/><ellipse cx="12.5" cy="7.5" rx="3.5" ry="5" transform="rotate(-8 12.5 7.5)"/><ellipse cx="20" cy="7.5" rx="3.5" ry="5" transform="rotate(8 20 7.5)"/><ellipse cx="26" cy="13" rx="3.5" ry="5" transform="rotate(24 26 13)"/><path d="M16 14.5c-4 0-5.2 3.7-8.5 6.2C2 25 6.5 30 11 28.5c2-.7 3.4-1.5 5-1.5s3 .8 5 1.5C25.5 30 30 25 24.5 20.7 21.2 18.2 20 14.5 16 14.5Z"/></svg></span>)}</div>}
function ReviewForm({a,save}){
  const [text,T]=useState(''),[rating,R]=useState(5);
  return <form onSubmit={e=>{e.preventDefault();save(routes.reviews,'POST',{appointmentId:a.id,rating:Number(rating),text:text});}}>
    <div className="client-review-rating" role="group" aria-label="Оценка от 1 до 5 лапок">
      <span>Оценка</span>
      <div className="client-review-paws">{[1,2,3,4,5].map(n=><button key={n} type="button" className={'client-review-paw '+(n<=rating?'selected':'')} aria-label={`Оценка ${n} из 5`} aria-pressed={n===rating} onClick={()=>R(n)}>
        <svg viewBox="0 0 32 32" width="30" height="30" aria-hidden="true" fill="currentColor"><ellipse cx="6" cy="13" rx="3.5" ry="5" transform="rotate(-24 6 13)"/><ellipse cx="12.5" cy="7.5" rx="3.5" ry="5" transform="rotate(-8 12.5 7.5)"/><ellipse cx="20" cy="7.5" rx="3.5" ry="5" transform="rotate(8 20 7.5)"/><ellipse cx="26" cy="13" rx="3.5" ry="5" transform="rotate(24 26 13)"/><path d="M16 14.5c-4 0-5.2 3.7-8.5 6.2C2 25 6.5 30 11 28.5c2-.7 3.4-1.5 5-1.5s3 .8 5 1.5C25.5 30 30 25 24.5 20.7 21.2 18.2 20 14.5 16 14.5Z"/></svg>
      </button>)}</div>
    </div>
    <label>Отзыв<textarea required maxLength={1500} placeholder="Только честный пушистый отзыв" value={text} onChange={e=>T(e.target.value)} className="client-review-text"/></label>
    <button className="btn btn-outline" disabled={!text.trim()}>Отправить</button>
  </form>
}
export function Cabinet({user,token,logout,onBook,groomers=[]}){const role=roleOf(user);const tabs=role==='admin'?['Профиль','Записи','Отзывы','Отпуска','Расписание']:role==='groomer'?['Профиль','Расписание','Отпуск']:['Профиль','Питомцы','Записи','История','Отзывы'];const [tab,setTab]=useState('Профиль'),[date,D]=useState(today),[version,V]=useState(0),[message,M]=useState(''),[busy,B]=useState(false);let path=null,key='items';if(role==='client'){if(tab==='Питомцы'){path=routes.myPets;key='pets';}if(['Записи','История'].includes(tab)){path=routes.myAppointments;key='appointments';}if(tab==='Отзывы'){path=routes.myReviews;key='reviews';}}else if(role==='groomer'){if(tab==='Расписание'){path=`${routes.groomerSchedule}?date=${date}`;key='appointments';}if(tab==='Отпуск'){path=routes.myLeaves;key='requests';}}else{if(tab==='Записи'){path=routes.adminAppointments;key='appointments';}if(tab==='Отзывы'){path=routes.moderation;key='reviews';}if(tab==='Отпуска'){path=routes.leaves;key='requests';}}const state=useList(path,token,key,version);async function save(path,method,body,photo,entityId){if(busy)return false;B(true);M('');try{const result=await request(path,token,method,body);if(photo){try{await uploadPhoto('pets',entityId??result?.id,photo,token);}catch(e){M('Данные сохранены, но фото не загружено: '+e.message+'. Откройте редактирование для повторной загрузки.');V(v=>v+1);return true;}}M('Изменения сохранены.');V(v=>v+1);return true;}catch(e){M(e.message);return false;}finally{B(false);}}const done=a=>['completed','cancelled','canceled','no_show'].includes(String(a.status).toLowerCase())||(a.date<today()||new Date(a.startDatetime||`${a.date}T${a.time||'23:59'}`)<new Date());const rows=state.items.filter(a=>role!=='client'||!['Записи','История'].includes(tab)||(tab==='История'?done(a):!done(a)));return <div className="dashboard"><nav className="panel dashboard-nav">{tabs.map(t=><button key={t} className={'btn '+(tab===t?'btn-dark':'btn-outline')} onClick={()=>{setTab(t);M('');}}>{t}</button>)}<button className="subtle-link" onClick={logout}>Выйти</button></nav><section className="panel">{role==='client'&&tab==='Записи'?<div className="client-appointments-heading"><h2>{tab}</h2><button type="button" className="btn btn-dark client-appointment-add" onClick={onBook} aria-label="Новая запись" title="Новая запись">+</button></div>:<h2>{tab}</h2>}{message&&<p role="status" className="form-message">{message}</p>}<fieldset disabled={busy} className="plain-fieldset">{tab==='Профиль'?<><p><b>{user.name}</b></p><p>{user.email}</p><p>{user.phone||'Телефон не указан'}</p><p>Роль: {role==='admin'?'Администратор':role==='groomer'?'Грумер':'Клиент'}</p></>:role==='admin'&&tab==='Расписание'?<AdminCalendar token={token} groomers={groomers} version={version}/>:tab==='Питомцы'?<ClientPets state={state} save={save}/>:<>{tab==='Расписание'&&<label>Дата<input type="date" value={date} onChange={e=>D(e.target.value)}/></label>}{tab==='Отпуск'&&<LeaveForm save={save}/ >}<Feedback state={state}/>{rows.map(a=><div key={a.id??`${a.groomerId}-${a.date}-${a.time}`} className="record">{tab==='Питомцы'?<><b>{a.name}</b><p>{a.type==='dog'?'Собака':a.type==='cat'?'Кошка':'Вид не указан'} · {a.breed} · {a.age??'Возраст не указан'}</p><p>{a.comment}</p></>:tab==='Отзывы'?<>{role==='client'?<>{(a.groomerName||a.groomer?.name)&&<b>Мастер: {a.groomerName||a.groomer?.name}</b>}<ReviewPaws rating={a.rating}/><p className="client-review-copy">{a.comment||a.text}</p>{a.createdAt&&<small>{new Date(a.createdAt).toLocaleDateString('ru-RU')}</small>}</>:<><AdminReviewPaws rating={a.rating}/><p>{a.comment||a.text}</p><p><StatusLabel value={a.status} context="review"/></p></>}{role==='admin'&&<div className="hero-actions"><button className="btn btn-dark" onClick={()=>save(`${routes.reviews}/${a.id}/moderate`,'PATCH',{status:'approved'})}>Опубликовать</button><button className="btn btn-outline" onClick={()=>save(`${routes.reviews}/${a.id}/moderate`,'PATCH',{status:'rejected'})}>Отклонить</button></div>}</>:['Отпуска','Отпуск'].includes(tab)?<><b>{a.groomerName}</b><p>{a.startDate?.slice(0,10)} — {a.endDate?.slice(0,10)}</p><p>{a.reason} <StatusLabel value={a.status}/></p>{role==='admin'&&String(a.status).toLowerCase()==='pending'&&<div className="hero-actions"><button className="btn btn-dark" onClick={()=>save(`${routes.leaves}/${a.id}/decision`,'PATCH',{status:'approved'})}>Подтвердить отпуск</button><button className="btn btn-outline" onClick={()=>save(`${routes.leaves}/${a.id}/decision`,'PATCH',{status:'rejected'})}>Отклонить</button></div>}</>:role==='admin'&&tab==='Расписание'?<><b>{a.groomerName}</b><p>{a.startTime} — {a.endTime} · {a.kind==='leave'?'Отпуск / отсутствие':a.kind==='dayOff'?'Выходной':a.kind==='appointment'?'Запись': 'Рабочее время'}</p></>:a.kind?<><b>{a.kind==='leave'?'Отсутствие':'Рабочее время'}</b><p>{a.date} {a.startTime||a.startDatetime?.slice(11,16)} — {a.endTime||a.endDatetime?.slice(11,16)}</p><p>{a.comment}</p></>:<Appointment a={a}>{((role==='client'&&tab==='Записи')||(role==='admin'&&tab==='Записи'))&&!done(a)&&<button className="cancel" onClick={()=>{if(window.confirm('Отменить запись?'))role==='admin'?save(`${routes.appointments}/${a.id}/status`,'PUT',{status:'cancelled'}):save(`${routes.appointments}/${a.id}`,'DELETE');}}>Отменить запись</button>}{role==='client'&&tab==='История'&&String(a.status).toLowerCase()==='completed'&&!a.hasReview&&<ReviewForm a={a} save={save}/>}</Appointment>}</div>)}</>}</fieldset></section></div>}
