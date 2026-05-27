import { X } from 'lucide-react'
import { PixelButton } from './PixelButton'

export function ConfirmDialog({ confirming = false, open, title, message, onCancel, onConfirm }) {
  if (!open) {
    return null
  }

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-ink/45 p-4 backdrop-blur-sm">
      <div className="w-full max-w-lg rotate-[-0.4deg] rounded-[4px_11px_5px_13px/10px_4px_12px_5px] border-2 border-pencil-red bg-paper-card p-5 text-ink shadow-[3px_4px_0_rgba(27,37,64,0.18),8px_18px_40px_rgba(27,37,64,0.20)]">
        <div className="flex items-start justify-between gap-4">
          <div className="space-y-3">
            <p className="text-base leading-5 tracking-[0.18em] text-pencil-red">confirm action</p>
            <h2 className="font-title text-4xl font-bold leading-none">{title}</h2>
          </div>
          <button className="rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-ink/35 p-2 text-ink-muted hover:border-ink hover:text-ink disabled:cursor-not-allowed disabled:opacity-60" disabled={confirming} type="button" onClick={onCancel}>
            <X size={18} />
          </button>
        </div>
        <p className="mt-5 text-lg leading-7 text-ink-soft">{message}</p>
        <div className="mt-6 flex flex-wrap justify-end gap-3">
          <PixelButton disabled={confirming} variant="ghost" onClick={onCancel}>
            Cancel
          </PixelButton>
          <PixelButton disabled={confirming} variant="danger" onClick={onConfirm}>
            {confirming ? 'Deleting' : 'Delete'}
          </PixelButton>
        </div>
      </div>
    </div>
  )
}
