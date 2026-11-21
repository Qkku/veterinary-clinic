# Кабинет мастера
Основа: Sherstyuga-frontend-schedule-fix.zip. Изменены только src/main.jsx, src/style.css; добавлены src/groomer-panel.jsx и src/groomer-month.js. Backend и SQL не менялись.

## Существующий API Даши
- GET GroomerCabinet/me — профиль (используется уже существующей авторизацией).
- GET GroomerCabinet/schedule?date=YYYY-MM-DD — одна неделя для текущего мастера; календарь объединяет 4–6 недель. Рабочие интервалы, отсутствия и записи приходят с сервера.
- GET Notifications — уведомления текущего аккаунта (до 100 последних).
- PUT Notifications/{id}/read и PUT Notifications/read-all — отметки прочтения. Проверка новых уведомлений каждые 30 секунд. В оригинальном ReviewsController уже создаётся уведомление review_created при добавлении отзыва.
- GET GroomerTimeOff — существующий список назначенных отсутствий; интерфейс показывает только периоды текущего мастера.

## Недостающий API (предложение, не реализован в оригинальном сервере)
- GET GroomerCabinet/reviews — только отзывы о мастере из JWT, роль Groomer. Массив {id, userName, rating, text, createdAt}. Если вводится модерация, согласовать видимость с Дашей. GET Reviews не подходит: он возвращает отзывы, написанные пользователем с этим ID, а не отзывы о мастере.
- GET LeaveRequests/my — только заявки текущего мастера, массив {id,startDate,endDate,reason,status}.
- POST LeaveRequests — {startDate,endDate,reason}, мастер определяется из JWT. При отсутствии первого метода форма отключается. Статусы pending/approved/rejected.
- При решении администратора сервер должен создать уведомление конкретному мастеру через NotificationService.NotifyGroomerAsync, например leave_approved / leave_rejected. На одобрение требуется согласование конфликта с записями и внесение периода в groomer_time_off.
Не добавляйте методы в Backend без согласования с Дашей. В оригинальном дампе нет таблицы заявок на отпуск. Интерфейсы не подставляют фиктивные отзывы и уведомления.

## Проверка
Синтаксис изменённых JS/JSX проверен TypeScript parser. Алгоритм календаря проверен Node.js на 48 месяцах и пересечениях интервалов. Полная сборка и проверка с живым Backend не выполнялись.
