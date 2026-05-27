export function StatCard({ label, value, tone = 'cyan' }) {
  const tones = {
    cyan: 'border-ink/65 shadow-[2px_3px_0_rgba(27,37,64,0.18)]',
    red: 'border-pencil-red/70 shadow-[2px_3px_0_rgba(214,57,74,0.16)]',
    amber: 'border-tape-gold/75 shadow-[2px_3px_0_rgba(192,140,37,0.18)]',
    green: 'border-stamp-green/70 shadow-[2px_3px_0_rgba(74,156,106,0.18)]',
  }

  return (
    <div className={`rotate-[-0.25deg] rounded-[4px_10px_5px_12px/9px_4px_11px_5px] border-2 bg-paper-card/95 p-4 text-ink ${tones[tone]}`}>
      <p className="text-base leading-5 text-ink-muted">{label}</p>
      <p className="mt-3 break-words font-title text-[2.4rem] font-bold leading-none text-ink xl:text-[2.65rem]">{value}</p>
    </div>
  )
}
