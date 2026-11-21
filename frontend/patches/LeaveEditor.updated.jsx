// Заменяет существующую функцию LeaveEditor в полном admin-panel.jsx.
// Оставьте импорты и остальную админ-панель без изменений.
function LeaveEditor({save,busy}) {
    const date = new Date();
    const originalDay = date.getDate();
    date.setHours(0, 0, 0, 0);
    date.setDate(1);
    date.setMonth(date.getMonth() + 3);
    const lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0).getDate();
    date.setDate(Math.min(originalDay, lastDay));

    const minDate = [
        date.getFullYear(),
        String(date.getMonth() + 1).padStart(2, '0'),
        String(date.getDate()).padStart(2, '0')
    ].join('-');

    const [from,F] = useState(minDate);
    const [to,T] = useState(minDate);
    const [reason,R] = useState('');

    return <form onSubmit={e => {
        e.preventDefault();
        if (from < minDate || to < from) return;
        save({
            startDatetime: `${from}T00:00:00`,
            // Последний день включительно: так дата корректно отображается в календаре.
            endDatetime: `${to}T23:59:59`,
            type: 'vacation',
            comment: reason.trim()
        });
    }}>
        <fieldset disabled={busy} className="plain-fieldset">
            <label>Начало
                <input type="date" required min={minDate} value={from}
                    onChange={e => {
                        F(e.target.value);
                        if (to < e.target.value) T(e.target.value);
                    }}/>
            </label>
            <label>Последний день отпуска
                <input type="date" required min={from} value={to}
                    onChange={e => T(e.target.value)}/>
            </label>
            <label>Причина / комментарий
                <textarea maxLength={500} value={reason}
                    onChange={e => R(e.target.value)}/>
            </label>
            <button className="btn btn-dark" disabled={from < minDate || to < from}>
                Назначить отпуск
            </button>
        </fieldset>
    </form>;
}
