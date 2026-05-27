export function ErrorBanner({ error }) {
  if (!error) {
    return null
  }

  const entries = error.errors ? Object.entries(error.errors) : []

  return (
    <div className="rounded-[4px_10px_5px_12px/9px_4px_11px_5px] border-2 border-dashed border-pencil-red bg-pencil-red/10 p-4 text-left text-base leading-6 text-ink shadow-[2px_3px_0_rgba(214,57,74,0.14)]">
      <p className="text-pencil-red">{error.message}</p>
      {entries.length > 0 ? (
        <ul className="mt-3 space-y-1">
          {entries.map(([field, messages]) => (
            <li key={field}>
              <span className="text-ink-muted">{field}</span>: {Array.isArray(messages) ? messages.join(', ') : String(messages)}
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  )
}
