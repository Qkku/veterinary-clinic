import ReviewPaws from './review-paws';
import Portfolio from './portfolio';
import {PhotoField,uploadPhoto,photoUrl} from './photo-field';
import React,{useEffect,useState,useRef} from 'react';
import {request} from './api';
import AdminCalendar from './admin-calendar';
import {statistics} from './admin-stats';
const tabs=['Услуги','Мастера','Записи','Отзывы','Клиенты','Статистика'];
import StatusLabel,{canMarkNoShow} from './status-label';
const dateText=v=>v?String(v).slice(0,16).replace('T',' '):'—';
const active=a=>['booked','confirmed'].includes(a.status);
const paths={services:'Services',masters:'Groomers',appointments:'Appointments/all',reviews:'Reviews/all',users:'Users',leaves:'LeaveRequests'};
function Modal({title,close,busy,children,wide=false}){const ref=useRef(null);useEffect(()=>{ref.current?.showModal();},[]);return <dialog ref={ref} className={"admin-modal"+(wide?" admin-modal-calendar":"")} onCancel={e=>{e.preventDefault();if(!busy)close();}}><div className="admin-heading"><h2>{title}</h2><button type="button" className="admin-icon" aria-label="Закрыть" disabled={busy} onClick={close}>×</button></div>{children}</dialog>}
const Button=({children,onClick,danger=false,disabled=false})=><button type="button" className={'btn '+(danger?'admin-danger':'btn-outline')} disabled={disabled} onClick={onClick}>{children}</button>;
function Info({fields}){return <dl className="admin-info">{fields.map(([k,v])=><React.Fragment key={k}><dt>{k}</dt><dd>{v??'—'}</dd></React.Fragment>)}</dl>}
function Editor({kind,row,save,busy}){const [photo,setPhoto]=useState(null);const service=kind==='service';const [f,F]=useState(()=>service?{name:row?.name||'',description:row?.description||'',price:row?.price??'',durationMinutes:row?.durationMinutes??'',isActive:row?.isActive!==false,imageUrl:row?.imageUrl??null}:{name:row?.name||'',email:row?.email||'',phone:row?.phone||'',age:row?.age??'',education:row?.education||'',experienceYears:row?.experienceYears??'',specialization:row?.specialization||'',password:'',isActive:row?.isActive!==false});const field=(name,label,props={})=><label>{label}<input {...props} value={f[name]} onChange={e=>F(p=>({...p,[name]:e.target.value}))}/></label>;return <form onSubmit={e=>{e.preventDefault();const body={...f,name:f.name.trim()};if(service){body.price=Number(f.price);body.durationMinutes=Number(f.durationMinutes);}else{for(const k of ['age','experienceYears'])body[k]=f[k]===''?null:Number(f[k]);if(row&&!f.password)delete body.password;}save(body,photo);}}><fieldset disabled={busy} className="plain-fieldset">{field('name',service?'Название услуги':'Имя мастера',{required:true,maxLength:service?150:100,pattern:'.*\\S.*'})}{service?<><label>Описание<textarea rows={3} value={f.description} onChange={e=>F(p=>({...p,description:e.target.value}))}/></label>{field('price','Цена, ₽',{type:'number',required:true,min:0,max:99999999.99,step:0.01})}{field('durationMinutes','Длительность, мин',{type:'number',required:true,min:1,max:1440,step:1})}</>:<>{field('email','Email',{type:'email',required:true,maxLength:150})}{field('phone','Телефон',{type:'tel',maxLength:30})}{field('age','Возраст',{type:'number',min:0,max:120,step:1})}{field('education','Образование',{maxLength:255})}{field('experienceYears','Стаж, лет',{type:'number',min:0,max:100,step:1})}{field('specialization','Специализация',{maxLength:150})}{field('password',row?'Новый пароль (необязательно)':'Пароль',{type:'password',required:!row,minLength:8,maxLength:72,autoComplete:'new-password'})}</>}<label className="admin-check"><input type="checkbox" checked={f.isActive} onChange={e=>F(p=>({...p,isActive:e.target.checked}))}/> Активен{service?' (показывать на сайте)':''}</label><PhotoField current={row?.imageUrl} file={photo} onChange={setPhoto}/><button className="btn btn-dark" disabled={busy}>Сохранить</button></fieldset></form>}
function LeaveEditor({save,busy}) {
    // Минимальная дата отпуска — через 3 месяца
    const now = new Date();
    now.setHours(0, 0, 0, 0);
    now.setMonth(now.getMonth() + 3);

    const minDate = [
        now.getFullYear(),
        String(now.getMonth() + 1).padStart(2, '0'),
        String(now.getDate()).padStart(2, '0')
    ].join('-');

    const [from,F] = useState(minDate);
    const [to,T] = useState(minDate);
    const [reason,R] = useState('');

    return (
        <form onSubmit={e => {
            e.preventDefault();

            if (from < minDate || to < from) return;

            // Превращаем последний день отпуска
            // в следующий день для корректного сохранения периода
            const [y,m,d] = to.split('-').map(Number);
            const end = new Date(y, m - 1, d + 1);

            const next = [
                end.getFullYear(),
                String(end.getMonth() + 1).padStart(2, '0'),
                String(end.getDate()).padStart(2, '0')
            ].join('-');

            save({
                startDatetime: from + 'T00:00:00',
                endDatetime: next + 'T00:00:00',
                type: 'vacation',
                comment: reason.trim()
            });
        }}>
            <fieldset disabled={busy} className="plain-fieldset">
                <label>
                    Начало
                    <input
                        type="date"
                        required
                        min={minDate}
                        value={from}
                        onChange={e => {
                            F(e.target.value);
                            if (to < e.target.value) {
                                T(e.target.value);
                            }
                        }}
                    />
                </label>

                <label>
                    Последний день отпуска
                    <input
                        type="date"
                        required
                        min={from}
                        value={to}
                        onChange={e => T(e.target.value)}
                    />
                </label>

                <label>
                    Причина / комментарий
                    <textarea
                        maxLength={500}
                        value={reason}
                        onChange={e => R(e.target.value)}
                    />
                </label>

                <button
                    className="btn btn-dark"
                    disabled={from < minDate || to < from}
                >
                    Назначить отпуск
                </button>
            </fieldset>
        </form>
    );
}
function CancelEditor({row,save,busy}){const [reason,R]=useState('');return <><Info fields={appointmentFields(row)}/><form onSubmit={e=>{e.preventDefault();if(reason.trim())save({status:'cancelled',reason:reason.trim()});}}><label>Причина отмены<textarea required maxLength={250} rows={4} value={reason} onChange={e=>R(e.target.value)} disabled={busy}/></label><button className="btn admin-danger" disabled={busy||!reason.trim()}>Подтвердить отмену записи</button></form></>}
function appointmentFields(a){return [['Дата и время',dateText(a.startDatetime)],['Окончание',dateText(a.endDatetime)],['Мастер',a.groomerName],['Услуга',a.serviceName],['Клиент',a.customerName],['Телефон',a.customerPhone],['Питомец',a.petName],['Статус',<StatusLabel value={a.status}/>],['Комментарий',a.comment],['Причина отмены',a.cancellationReason]];}
export default function AdminPanel({user,token,logout,onHome,onCatalogChange}){const [tab,T]=useState('Услуги'),[data,D]=useState({}),[version,V]=useState(0),[loading,L]=useState(true),[errors,E]=useState({}),[modal,M]=useState(null),[busy,B]=useState(false),[message,S]=useState(''),[modalError,ME]=useState(''),[date,DateSet]=useState(''),[master,Master]=useState(''),[search,Search]=useState(''),[black,Black]=useState(false),[masterView,MV]=useState('list');
useEffect(()=>{const c=new AbortController();L(true);E({});const keys=tab==='Услуги'?['services']:tab==='Мастера'?['masters','leaves']:tab==='Записи'?['appointments','masters','users']:tab==='Отзывы'?['reviews','users']:tab==='Клиенты'?['users']:['masters','appointments','reviews'];Promise.all(keys.map(async k=>{try{const result=await request(paths[k],token,'GET',undefined,c.signal);if(!Array.isArray(result))throw new Error('Неверный формат ответа');return [k,result,null];}catch(e){return [k,[],e.message];}})).then(rows=>{if(c.signal.aborted)return;D(prev=>({...prev,...Object.fromEntries(rows.map(([k,r])=>[k,r]))}));E(Object.fromEntries(rows.filter(r=>r[2]).map(([k,r,e])=>[k,e])));L(false);});return()=>c.abort();},[tab,token,version]);
const rows=k=>data[k]||[];const open=m=>{if(busy)return;ME('');S('');M(m);};const close=()=>{ME('');M(null);};const select=t=>{T(t);S('');close();if(t==='Клиенты')Black(false);};async function mutate(path,method,body,photo,category,entityId){if(busy)return;B(true);ME('');S('');try{const result=await request(path,token,method,body);if(photo){try{await uploadPhoto(category,entityId??result?.id,photo,token);}catch(e){close();S('Данные сохранены, но фото не загружено: '+e.message+'. Откройте редактирование для повторной загрузки.');V(v=>v+1);onCatalogChange?.();return;}}close();S(result?.accountBanned?'Статус изменён. Клиент заблокирован сервером за повторные неявки.':'Изменения сохранены.');V(v=>v+1);onCatalogChange?.();}catch(e){ME(e.message);S(e.message);}finally{B(false);}}const confirm=(title,text,path,method='DELETE',body)=>open({kind:'confirm',title,text,path,method,body});const ban=u=>confirm(u.isBanned?'Разблокировать клиента':'Заблокировать клиента',u.isBanned?`Вернуть доступ: ${u.name}?`:`Заблокировать ${u.name}? Будущие активные записи будут отменены.`, `Users/${u.id}/${u.isBanned?'unban':'ban'}`,'PUT');const client=id=>{const u=rows('users').find(x=>String(x.id)===String(id));if(u)open({kind:'client',row:u});else S('Клиент не найден. Обновите список.');};const aList=rows('appointments').filter(a=>(!date||a.startDatetime?.slice(0,10)===date)&&(!master||String(a.groomerId)===master)).sort((a,b)=>String(b.createdAt||b.startDatetime).localeCompare(String(a.createdAt||a.startDatetime)));const userList=rows('users').filter(u=>!u.isAdmin&&!!u.isBanned===black&&String(u.name).toLowerCase().includes(search.trim().toLowerCase()));const stats=statistics(rows('masters'),rows('appointments'),rows('reviews'));const empty=a=>!a.length&&<p className="hint">Нет данных.</p>;
return <div className="admin-shell"><header className="admin-top"><button className="admin-profile" aria-label="Профиль администратора" title="Профиль администратора" onClick={()=>open({kind:'profile'})}>{user.name?.trim().slice(0,1).toUpperCase()||'А'}</button><nav aria-label="Разделы администратора">{tabs.map(t=><button key={t} className={tab===t?'active':''} onClick={()=>select(t)} disabled={busy}>{t}</button>)}<button onClick={logout} disabled={busy}>Выйти</button></nav></header><section className="admin-content"><div className="admin-heading"><div><p className="eyebrow">ПАНЕЛЬ АДМИНИСТРАТОРА</p><h1>{tab}</h1></div>{['Услуги','Мастера'].includes(tab)&&<button className="btn btn-dark admin-add" disabled={busy} aria-label={tab==='Услуги'?'Добавить услугу':'Добавить мастера'} onClick={()=>open({kind:tab==='Услуги'?'service':'master'})}>+</button>}</div>{message&&<p role="status" className="form-message">{message}</p>}{Object.entries(errors).map(([k,e])=><p key={k} role="alert" className="field-error">{paths[k]}: {e}</p>)}{loading?<p>Загрузка…</p>:<fieldset disabled={busy} className="plain-fieldset">
{tab==='Услуги'&&<>{empty(rows('services'))}{rows('services').map(s=><article className="admin-row" key={s.id}><div><h3>{s.name}</h3><p>{s.description}</p><p>{Number(s.price).toLocaleString('ru-RU')} ₽ · {s.durationMinutes} мин · <StatusLabel value={s.isActive===false?"inactive":"active"}/></p></div><div className="admin-actions"><Button onClick={()=>open({kind:'service',row:s})}>Редактировать</Button><Button danger onClick={()=>confirm('Удалить услугу',`Удалить «${s.name}»? Если есть история записей, услуга будет скрыта вместо удаления.`,`Services/${s.id}`)}>Удалить</Button></div></article>)}</>}
{tab==='Мастера'&&<><div className="admin-actions admin-filters"><Button onClick={()=>MV('list')}>Перечень мастеров</Button><Button onClick={()=>MV('calendar')}>Общее расписание</Button><Button onClick={()=>MV('leaves')}>Заявки на отпуск ({rows('leaves').filter(l=>l.status==='pending').length})</Button></div>{masterView==='calendar'?<AdminCalendar token={token} groomers={rows('masters')} version={version}/>:masterView==='leaves'?<>{empty(rows('leaves'))}{rows('leaves').map(l=><article className="admin-row" key={l.id}><div><h3>{l.groomerName}</h3><p>{l.startDate?.slice(0,10)} — {l.endDate?.slice(0,10)}</p><p>{l.reason} · <StatusLabel value={l.status}/></p></div>{l.status==='pending'&&<div className="admin-actions"><Button onClick={()=>mutate(`LeaveRequests/${l.id}/decision`,'PATCH',{status:'approved'})}>Подтвердить</Button><Button danger onClick={()=>mutate(`LeaveRequests/${l.id}/decision`,'PATCH',{status:'rejected'})}>Отклонить</Button></div>}</article>)}</>:<>{empty(rows('masters'))}{rows('masters').map(g=><article className="admin-row" key={g.id}><div><h3>{g.name}</h3><p>{g.specialization||'Специализация не указана'} · <StatusLabel value={g.isActive===false?"inactive":"active"}/></p></div><div className="admin-actions"><Button onClick={()=>open({kind:'masterInfo',row:g})}>Сведения</Button><Button onClick={()=>open({kind:'portfolio',row:g})}>Портфолио</Button><Button onClick={()=>open({kind:'schedule',row:g})}>Расписание</Button><Button onClick={()=>open({kind:'leave',row:g})}>Назначить отпуск</Button><Button danger onClick={()=>confirm('Удалить мастера',`Удалить ${g.name}? Если есть история записей, аккаунт будет деактивирован.`,`Groomers/${g.id}`)}>Удалить</Button></div></article>)}</>}</>}
{tab==='Записи'&&<><div className="admin-filters"><label>Дата<input type="date" value={date} onChange={e=>DateSet(e.target.value)}/></label><label>Мастер<select value={master} onChange={e=>Master(e.target.value)}><option value="">Все мастера</option>{rows('masters').map(g=><option key={g.id} value={g.id}>{g.name}</option>)}</select></label><Button onClick={()=>{DateSet('');Master('');}}>Сбросить фильтры</Button></div>{empty(aList)}{aList.map(a=><article className="admin-row" key={a.id}><div><h3>{a.serviceName} · <StatusLabel value={a.status}/></h3><p>{dateText(a.startDatetime)} — {String(a.endDatetime||'').slice(11,16)} · {a.groomerName}</p><p>Клиент: <button className="subtle-link" onClick={()=>client(a.userId)}>{a.customerName}</button> · {a.petName}</p>{a.cancellationReason&&<p>Причина отмены: {a.cancellationReason}</p>}</div><div className="admin-actions"><Button onClick={()=>open({kind:'appointment',row:a})}>Сведения</Button><Button danger disabled={!active(a)} onClick={()=>open({kind:'cancel',row:a})}>Отменить запись</Button>{canMarkNoShow(a)&&<Button danger onClick={()=>confirm('Неявка на приём',`Отметить неявку клиента ${a.customerName||''}? При трёх последних неявках сервер может заблокировать клиента и отменить его будущие записи.`, `Appointments/${a.id}/status`,'PUT',{status:'no_show'})}>Отметить неявку</Button>}</div></article>)}</>}
{tab==='Отзывы'&&<>{empty(rows('reviews'))}{rows('reviews').map(r=><article className="admin-row" key={r.id}><div><div className="review-rating-row"><ReviewPaws rating={r.rating}/><StatusLabel value={r.status} context="review"/></div><p>Мастер: {r.groomerName} · Автор: <button className="subtle-link" onClick={()=>client(r.userId)}>{r.userName}</button></p><p className="admin-review-text">{r.text}</p><p className="hint">{dateText(r.createdAt)}</p></div><div className="admin-actions"><Button disabled={r.status!=='pending'} onClick={()=>mutate(`Reviews/${r.id}/moderate`,'PATCH',{status:'approved'})}>Опубликовать</Button><Button danger onClick={()=>confirm('Удалить отзыв','Удалить отзыв без возможности восстановления?',`Reviews/${r.id}`)}>Удалить</Button><Button danger disabled={!rows('users').some(u=>u.id===r.userId&&!u.isAdmin&&!u.isBanned)} onClick={()=>{const u=rows('users').find(u=>u.id===r.userId);if(u)ban(u);}}>Бан</Button></div></article>)}</>}
{tab==='Клиенты'&&<><div className="admin-filters"><label>Поиск по имени<input value={search} onChange={e=>Search(e.target.value)} placeholder="Имя клиента"/></label><Button onClick={()=>Black(!black)}>{black?'Все клиенты':'Чёрный список'} ({rows('users').filter(u=>!u.isAdmin&&!!u.isBanned===!black).length})</Button></div><h2>{black?'Чёрный список':'Все клиенты'}</h2>{empty(userList)}{userList.map(u=><article className="admin-row" key={u.id}><div><h3>{u.name}</h3><p>{u.email} · {u.phone||'Телефон не указан'} <StatusLabel value={u.isBanned?"banned":"active"}/></p></div><div className="admin-actions"><Button onClick={()=>open({kind:'client',row:u})}>Сведения</Button><Button danger={!u.isBanned} onClick={()=>ban(u)}>{u.isBanned?'Разблокировать':'Бан'}</Button></div></article>)}</>}
{tab==='Статистика'&&<>{Object.keys(errors).length?<p>Статистика недоступна: не все данные загружены.</p>:<><p className="hint">За весь период. Посещения — завершённые записи; рейтинг — только опубликованные отзывы.</p><div className="admin-stats"><div><span>Посещения</span><strong>{stats.total}</strong></div><div><span>Самый посещаемый день</span><strong>{stats.days[0]?.count?stats.days[0].name:'—'}</strong></div><div><span>Опубликованные отзывы</span><strong>{stats.reviewCount}</strong></div></div><div className="admin-table-wrap"><table className="admin-table"><thead><tr><th>Мастер</th><th>Рейтинг</th><th>Посещения</th><th>Популярный день</th></tr></thead><tbody>{stats.masters.map(g=><tr key={g.id}><td>{g.name}</td><td>{g.rating===null?'Нет оценок':`${g.rating.toFixed(1)} / 5 (${g.reviewCount})`}</td><td>{g.count}</td><td>{g.day}</td></tr>)}</tbody></table></div><h2>Посещения по дням недели</h2>{stats.days.map(d=><div className="admin-bar" key={d.name}><span>{d.name}</span><div><i style={{width:`${stats.total?d.count/stats.total*100:0}%`}}/></div><b>{d.count}</b></div>)}</>}</>}
</fieldset>}</section>{modal&&<Modal wide={modal.kind==='schedule'} key={`${modal.kind}-${modal.row?.id||''}`} title={modal.title||({profile:'Профиль администратора',service:modal.row?'Редактирование услуги':'Новая услуга',master:modal.row?'Редактирование мастера':'Новый мастер',masterInfo:'Сведения о мастере',portfolio:'Редактирование портфолио',schedule:'Расписание мастера',leave:'Назначить отпуск',appointment:'Сведения о записи',cancel:'Отмена записи',client:'Сведения о клиенте'}[modal.kind])} close={close} busy={busy}>{modalError&&<p className="field-error" role="alert">{modalError}</p>}{modal.kind==='profile'&&<><Info fields={[["Имя",user.name],["Email",user.email],["Телефон",user.phone],["Роль","Администратор"]]}/><Button onClick={onHome}>На сайт</Button></>}{['service','master'].includes(modal.kind)&&<Editor kind={modal.kind} row={modal.row} busy={busy} save={(body,photo)=>mutate(`${modal.kind==='service'?'Services':'Groomers'}${modal.row?'/'+modal.row.id:''}`,modal.row?'PUT':'POST',body,photo,modal.kind==='service'?'services':'groomers',modal.row?.id)}/ >}{modal.kind==='masterInfo'&&<><Info fields={[["Имя",modal.row.name],["Email",modal.row.email],["Телефон",modal.row.phone],["Возраст",modal.row.age],["Образование",modal.row.education],["Стаж",modal.row.experienceYears],["Специализация",modal.row.specialization],["Статус",<StatusLabel value={modal.row.isActive===false?"inactive":"active"}/>]]}/><Button onClick={()=>open({kind:'master',row:modal.row})}>Редактировать данные</Button></>}{modal.kind==='portfolio'&&<><h3>{modal.row.name}</h3><Portfolio key={modal.row.id} id={modal.row.id} token={token} editable/></>}{modal.kind==='schedule'&&<AdminCalendar token={token} groomers={[modal.row]} onlyId={modal.row.id} version={version}/ >}{modal.kind==='leave'&&<><p>{modal.row.name}</p><LeaveEditor busy={busy} save={body=>mutate('GroomerTimeOff','POST',{...body,groomerId:modal.row.id})}/></>}{modal.kind==='appointment'&&<Info fields={appointmentFields(modal.row)}/ >}{modal.kind==='cancel'&&<CancelEditor row={modal.row} busy={busy} save={body=>mutate(`Appointments/${modal.row.id}/status`,'PUT',body)}/ >}{modal.kind==='client'&&<><Info fields={[["Имя",modal.row.name],["Email",modal.row.email],["Телефон",modal.row.phone],["Регистрация",dateText(modal.row.createdAt)],["Статус",<StatusLabel value={modal.row.isBanned?"banned":"active"}/>]]}/>{!modal.row.isAdmin&&<Button danger={!modal.row.isBanned} onClick={()=>ban(modal.row)}>{modal.row.isBanned?'Разблокировать':'Бан'}</Button>}</>}{modal.kind==='confirm'&&<><p>{modal.text}</p><div className="admin-actions"><Button disabled={busy} onClick={close}>Назад</Button><Button danger disabled={busy} onClick={()=>mutate(modal.path,modal.method,modal.body)}>Подтвердить</Button></div></>}</Modal>}</div>}
