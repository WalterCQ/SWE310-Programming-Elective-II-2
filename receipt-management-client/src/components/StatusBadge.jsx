import { getStatusStyle } from '../utils/statusStyles'

export function StatusBadge({ status, className = '' }) {
  const style = getStatusStyle(status)

  return (
    <div className={`inline-flex items-center rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 px-3 py-1.5 text-base leading-5 ${style.badge} ${className}`}>
      {status ?? 'Unknown'}
    </div>
  )
}
