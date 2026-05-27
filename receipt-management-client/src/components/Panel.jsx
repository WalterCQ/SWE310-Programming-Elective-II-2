export function Panel({ children, className = '' }) {
  return (
    <section
      className={`rounded-[4px_11px_5px_13px/10px_4px_12px_5px] border-2 border-ink/70 bg-paper-card/95 p-4 text-ink shadow-[2px_3px_0_rgba(27,37,64,0.18),4px_8px_20px_rgba(27,37,64,0.10)] ${className}`}
    >
      {children}
    </section>
  )
}
