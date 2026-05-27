import { Inbox } from 'lucide-react'
import { Panel } from './Panel'

export function EmptyState({ action, icon: Icon = Inbox, message, title }) {
  return (
    <Panel className="grid min-h-52 place-items-center text-center">
      <div className="max-w-md space-y-4">
        <div className="mx-auto grid h-14 w-14 place-items-center rounded-[3px_9px_4px_10px/8px_3px_10px_4px] border-2 border-dashed border-ink/35 bg-paper-soft text-ink-muted">
          <Icon size={28} />
        </div>
        <div className="space-y-2">
          <h2 className="font-title text-4xl font-bold leading-none text-ink">{title}</h2>
          <p className="text-lg leading-7 text-ink-soft">{message}</p>
        </div>
        {action ? <div className="flex justify-center">{action}</div> : null}
      </div>
    </Panel>
  )
}
