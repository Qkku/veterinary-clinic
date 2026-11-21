import ReviewPaws from './review-paws';
import React,{useEffect,useState,useRef} from 'react';
import {API_URL} from './api';
import {dateKey,monthGrid,visibleAppointments,workingHours,clock} from './groomer-month';
async function api(path,token,method='GET',body,signal){const r=await fetch(`${API_URL}/${path}`,{method,signal,headers:{Authorization:`Bearer ${token}`,...(body!==undefined?{'Content-Type':'application/json'}:{})},...(body!==undefined?{body:JSON.stringify(body)}:{})});const raw=await r.text();let data;try{data=JSON.parse(raw)}catch{data=raw}if(!r.ok){const e=new Error(data?.message||data?.title||`Ошибка API (${r.status})`);e.status=r.status;throw e;}return data;}
function useData(path,token,version=0){const [s,S]=useState({loading:true,data:null,error:'',missing:false});useEffect(()=>{const c=new AbortController();S({loading:true,data:null,error:'',missing:false});api(path,token,'GET',undefined,c.signal).then(data=>{if(!c.signal.aborted)S({loading:false,data,error:'',missing:false});}).catch(e=>{if(!c.signal.aborted)S({loading:false,data:null,error:e.message,missing:e.status===404||e.status===405});});return()=>c.abort();},[path,token,version]);return s;}
function Status({s,missing}){return s.loading?<p>Загрузка…</p>:s.missing?<p className="hint">{missing}</p>:s.error?<p role="alert" className="field-error">{s.error}</p>:null;}
function Modal({title,close,children}){const ref=useRef(null);useEffect(()=>{ref.current.showModal();},[]);return <dialog ref={ref} className="admin-modal" onCancel={e=>{e.preventDefault();close();}}><div className="admin-heading"><h2>{title}</h2><button className="admin-icon" aria-label="Закрыть" onClick={close}>×</button></div>{children}</dialog>;}
const details=[['Имя','name'],['Email','email'],['Телефон','phone'],['Возраст','age'],['Образование','education'],['Стаж (лет)','experienceYears'],['Специализация','specialization']];
function Month({token}){const [month,M]=useState(()=>{const d=new Date();return new Date(d.getFullYear(),d.getMonth(),1)});const [state,S]=useState({loading:true,days:[],error:''});const [selected,D]=useState(null);const {cells}=monthGrid(month);const key=dateKey(month);useEffect(()=>{const c=new AbortController();S({loading:true,days:[],error:''});D(null);Promise.all(monthGrid(month).weeks.map(date=>api(`GroomerCabinet/schedule?date=${date}`,token,'GET',undefined,c.signal))).then(parts=>{if(!parts.every(p=>Array.isArray(p.days)))throw new Error('Сервер вернул некорректное расписание.');if(!c.signal.aborted)S({loading:false,days:parts.flatMap(p=>p.days),error:''});}).catch(e=>{if(!c.signal.aborted)S({loading:false,days:[],error:e.message});});return()=>c.abort();},[key,token]);const move=n=>M(new Date(month.getFullYear(),month.getMonth()+n,1));const day=state.days.find(d=>d.date===selected);return <><div className="groomer-month-toolbar"><button className="btn btn-outline" onClick={()=>move(-1)} aria-label="Предыдущий месяц">‹</button><h2>{month.toLocaleDateString('ru-RU',{month:'long',year:'numeric'})}</h2><button className="btn btn-outline" onClick={()=>move(1)} aria-label="Следующий месяц">›</button><button className="btn btn-outline" onClick={()=>{const d=new Date();M(new Date(d.getFullYear(),d.getMonth(),1));}}>Сегодня</button></div><div className="groomer-legend"><span><i className="groomer-booked"/>Активная запись</span><span><i className="groomer-completed"/>Завершённая запись</span><span><i/>Нерабочий день</span></div>{state.loading?<p>Загрузка расписания…</p>:state.error?<p role="alert" className="field-error">{state.error}</p>:<div className="groomer-month-scroll"><div className="groomer-month-grid">{['Пн','Вт','Ср','Чт','Пт','Сб','Вс'].map(d=><b className="groomer-weekday" key={d}>{d}</b>)}{cells.map((date,i)=>{if(!date)return <div key={`blank${i}`} className="groomer-day groomer-blank"/>;const d=state.days.find(x=>x.date===date);const hours=workingHours(d);const bookings=visibleAppointments(d);return <button type="button" key={date} className={'groomer-day '+(date===dateKey(new Date())?'groomer-today ':'')+(selected===date?'groomer-selected':'')} onClick={()=>D(date)} aria-label={`Расписание на ${date}`}><b>{Number(date.slice(-2))}</b>{hours.length>0?<small className="groomer-shift">Рабочий день<br/>{hours.map(h=><React.Fragment key={h}>{h}<br/></React.Fragment>)}</small>:d?.timeOffs?.length===0?<small className="groomer-shift">Выходной</small>:null}{d?.timeOffs?.map(o=><small key={o.id} className="groomer-absence">{o.type==='vacation'?'Отпуск':o.type==='sick_leave'?'Больничный':'Отсутствие'}</small>)}{bookings.map(a=><span key={a.id} className={(String(a.status).toLowerCase()==='completed'?'groomer-completed':'groomer-booked')+' groomer-booking'}><strong>{clock(a.startDatetime)} – {clock(a.endDatetime)}</strong><span>{a.service?.name||'Услуга'}</span></span>)}{hours.length>0&&!bookings.length&&<small className="groomer-empty">Нет записей</small>}</button>})}</div></div>}{selected&&day&&<Modal title={`Расписание на ${selected}`} close={()=>D(null)}><h3>Рабочие часы</h3>{day.schedules?.length?day.schedules.map(s=><p key={s.id}>{s.startTime} — {s.endTime}</p>):<p>Нерабочий день</p>}{day.timeOffs?.map(o=><p key={o.id}>Отсутствие: {o.type==='vacation'?'Отпуск':o.type==='sick_leave'?'Больничный':'Другое'} · {o.comment}</p>)}<h3>Записи клиентов</h3>{visibleAppointments(day).length?visibleAppointments(day).map(a=><article className={'admin-row '+(String(a.status).toLowerCase()==='completed'?'groomer-completed-detail':'')} key={a.id}><div><b>{clock(a.startDatetime)} — {clock(a.endDatetime)} · {a.service?.name}</b><p>Статус: {({booked:'Записан',confirmed:'Подтверждена',completed:'Завершена',cancelled:'Отменена',no_show:'Неявка'})[a.status]||a.status}</p><p>Клиент: {a.client?.name} · {a.client?.phone}</p><p>Питомец: {a.pet?.name} · {a.pet?.breed} · {a.pet?.age??'—'} лет</p>{a.pet?.notes&&<p>Особенности: {a.pet.notes}</p>}{a.comment&&<p>Комментарий: {a.comment}</p>}</div></article>):<p>Нет записей.</p>}</Modal>}</>;}
function Leave({token,user}) {
    const [v,V] = useState(0);
    const s = useData('LeaveRequests/my',token,v);
    const confirmed = useData('GroomerTimeOff',token,v);

    const minLeaveDate = new Date();
    const originalDay = minLeaveDate.getDate();
    minLeaveDate.setHours(0, 0, 0, 0);
    minLeaveDate.setDate(1);
    minLeaveDate.setMonth(minLeaveDate.getMonth() + 3);
    const lastDay = new Date(minLeaveDate.getFullYear(), minLeaveDate.getMonth() + 1, 0).getDate();
    minLeaveDate.setDate(Math.min(originalDay, lastDay));
    const minDate = dateKey(minLeaveDate);

    const [from,F] = useState(minDate);
    const [to,T] = useState(minDate);
    const [reason,R] = useState('');
    const [busy,B] = useState(false);
    const [msg,Q] = useState('');

    async function send(e) {
        e.preventDefault();
        if (from < minDate || to < from || !reason.trim()) return;
        B(true);
        Q('');
        try {
            await api('LeaveRequests',token,'POST',{
                startDate:from,
                endDate:to,
                reason:reason.trim()
            });
            Q('Заявка отправлена.');
            R('');
            V(x => x + 1);
        } catch(e) {
            Q(e.message);
        } finally {
            B(false);
        }
    }

    return <>
        <div className="panel">
            <h2>Заявка на отпуск</h2>
            <Status s={s} missing="API заявок на отпуск недоступен. Обратитесь к администратору."/>
            <form onSubmit={send}>
                <fieldset className="plain-fieldset" disabled={busy || s.loading || !!s.error}>
                    <label>Дата начала
                        <input type="date" min={minDate} required value={from}
                            onChange={e => {
                                F(e.target.value);
                                if (to < e.target.value) T(e.target.value);
                            }}/>
                    </label>
                    <label>Дата окончания
                        <input type="date" min={from} required value={to}
                            onChange={e => T(e.target.value)}/>
                    </label>
                    <label>Причина
                        <textarea required maxLength={500} value={reason}
                            onChange={e => R(e.target.value)}/>
                    </label>
                    <button className="btn btn-dark" disabled={!reason.trim() || from < minDate || to < from}>
                        Отправить заявку
                    </button>
                </fieldset>
            </form>
            {msg && <p role="status">{msg}</p>}
        </div>

        <h2>Мои заявки</h2>
        {Array.isArray(s.data) && (!s.data.length
            ? <p>Заявок нет.</p>
            : s.data.map(a => <article key={a.id} className="admin-row"><div>
                <b>{a.startDate?.slice(0,10)} — {a.endDate?.slice(0,10)}</b>
                <p>{a.reason}</p>
                <p>{({pending:'На рассмотрении',approved:'Подтверждена',rejected:'Отклонена'})[a.status] || a.status}</p>
            </div></article>))}

        <h2>Назначенные периоды отсутствия</h2>
        <Status s={confirmed} missing="Не удалось загрузить периоды отсутствия."/>
        {Array.isArray(confirmed.data) && confirmed.data
            .filter(x => String(x.groomerId) === String(user.id))
            .map(a => <article className="admin-row" key={a.id}><div>
                <b>{a.startDatetime?.slice(0,10)} — {a.endDatetime?.slice(0,10)}</b>
                <p>{a.type === 'vacation' ? 'Отпуск' : a.type === 'sick_leave' ? 'Больничный' : 'Другое'} · {a.comment}</p>
            </div></article>)}
        {Array.isArray(confirmed.data) && !confirmed.data.some(x => String(x.groomerId) === String(user.id)) && <p>Нет назначенных периодов.</p>}
    </>;
}
function Reviews({token}){const s=useData('GroomerCabinet/reviews',token);return <><Status s={s} missing="Список отзывов мастера сейчас недоступен. Попробуйте обновить страницу позже."/>{Array.isArray(s.data)&&(!s.data.length?<p>Отзывов пока нет.</p>:s.data.map(r=><article className="admin-row" key={r.id}><div><div className="review-rating-row"><ReviewPaws rating={r.rating}/></div><p>{r.userName||'Клиент'} · {r.createdAt?.slice(0,10)}</p><p className="admin-review-text">{r.text}</p></div></article>))}</>;}
export default function GroomerPanel({user,token,logout,onHome}){const [tab,T]=useState('Расписание'),[profile,P]=useState(false),[open,O]=useState(false),[notices,N]=useState([]),[error,E]=useState(''),[loading,L]=useState(true),[busy,B]=useState(false);const revision=useRef(0);useEffect(()=>{const c=new AbortController();async function load(){const seq=++revision.current;try{const data=await api('Notifications',token,'GET',undefined,c.signal);if(!Array.isArray(data))throw new Error('Некорректный ответ уведомлений.');if(!c.signal.aborted&&seq===revision.current){N(data);E('');}}catch(e){if(!c.signal.aborted&&seq===revision.current)E(e.message);}finally{if(!c.signal.aborted)L(false);}}load();const id=setInterval(load,30000);return()=>{c.abort();clearInterval(id)};},[token]);async function mark(id){B(true);try{await api(id==null?'Notifications/read-all':`Notifications/${id}/read`,token,'PUT');++revision.current;N(prev=>prev.map(n=>id==null||n.id===id?{...n,isRead:true}:n));E('');}catch(e){E(e.message);}finally{B(false);}}const count=notices.filter(n=>!n.isRead).length;return <div className="admin-shell groomer-shell"><header className="admin-top"><button className="admin-profile" aria-label="Профиль мастера" title="Профиль мастера" onClick={()=>P(true)}>{user.name?.trim().slice(0,1).toUpperCase()||'М'}</button><nav aria-label="Разделы мастера">{['Расписание','Отпуск','Мои отзывы'].map(t=><button key={t} className={tab===t?'active':''} onClick={()=>{T(t);O(false)}}>{t}</button>)}<button onClick={logout}>Выйти</button></nav><div className="groomer-notice-anchor"><button className={'groomer-bell '+(count?'unread':'')} title="Уведомления" aria-label={`Уведомления, непрочитанных ${count}`} aria-expanded={open} onClick={()=>O(x=>!x)}><svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4"/></svg>{count>0&&<span>{count>99?'99+':count}</span>}</button>{open&&<section className="groomer-notices"><div className="admin-heading"><h3>Уведомления</h3><button className="subtle-link" disabled={busy||!count} onClick={()=>mark(null)}>Прочитать все</button></div>{loading&&<p>Загрузка…</p>}{error&&<p role="alert" className="field-error">{error}</p>}{!loading&&!error&&!notices.length&&<p>Уведомлений нет.</p>}{notices.map(n=><article key={n.id} className={!n.isRead?'new-notice':''}><b>{n.title}</b><p>{n.message}</p><small>{n.createdAt?.slice(0,16).replace('T',' ')}</small>{!n.isRead&&<button className="subtle-link" disabled={busy} onClick={()=>mark(n.id)}>Отметить прочитанным</button>}</article>)}</section>}</div></header><section className="admin-content"><div className="admin-heading"><div><p className="eyebrow">КАБИНЕТ МАСТЕРА</p><h1>{tab}</h1></div></div>{tab==='Расписание'?<Month token={token}/>:tab==='Отпуск'?<Leave token={token} user={user}/>:<Reviews token={token}/>}</section>{profile&&<Modal title="Профиль мастера" close={()=>P(false)}><dl className="admin-info">{details.map(([name,key])=><React.Fragment key={key}><dt>{name}</dt><dd>{user[key]??'Не указано'}</dd></React.Fragment>)}<dt>Статус</dt><dd>{user.isActive===false?'Неактивен':'Активен'}</dd></dl></Modal>}</div>;}
