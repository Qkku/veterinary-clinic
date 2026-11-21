import HeroSlider from './hero-slider';
import ClientNotifications from './client-notifications';
import {PortfolioPage} from './portfolio';
import {photoUrl} from './photo-field';
import React, { useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import './style.css';

import { API_URL } from './api';
import { login, profile } from './auth';
import AdminPanel from './admin-panel';
import GroomerPanel from './groomer-panel';
import { Booking, Cabinet, roleOf } from './workspace';

function PasswordInput(props) {
 const [visible, setVisible] = useState(false);
 return <span className="password-field">
  <input {...props} type={visible ? 'text' : 'password'} />
  <button type="button" className="password-toggle" aria-label={visible ? 'Скрыть пароль' : 'Показать пароль'} aria-pressed={visible} title={visible ? 'Скрыть пароль' : 'Показать пароль'} onClick={()=>setVisible(v=>!v)}>
   <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7Z"/><circle cx="12" cy="12" r="3"/>{visible&&<path d="M3 3l18 18"/>}
   </svg>
  </button>
 </span>;
}

const dayString = d => [d.getFullYear(), String(d.getMonth()+1).padStart(2,'0'), String(d.getDate()).padStart(2,'0')].join('-');
const rub = amount => new Intl.NumberFormat('ru-RU').format(amount) + ' ₽';

const validateAuth = (form, mode) => {
 const errors = {};
 const name = form.name.trim();
 const email = form.email.trim();
 const phone = form.phone.trim();
 if (mode === 'register') {
  if (!name) errors.name = 'Укажите имя.';
  else if (name.length < 2) errors.name = 'Имя должно содержать минимум 2 символа.';
  else if (name.length > 100) errors.name = 'Имя слишком длинное (максимум 100 символов).';
  else if (!/^[\p{L}\s'-]+$/u.test(name)) errors.name = 'В имени допустимы буквы, пробел, дефис и апостроф.';
  if (phone && !/^\+?[0-9\s()\-]+$/.test(phone)) errors.phone = 'Телефон может содержать цифры, +, пробелы, скобки и дефисы.';
  else if (phone && (phone.replace(/\D/g, '').length < 10 || phone.replace(/\D/g, '').length > 15)) errors.phone = 'Укажите номер из 10–15 цифр.';
 }
 if (!email) errors.email = 'Укажите email.';
 else if (email.length > 150 || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) errors.email = 'Введите корректный email, например name@mail.ru.';
 if (!form.password) errors.password = 'Введите пароль.';
 else if (mode === 'register' && form.password.length < 8) errors.password = 'Пароль должен содержать минимум 8 символов.';
 else if (mode === 'register' && form.password.length > 72) errors.password = 'Пароль должен содержать не более 72 символов.';
 if (mode === 'register') {
  if (!form.confirmPassword) errors.confirmPassword = 'Повторите пароль.';
  else if (form.password !== form.confirmPassword) errors.confirmPassword = 'Пароли не совпадают.';
 }
 return errors;
};

function RatingPaws({value}){
 const mean=Number(value);
 return <span role="img" aria-label={`Оценка ${mean} из 5`}> <span className="master-rating-paws" aria-hidden="true">{[1,2,3,4,5].map(n=><span key={n} className="master-rating-paw"><svg viewBox="0 0 32 32" width="18" height="18" aria-hidden="true" fill="currentColor"><ellipse cx="6" cy="13" rx="3.5" ry="5" transform="rotate(-24 6 13)"/><ellipse cx="12.5" cy="7.5" rx="3.5" ry="5" transform="rotate(-8 12.5 7.5)"/><ellipse cx="20" cy="7.5" rx="3.5" ry="5" transform="rotate(8 20 7.5)"/><ellipse cx="26" cy="13" rx="3.5" ry="5" transform="rotate(24 26 13)"/><path d="M16 14.5c-4 0-5.2 3.7-8.5 6.2C2 25 6.5 30 11 28.5c2-.7 3.4-1.5 5-1.5s3 .8 5 1.5C25.5 30 30 25 24.5 20.7 21.2 18.2 20 14.5 16 14.5Z"/></svg><span className="master-rating-fill" style={{width:`${Math.max(0,Math.min(1,mean-n+1))*100}%`}}><svg viewBox="0 0 32 32" width="18" height="18" aria-hidden="true" fill="currentColor"><ellipse cx="6" cy="13" rx="3.5" ry="5" transform="rotate(-24 6 13)"/><ellipse cx="12.5" cy="7.5" rx="3.5" ry="5" transform="rotate(-8 12.5 7.5)"/><ellipse cx="20" cy="7.5" rx="3.5" ry="5" transform="rotate(8 20 7.5)"/><ellipse cx="26" cy="13" rx="3.5" ry="5" transform="rotate(24 26 13)"/><path d="M16 14.5c-4 0-5.2 3.7-8.5 6.2C2 25 6.5 30 11 28.5c2-.7 3.4-1.5 5-1.5s3 .8 5 1.5C25.5 30 30 25 24.5 20.7 21.2 18.2 20 14.5 16 14.5Z"/></svg></span></span>)}</span></span>;
}

function MasterRating({id,reviews,ready,error}){
 if(error)return <div className="master-rating hint">Рейтинг временно недоступен</div>;
 if(!ready)return <div className="master-rating hint">Загрузка рейтинга…</div>;
 const scores=reviews.filter(r=>String(r.groomerId)===String(id)&&r.rating!=null).map(r=>Number(r.rating)).filter(n=>Number.isFinite(n)&&n>=1&&n<=5);
 if(!scores.length)return <div className="master-rating hint">Пока нет оценок</div>;
 const mean=scores.reduce((a,b)=>a+b,0)/scores.length;
 return <div className="master-rating" aria-label={`Рейтинг ${mean.toFixed(1)} из 5, оценок: ${scores.length}`}><RatingPaws value={mean}/><strong>{mean.toFixed(1).replace('.',',')}</strong><small>({scores.length})</small></div>;
}

function App(){
 const routePage=()=>/^portfolio\/\d+$/.test(location.hash.slice(2))?location.hash.slice(2):['home','services','masters','booking','success','cabinet'].includes(location.hash.slice(2))?location.hash.slice(2):'home';
 const [page,setPage]=useState(routePage);
 useEffect(()=>{const sync=()=>setPage(routePage());window.addEventListener('hashchange',sync);return()=>window.removeEventListener('hashchange',sync);},[]);
 useEffect(()=>{if(location.hash!=='#/'+page)history.replaceState(null,'','#/'+page);},[page]);
 const [returnToBooking,setReturnToBooking]=useState(false);
 const [preferredMaster,setPreferredMaster]=useState('');
 const [services,setServices]=useState([]);
 const [publicReviews,setPublicReviews]=useState([]);
 const [reviewError,setReviewError]=useState('');
 const [reviewsReady,setReviewsReady]=useState(false);
 const [groomers,setGroomers]=useState([]);
 const [apiLoading,setApiLoading]=useState(true);
 const [catalogVersion,refreshCatalog]=useState(0);
 const [apiError,setApiError]=useState('');
 const [authToken,setAuthToken]=useState(()=>localStorage.getItem('grooming_token') || '');
 const [authUser,setAuthUser]=useState(null);
 const [authKind,setAuthKind]=useState(()=>localStorage.getItem('grooming_account_kind')==='groomer'?'groomer':'user');
 const [profileState,setProfileState]=useState(()=>localStorage.getItem('grooming_token')?'loading':'idle');
 const [profileError,setProfileError]=useState('');
 const [profileRetry,retryProfile]=useState(0);
 const [authMode,setAuthMode]=useState('login');
 const [authForm,setAuthForm]=useState({name:'',email:'',phone:'',password:'',confirmPassword:''});
 const [authMessage,setAuthMessage]=useState('');
 const [authErrors,setAuthErrors]=useState({});
 const [authTouched,setAuthTouched]=useState({});
 const [authLoading,setAuthLoading]=useState(false);
 const [form,setForm]=useState({service_id:'',groomer_id:''});
 const [message,setMessage]=useState('');
 useEffect(()=>{const c=new AbortController();if(page==='home'||page==='masters'){setReviewsReady(false);setReviewError('');fetch(`${API_URL}/Reviews/public`,{signal:c.signal}).then(r=>{if(!r.ok)throw new Error('Отзывы временно недоступны');return r.json();}).then(rows=>{if(!c.signal.aborted){if(!Array.isArray(rows))throw new Error('Некорректный ответ сервера');setPublicReviews(rows);setReviewsReady(true);}}).catch(e=>{if(!c.signal.aborted)setReviewError(e.message);});}return()=>c.abort();},[page,catalogVersion]);
 useEffect(()=>{
  async function loadCatalog(){
   try{
    setApiLoading(true);
    setApiError('');
    const [servicesResponse,groomersResponse]=await Promise.all([
     fetch(`${API_URL}/Services`),
     fetch(`${API_URL}/Groomers`)
    ]);
    if(!servicesResponse.ok) throw new Error(`Не удалось загрузить услуги: HTTP ${servicesResponse.status}`);
    if(!groomersResponse.ok) throw new Error(`Не удалось загрузить мастеров: HTTP ${groomersResponse.status}`);
    const servicesData=await servicesResponse.json();
    const groomersData=await groomersResponse.json();
    const activeServices=servicesData.filter(s=>s.isActive!==false).map(s=>({
     ...s,
     duration_minutes:s.durationMinutes ?? s.duration_minutes,
     price:Number(s.price)
    }));
    const activeGroomers=(Array.isArray(groomersData)?groomersData:groomersData.groomers ?? []).filter(g=>g.isActive!==false).map((g,index)=>({
     ...g,
     experience_years:g.experienceYears ?? g.experience_years ?? 0,
     initials:(g.name ?? 'Мастер').split(' ').map(part=>part[0]).slice(0,2).join('').toUpperCase(),
     color:['peach','lavender','sage'][index%3]
    }));
    setServices(activeServices);
    setGroomers(activeGroomers);
    if(activeServices.length) setForm(f=>({...f,service_id:activeServices.some(s=>String(s.id)===f.service_id)?f.service_id:String(activeServices[0].id)}));
    if(activeGroomers.length) setForm(f=>({...f,groomer_id:activeGroomers.some(g=>String(g.id)===f.groomer_id)?f.groomer_id:String(activeGroomers[0].id)}));
   }catch(error){
    console.error('Ошибка подключения к API:',error);
    setApiError(`${error.message}. Проверь, что backend запущен и открыт http://localhost:5125/swagger/index.html.`);
   }finally{
    setApiLoading(false);
   }
  }
  loadCatalog();
 },[catalogVersion]);
 useEffect(()=>{
  if(!authToken){setAuthUser(null);setProfileState('idle');return;}
  const c=new AbortController();
  setProfileState('loading');setProfileError('');
  profile(authToken,authKind,c.signal).then(user=>{
   if(c.signal.aborted)return;
   setAuthUser(user);setProfileState('ready');setAuthMessage('');
  }).catch(error=>{
   if(c.signal.aborted)return;
   setAuthUser(null);
   if(error.status===401||error.status===403){
    localStorage.removeItem('grooming_token');localStorage.removeItem('grooming_account_kind');
    setAuthToken('');setProfileState('idle');
    setAuthMessage(`Сервер не подтвердил сессию (HTTP ${error.status}). ${error.message}`);
   }else{setProfileState('error');setProfileError(error.message||'Не удалось проверить профиль.');}
  });
  return ()=>c.abort();
 },[authToken,authKind,profileRetry]);
 function go(next,bookingMaster=''){if(next==='booking')setPreferredMaster(String(bookingMaster||''));if(next!=='booking'&&next!=='cabinet')setReturnToBooking(false);if(next==='booking'&&!authUser){setReturnToBooking(true);next='cabinet';}else if(next==='booking'&&roleOf(authUser)!=='client'){next='cabinet';}location.hash='/'+next;setPage(next);setMessage('');setAuthMessage('');window.scrollTo({top:0,behavior:'smooth'});}
 useEffect(()=>{if(page==='booking'&&!authToken){setReturnToBooking(true);setPage('cabinet');}else if(page==='booking'&&authUser&&roleOf(authUser)!=='client'){setPage('cabinet');}},[page,authToken,authUser]);
 useEffect(()=>{if(authUser&&returnToBooking){setReturnToBooking(false);if(roleOf(authUser)==='client')setPage('booking');}},[authUser,returnToBooking]);
 function setAuthField(field, value) {
  const next = {...authForm, [field]: value};
  setAuthForm(next);
  setAuthErrors(previous => {
   if (!authTouched[field] && !Object.keys(previous).length) return previous;
   const validation = validateAuth(next, authMode);
   const updated = {...previous};
   for (const key of Object.keys(updated)) {
    if (!validation[key]) delete updated[key];
    else updated[key] = validation[key];
   }
   if (authTouched[field] && validation[field]) updated[field] = validation[field];
   if (field === 'password' && authTouched.confirmPassword) {
    if (validation.confirmPassword) updated.confirmPassword = validation.confirmPassword;
    else delete updated.confirmPassword;
   }
   return updated;
  });
 }
 function touchAuthField(field) {
  setAuthTouched(previous => ({...previous, [field]: true}));
  const validation = validateAuth(authForm, authMode);
  setAuthErrors(previous => ({...previous, [field]: validation[field]}));
 }
 function changeAuthMode(mode) {
  setAuthMode(mode);
  setAuthMessage('');
  setAuthErrors({});
  setAuthTouched({});
 }
 async function submitAuth(e){
  e.preventDefault();
  if (authLoading) return;
  setAuthMessage('');
  const errors = validateAuth(authForm, authMode);
  setAuthErrors(errors);
  setAuthTouched(Object.fromEntries(Object.keys(authForm).map(field => [field,true])));
  if (Object.keys(errors).length) return;
  setAuthLoading(true);
  try{
   const endpoint=authMode==='login'?'Auth/login':'Users/register';
   const payload=authMode==='login'
    ? {email:authForm.email.trim(),password:authForm.password}
    : {name:authForm.name.trim(),email:authForm.email.trim(),phone:authForm.phone.trim()||null,password:authForm.password};
   if(authMode==='login'){
    const result=await login(payload);
    localStorage.setItem('grooming_token',result.token);
    localStorage.setItem('grooming_account_kind',result.kind);
    setAuthKind(result.kind);setAuthToken(result.token);setAuthUser(null);setProfileState('loading');
    setAuthForm(f=>({...f,password:'',confirmPassword:''}));
    return;
   }
   const response=await fetch(`${API_URL}/${endpoint}`,{
    method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(payload)
   });
   const data=await response.json().catch(()=>({}));
   if(!response.ok){
    if (data.errors && typeof data.errors === 'object') {
     const fieldErrors = {};
     for (const [field,messages] of Object.entries(data.errors)) {
      const normalized = field.charAt(0).toLowerCase() + field.slice(1);
      if (['name','email','phone','password','confirmPassword'].includes(normalized)) {
       fieldErrors[normalized] = Array.isArray(messages) ? messages.join(' ') : String(messages);
      }
     }
     if (Object.keys(fieldErrors).length) setAuthErrors(previous=>({...previous,...fieldErrors}));
    }
    const errorText = typeof data.message === 'string' ? data.message
     : typeof data.title === 'string' && response.status === 400 ? 'Проверьте правильность заполнения полей.'
     : response.status===409 ? 'Пользователь с таким email уже существует.'
     : response.status===401 ? 'Неверный email или пароль.'
     : response.status===403 ? 'Доступ запрещён.'
     : `Ошибка сервера: ${response.status}`;
    throw new Error(errorText);
   }
   if(authMode==='register'){
    changeAuthMode('login');
    setAuthForm(f=>({...f,name:'',phone:'',confirmPassword:'',password:''}));
    setAuthMessage('Регистрация прошла успешно. Теперь войдите, используя email и пароль.');
   }
  }catch(error){setAuthMessage(error instanceof TypeError ? 'Нет соединения с сервером. Проверьте запуск Backend и адрес API.' : error.message || 'Не удалось выполнить запрос.');}
  finally{setAuthLoading(false);}
 }
 function logout(){localStorage.removeItem('grooming_account_kind');setProfileState('idle');setProfileError('');setReturnToBooking(false);setPage('home');localStorage.removeItem('grooming_token');setAuthToken('');setAuthUser(null);setAuthForm({name:'',email:'',phone:'',password:'',confirmPassword:''});setAuthMessage('Вы вышли из аккаунта.');}
 function prefService(id){setForm(f=>({...f,service_id:String(id)}));go('booking');}
 if(page==='cabinet'&&authUser&&profileState==='ready'&&roleOf(authUser)==='groomer')return <GroomerPanel key={authUser.id} user={authUser} token={authToken} logout={logout} onHome={()=>go('home')}/>;
 if(page==='cabinet'&&authUser&&profileState==='ready'&&roleOf(authUser)==='admin')return <AdminPanel user={authUser} token={authToken} logout={logout} onHome={()=>go('home')} onCatalogChange={()=>refreshCatalog(v=>v+1)}/>;
 return <div className={page==='cabinet'?'app cabinet-page':'app'}>
  {page==='cabinet'&&!(authUser&&profileState==='ready'&&roleOf(authUser)==='client')?<header className="cabinet-header"><button className="brand" onClick={()=>go('home')}><span className="brand-word">Шерст<span className="brand-accent-letter">ю<svg className="brand-accent-heart" viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M12 21C10 19 2 13.5 2 7.8 2 2.5 8.4 1 12 5.5 15.6 1 22 2.5 22 7.8 22 13.5 14 19 12 21Z"/></svg></span>га</span></button><button className="btn btn-outline" onClick={()=>go('home')}>На сайт</button></header>:<header className="header"><div className="wrap head"><button className="brand" onClick={()=>go('home')}><span><span className="brand-word">Шерст<span className="brand-accent-letter">ю<svg className="brand-accent-heart" viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M12 21C10 19 2 13.5 2 7.8 2 2.5 8.4 1 12 5.5 15.6 1 22 2.5 22 7.8 22 13.5 14 19 12 21Z"/></svg></span>га</span><small>ГРУМИНГ-САЛОН</small></span></button><nav className="nav" aria-label="Навигация сайта"><button className={page==='home'?'active':''} onClick={()=>go('home')}>Главная</button><button className={page==='services'?'active':''} onClick={()=>go('services')}>Услуги</button><button className={(page==='masters'||page.startsWith('portfolio/'))?'active':''} onClick={()=>go('masters')}>Мастера</button><button className={page==='cabinet'?'active':''} onClick={()=>go('cabinet')}>Личный кабинет</button></nav>{page==='cabinet'?<ClientNotifications key={authToken} token={authToken}/>:<button className="btn btn-dark header-book" onClick={()=>{if(!authUser){changeAuthMode('login');go('cabinet');}else go('booking');}}>{authUser ? 'Записаться' : 'Войти'}</button>}</div></header>}
  {page!=='cabinet'&&apiLoading&&<div className="wrap" style={{padding:'12px 0'}}>Загружаем услуги и мастеров из базы данных…</div>}
  {page!=='cabinet'&&apiError&&<div className="wrap" style={{padding:'12px 0',color:'#a33'}}>{apiError}</div>}
  <main>
  {page==='home'&&<><section className="hero"><div className="wrap hero-grid"><div className="hero-content"><span className="eyebrow">ГРУМИНГ-САЛОН «ШЕРСТЮГА»</span><h1>Забота о питомце <em>с любовью.</em></h1><p>Стрижка, купание и уход за шерстью. Запишите своего пушистого друга к нашим добрым мастерам — онлайн. <span aria-hidden="true" style={{color:"#9B98A5",fontSize:"0.85em",whiteSpace:"nowrap"}}>♥</span></p><div className="hero-actions"><button className="btn btn-dark" onClick={()=>go('booking')}>Записаться онлайн</button><button className="btn btn-outline" onClick={()=>go('services')}>Посмотреть услуги</button></div></div><div className="hero-art"><HeroSlider/></div></div></section><section className="section wrap"><div className="section-top"><div><span className="eyebrow">ЧТО МЫ ПРЕДЛАГАЕМ</span><h2>Услуги <em>нашего салона</em></h2></div><button className="subtle-link" onClick={()=>go('services')}>Все услуги</button></div><div className="service-grid">{services.slice(0,3).map(s=><ServiceCard key={s.id} s={s} onPick={prefService}/>)}</div></section><section className="cta"><div className="wrap cta-content"><span className="eyebrow">ПРОСТО И УДОБНО</span><h2>Запишите питомца <em>онлайн</em></h2><p>Выберите удобное время, мастера и процедуру — всё в пару кликов.</p><button className="btn btn-dark" onClick={()=>go('booking')}>Выбрать время</button></div></section><section className="section wrap"><h2>Отзывы клиентов</h2>{reviewError?<p className="hint">{reviewError}</p>:!publicReviews.length?<p className="hint">Пока нет опубликованных отзывов.</p>:<div className="service-grid">{publicReviews.map(r=><article className="panel" key={r.id}><h3>{r.userName}</h3><RatingPaws value={r.rating}/><p>Мастер: {r.groomerName}</p><p className="admin-review-text">{r.text}</p></article>)}</div>}</section></>}
  {page==='services'&&<section className="section wrap page-section"><span className="eyebrow">НАШ ПРАЙС</span><h1 className="page-title">Услуги <em>и цены</em></h1><p className="page-desc">Подберём уход по потребностям вашего питомца.</p><div className="service-grid">{services.map(s=><ServiceCard key={s.id} s={s} onPick={prefService}/>)}</div></section>}
  {page==='masters'&&<section className="section wrap page-section"><span className="eyebrow">ЗНАКОМЬТЕСЬ</span><h1 className="page-title">Наши <em>мастера</em></h1><p className="page-desc">Выберите специалиста, которому доверите своего питомца.</p><div className="master-grid">{groomers.map(g=><article className="master-card" key={g.id}><a className={'master-portrait master-profile-link '+g.color} href={'#/portfolio/'+g.id} aria-label={'Портфолио: '+g.name}>{g.imageUrl?<img className="catalog-photo" src={photoUrl(g.imageUrl)} alt={g.name}/>:<span>{g.initials}</span>}</a><h3><a className="master-name-link" href={'#/portfolio/'+g.id}>{g.name}</a></h3><p>{g.specialization}</p><MasterRating id={g.id} reviews={publicReviews} ready={reviewsReady} error={reviewError}/><div className="master-meta">Опыт {g.experience_years} лет</div><div className="portfolio-card-actions"><button className="btn btn-outline full" onClick={()=>go('booking',g.id)}>Выбрать время</button></div></article>)}</div></section>}
  {page.startsWith('portfolio/')&&<PortfolioPage key={page} id={page.split('/')[1]} onBack={()=>go('masters')} onBook={()=>go('booking',page.split('/')[1])}/>}
  {page==='booking'&&authUser&&roleOf(authUser)==='client'&&<Booking token={authToken} services={services} groomers={groomers} initialService={form.service_id} initialMaster={preferredMaster} onSuccess={()=>go('success')} onCabinet={()=>go('cabinet')}/>}
  {page==='success'&&<section className="section wrap success-page"><span className="eyebrow">ГОТОВО!</span><h1 className="page-title">Вы <em>записаны!</em></h1><p>Запись успешно создана. Она уже отображается в вашем личном кабинете.</p><div className="hero-actions"><button className="btn btn-dark" onClick={()=>go('cabinet')}>Мои записи</button><button className="btn btn-outline" onClick={()=>go('home')}>На главную</button></div></section>}
  {page==='cabinet'&&<section className="cabinet-content"><h1 className="cabinet-title">{authUser?(roleOf(authUser)==='admin'?'Панель администратора':roleOf(authUser)==='groomer'?'Кабинет мастера':'Личный кабинет'):'Вход в личный кабинет'}</h1><p className="page-desc">{authUser ? (roleOf(authUser) === 'client' ? "Управляй своим аккаунтом даже лапками." : "Управляйте своим аккаунтом.") : returnToBooking ? "Войдите или зарегистрируйтесь, чтобы продолжить запись." : "Войдите в аккаунт или зарегистрируйтесь."}</p>
   {authToken&&profileState!=='ready' ? <div className="panel profile-panel" style={{maxWidth:'560px',margin:'0 auto'}}>{profileState==='error'?<><p role="alert" className="field-error">Не удалось проверить профиль: {profileError}</p><button className="btn btn-dark" onClick={()=>retryProfile(v=>v+1)}>Повторить</button></>:<p>Проверяем аккаунт…</p>}<button className="btn btn-outline" onClick={logout}>Выйти</button></div> : !authUser ? <div className="panel profile-panel" style={{maxWidth:'560px',margin:'0 auto'}}>
    <div className="panel-heading"><h2>{authMode==='login'?'Вход в аккаунт':'Регистрация'}</h2></div>
    <div className="hero-actions" style={{marginBottom:'20px'}}>
     <button type="button" className={'btn '+(authMode==='login'?'btn-dark':'btn-outline')} onClick={()=>changeAuthMode('login')}>Войти</button>
     <button type="button" className={'btn '+(authMode==='register'?'btn-dark':'btn-outline')} onClick={()=>changeAuthMode('register')}>Регистрация</button>
    </div>
    <form onSubmit={submitAuth} noValidate>
     {authMode==='register'&&<>
      <label>Имя<input required maxLength="100" autoComplete="name" placeholder="Ваше имя" value={authForm.name} aria-invalid={!!authErrors.name} aria-describedby={authErrors.name?'name-error':undefined} onBlur={()=>touchAuthField('name')} onChange={e=>setAuthField('name',e.target.value)}/>{authErrors.name&&<small id="name-error" className="field-error" role="alert">{authErrors.name}</small>}</label>
      <label>Телефон <small>(необязательно)</small><input type="tel" maxLength="30" autoComplete="tel" placeholder="+7 900 000-00-00" value={authForm.phone} aria-invalid={!!authErrors.phone} aria-describedby={authErrors.phone?'phone-error':undefined} onBlur={()=>touchAuthField('phone')} onChange={e=>setAuthField('phone',e.target.value)}/>{authErrors.phone&&<small id="phone-error" className="field-error" role="alert">{authErrors.phone}</small>}</label>
     </>}
     <label>Email<input required type="email" maxLength="150" autoComplete="email" placeholder="example@mail.com" value={authForm.email} aria-invalid={!!authErrors.email} aria-describedby={authErrors.email?'email-error':undefined} onBlur={()=>touchAuthField('email')} onChange={e=>setAuthField('email',e.target.value)}/>{authErrors.email&&<small id="email-error" className="field-error" role="alert">{authErrors.email}</small>}</label>
     <label>Пароль<PasswordInput key={authMode} required minLength={authMode==='register'?8:undefined} maxLength={authMode==='register'?72:undefined} autoComplete={authMode==='login'?'current-password':'new-password'} placeholder={authMode==='register'?'Минимум 8 символов':'Ваш пароль'} value={authForm.password} aria-invalid={!!authErrors.password} aria-describedby={authErrors.password?'password-error':undefined} onBlur={()=>touchAuthField('password')} onChange={e=>setAuthField('password',e.target.value)}/>{authErrors.password&&<small id="password-error" className="field-error" role="alert">{authErrors.password}</small>}</label>
     {authMode==='register'&&<label>Повторите пароль<PasswordInput required minLength="8" autoComplete="new-password" placeholder="Повторите пароль" value={authForm.confirmPassword} aria-invalid={!!authErrors.confirmPassword} aria-describedby={authErrors.confirmPassword?'confirmPassword-error':undefined} onBlur={()=>touchAuthField('confirmPassword')} onChange={e=>setAuthField('confirmPassword',e.target.value)}/>{authErrors.confirmPassword&&<small id="confirmPassword-error" className="field-error" role="alert">{authErrors.confirmPassword}</small>}</label>}
     {authMessage&&<p className="form-message" role="status">{authMessage}</p>}
     <button type="submit" className="btn btn-dark full" disabled={authLoading}>{authLoading?'Подождите…':authMode==='login'?'Войти':'Зарегистрироваться'}</button>
    </form>
   </div> : <Cabinet key={authUser.id||authUser.email} user={authUser} groomers={groomers} token={authToken} logout={logout} onBook={()=>go('booking')}/>}
  </section>}

  </main>{page!=='cabinet'&&<footer><div className="wrap footer-inner"><div className="brand footer-brand"><span><span className="brand-word">Шерст<span className="brand-accent-letter">ю<svg className="brand-accent-heart" viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M12 21C10 19 2 13.5 2 7.8 2 2.5 8.4 1 12 5.5 15.6 1 22 2.5 22 7.8 22 13.5 14 19 12 21Z"/></svg></span>га</span><small>ГРУМИНГ-САЛОН</small></span></div><p>Онлайн-запись на груминг</p></div></footer>}
 </div>
}
function ServiceCard({s,onPick}){return <article className="service-card"><div className="service-photo image-placeholder">{s.imageUrl?<img className="catalog-photo" src={photoUrl(s.imageUrl)} alt={s.name}/>:"Фото услуги"}</div><div className="card-overline">УХОД ЗА ПИТОМЦЕМ · {s.duration_minutes} МИН</div><h3>{s.name}</h3><p>{s.description}</p><div className="card-bottom"><strong>от {rub(s.price)}</strong><button onClick={()=>onPick(s.id)} aria-label={'Записаться: '+s.name}>Выбрать</button></div></article>}

createRoot(document.getElementById('root')).render(<App/>);
