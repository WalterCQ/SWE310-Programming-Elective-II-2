import {
  Briefcase,
  Car,
  GraduationCap,
  HeartPulse,
  Monitor,
  Plane,
  Receipt,
  ShoppingBag,
  Ticket,
  Utensils,
  Zap,
} from 'lucide-react'

const icons = {
  briefcase: Briefcase,
  car: Car,
  'graduation-cap': GraduationCap,
  'heart-pulse': HeartPulse,
  monitor: Monitor,
  plane: Plane,
  receipt: Receipt,
  'shopping-bag': ShoppingBag,
  ticket: Ticket,
  utensils: Utensils,
  zap: Zap,
}

export function CategoryIcon({ iconName, color = '#1B2540', label, className = '' }) {
  const Icon = icons[iconName] ?? Receipt

  return (
    <div
      aria-label={label ?? iconName ?? 'Category icon'}
      className={`grid h-12 w-12 shrink-0 place-items-center rounded-[3px_8px_4px_9px/7px_3px_9px_4px] border-2 border-ink/45 bg-paper-soft/80 ${className}`}
      role="img"
      style={{ color }}
    >
      <Icon size={24} strokeWidth={2.4} />
    </div>
  )
}
