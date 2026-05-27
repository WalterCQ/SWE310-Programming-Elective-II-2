const statusStyles = {
  Draft: {
    badge: 'border-tape-gold/75 bg-tape-gold/12 text-tape-gold',
    fill: '#C08C25',
  },
  Recorded: {
    badge: 'border-ink/55 bg-ink/5 text-ink-soft',
    fill: '#2A3554',
  },
  Reimbursed: {
    badge: 'border-stamp-green/70 bg-stamp-green/10 text-stamp-green',
    fill: '#4A9C6A',
  },
  Archived: {
    badge: 'border-ink-faint/65 bg-ink-faint/10 text-ink-muted',
    fill: '#6B7693',
  },
}

const fallbackStatus = {
  badge: 'border-pencil-plum/70 bg-pencil-plum/10 text-pencil-plum',
  fill: '#7D5CA6',
}

export function getStatusStyle(status) {
  return statusStyles[status] ?? fallbackStatus
}
