import React from 'react';
const labels={booked:'Запланировано',confirmed:'Подтверждено',completed:'Завершено',cancelled:'Отменено',no_show:'Неявка на приём',pending:'На рассмотрении',approved:'Одобрено',rejected:'Отклонено',active:'Активен',inactive:'Неактивен',banned:'Заблокирован'};
export function statusKey(value){const key=String(value??'').trim().toLowerCase();return key==='canceled'?'cancelled':key;}
export default function StatusLabel({value,context}){const key=statusKey(value);const label=key==='approved'&&context==='review'?'Опубликовано':labels[key]||'Статус не указан';return <span className={`status-label status-${Object.hasOwn(labels,key)?key:'unknown'}`}>{label}</span>;}
export function canMarkNoShow(row,now=new Date()){const start=new Date(row.startDatetime);return ['booked','confirmed','completed'].includes(statusKey(row.status))&&Number.isFinite(start.getTime())&&start<=now;}
