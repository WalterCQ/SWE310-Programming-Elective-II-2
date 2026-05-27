export function LoadingState({ label = 'Loading data...' }) {
  return (
    <div className="grid min-h-56 place-items-center rounded-[4px_11px_5px_13px/10px_4px_12px_5px] border-2 border-dashed border-ink/40 bg-paper-card/80">
      <div className="space-y-4 text-center">
        <div className="mx-auto h-12 w-12 animate-spin rounded-[45%_55%_50%_50%] border-2 border-ink/30 border-t-pencil-red" />
        <p className="text-base leading-5 text-ink-muted">{label}</p>
      </div>
    </div>
  )
}
