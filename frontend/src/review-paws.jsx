import React from 'react';

/** Read-only rating, matching the violet paw scale used in the client cabinet. */
export default function ReviewPaws({rating}) {
  const value = Number(rating);
  if (!Number.isFinite(value) || value < 1 || value > 5) return <span className="hint">Нет оценки</span>;
  return <span className="review-paws-display" role="img" aria-label={`Оценка ${value} из 5`}>
    {[1, 2, 3, 4, 5].map(n => <svg key={n} viewBox="0 0 32 32" width="24" height="24" fill="currentColor" className={n <= value ? 'selected' : ''} aria-hidden="true">
      <ellipse cx="6" cy="13" rx="3.5" ry="5" transform="rotate(-24 6 13)"/>
      <ellipse cx="12.5" cy="7.5" rx="3.5" ry="5" transform="rotate(-8 12.5 7.5)"/>
      <ellipse cx="20" cy="7.5" rx="3.5" ry="5" transform="rotate(8 20 7.5)"/>
      <ellipse cx="26" cy="13" rx="3.5" ry="5" transform="rotate(24 26 13)"/>
      <path d="M16 14.5c-4 0-5.2 3.7-8.5 6.2C2 25 6.5 30 11 28.5c2-.7 3.4-1.5 5-1.5s3 .8 5 1.5C25.5 30 30 25 24.5 20.7 21.2 18.2 20 14.5 16 14.5Z"/>
    </svg>)}
  </span>;
}
