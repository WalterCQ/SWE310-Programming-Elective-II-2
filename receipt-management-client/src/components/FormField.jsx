export function FormField({ label, error, children }) {
  return (
    <label className="block space-y-2 text-left">
      <span className="block text-base leading-5 text-ink-muted">{label}</span>
      {children}
      {error ? <span className="block text-sm leading-5 text-pencil-red">{error}</span> : null}
    </label>
  )
}

export const inputClass =
  'w-full rounded-[2px_7px_3px_8px/7px_2px_8px_3px] border-2 border-ink/45 bg-paper-soft/80 px-3 py-2.5 text-lg leading-6 text-ink outline-none transition placeholder:text-ink-faint focus:border-pencil-red focus:bg-paper-card focus:ring-2 focus:ring-pencil-red/15'

export const selectClass = `${inputClass} appearance-none`

export const textareaClass = `${inputClass} min-h-28 resize-y`
