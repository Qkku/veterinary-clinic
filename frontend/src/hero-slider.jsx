import React, {useEffect, useState} from 'react';
import first from './assets/pets-1.jpg';
import second from './assets/pets-2.jpg';
import third from './assets/pets-3.jpg';
const slides=[{src:first,alt:'Кошка и собака спят вместе'},{src:second,alt:'Собака обнимает кошку'},{src:third,alt:'Кот и улыбающаяся собака'}];
export default function HeroSlider(){
 const [index,setIndex]=useState(0);
 const [hover,setHover]=useState(false);
 const [focus,setFocus]=useState(false);
 const [paused,setPaused]=useState(()=>window.matchMedia('(prefers-reduced-motion: reduce)').matches);
 const move=step=>setIndex(i=>(i+step+slides.length)%slides.length);
 useEffect(()=>{if(paused||hover||focus)return;const timer=setTimeout(()=>setIndex(i=>(i+1)%slides.length),5000);return()=>clearTimeout(timer);},[index,hover,focus,paused]);
 return <div className="home-slider" role="region" aria-label="Фотографии питомцев" onMouseEnter={()=>setHover(true)} onMouseLeave={()=>setHover(false)} onFocus={()=>setFocus(true)} onBlur={e=>{if(!e.currentTarget.contains(e.relatedTarget))setFocus(false)}}>
  <div className="home-slider-frame">{slides.map((s,i)=><img key={s.src} src={s.src} alt={s.alt} aria-hidden={i!==index} className={i===index?'is-current':''}/>)}
   <button type="button" className="home-slider-arrow prev" aria-label="Предыдущая фотография" onClick={()=>move(-1)}>‹</button>
   <button type="button" className="home-slider-arrow next" aria-label="Следующая фотография" onClick={()=>move(1)}>›</button>
  </div>
  <div className="home-slider-controls">{slides.map((s,i)=><button type="button" key={s.src} className={'home-slider-dot '+(i===index?'is-current':'')} aria-label={'Фотография '+(i+1)} aria-current={i===index?'true':undefined} onClick={()=>setIndex(i)}/>)}<button type="button" className="home-slider-pause" aria-label={paused?'Запустить слайдер':'Приостановить слайдер'} onClick={()=>setPaused(v=>!v)}>{paused?'▶':'Ⅱ'}</button></div>
 </div>;
}
