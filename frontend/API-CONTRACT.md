# Реальные API-маршруты объединённой версии
- POST Users/register; POST Auth/login (user); POST Auth/groomer-login (groomer); GET Auth/me.
- GET Services, GET Groomers — массивы.
- GET/POST Pets — name, type (dog/cat), breed, age (целое или null), notes.
- GET Schedule/available?date=YYYY-MM-DD&serviceId=1 — {date,serviceId,slots:[{time,groomers:[{id,name}]}]}.
- POST Appointments — petId, serviceId, groomerId, startDatetime (YYYY-MM-DDTHH:mm:ss), comment.
- GET Appointments (свои), GET Appointments/all (Admin), DELETE Appointments/{id} (Customer), PUT Appointments/{id}/status (Admin, {status}).
- GET GroomerCabinet/schedule?date=... — {weekStart,weekEnd,days:[{date,schedules,timeOffs,appointments}]}.
- GET/POST Reviews (POST {appointmentId,rating,text}); GET Reviews/pending, PATCH Reviews/{id}/moderate {status:approved|rejected}.
- GET LeaveRequests/my, POST LeaveRequests {startDate,endDate,reason}; GET LeaveRequests, PATCH LeaveRequests/{id}/decision {status:approved|rejected}.
- GET Schedule/admin?date=... — смены, отсутствие, активные записи.
Все закрытые запросы используют Authorization: Bearer JWT. Последние методы модерации, отпусков и admin schedule добавлены в backend этой поставки; обязательна миграция database/upgrade.sql.

### Единый вход и календарь администратора (обновление)
`POST /api/Auth/login` принимает `{email,password}` для всех ролей. Ответ сохраняет существующую форму `token,user` либо `token,groomer`. Роль определяет сервер, `GET /api/Auth/me` восстанавливает профиль. Регистрация создаёт только клиента.
`GET /api/Schedule/admin?date=YYYY-MM-DD` (Admin): массив строк с `groomerId`, `groomerName`, `startTime`, `endTime`, `kind` (`shift`, `leave`, `appointment`); для `shift` дополнительно `slotDurationMinutes`. Календарь запрашивает семь суток, не меняет серверный график.
